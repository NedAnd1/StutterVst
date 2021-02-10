using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;
using System.Threading.Tasks;

namespace StutterVst
{
	/// <summary>
	///  This class is where the Stutter Magic happens.
	///  <para>The `Process` method is called by the VST host to apply our effect on the input audio.</para>
	/// </summary>
	internal sealed class AudioProcessor: IVstPluginAudioProcessor, IVstPluginBypass, IDisposable
	{
		/// <summary>
		///  The tail size lets the VST host know how long the output audio will continue after the last input sample is given.
		/// </summary>
		public const int DefaultTailSize= 0;
		public const int DefaultBlockSize= 512;
		public const int DefaultChannelCount= 2;
		public const float DefaultSampleRate= 44100.0f;

		bool bypass;
		int tailSize; // our internal buffer's capacity will provide an estimate of how many extra samples we'll be giving back to the VST host
		int blockSize;
		float sampleRate;
		int inputChannelCount;
		int outputChannelCount;
		ApplicationBase creator;
		Pan currentPan;
		long expectedSamplePosition;
		public AudioProcessor(ApplicationBase creator)
		{
			this.creator= creator;
			tailSize= DefaultTailSize;
			blockSize= DefaultBlockSize;
			sampleRate= DefaultSampleRate; // the VST host sets the sample rate later on
			inputChannelCount= DefaultChannelCount;
			outputChannelCount= DefaultChannelCount;
		}

		private int repetitionOffset= 0;
		private int lastStutterDuration= 0;
		private int remainingRepetitions= 0;
		private float currentStutterInterval= -1;
		private DynamicBuffer buffer= new DynamicBuffer(DefaultChannelCount);


		/// <summary>
		///  A higher threshold causes the volume bias to be more sensitive to higher amplitudes, making stutters be more likely/frequent.
		/// </summary>
		private static float getVolumeBias(float sampleA, float sampleB, float lastSampleA, float lastSampleB, float threshold)
		{
			float sampleA_steadiness= 1 - Math.Abs( sampleA - lastSampleA ), // sharp rises or falls in amplitude tend towards 0
			      sampleB_steadiness= 1 - Math.Abs( sampleB - lastSampleB ); // minor changes in amplitude tend towards 1

			if ( sampleA < 0 )
				sampleA= -sampleA;

			if ( sampleB < 0 )
				sampleB= -sampleB;

			// higher, more unstable amplitudes drive up the 'volume bias'
			double ratio= ( ratio= Math.Sqrt( 1 - sampleA_steadiness * sampleA_steadiness ) ) + ( Math.Sqrt( sampleA ) * ( 1 + threshold ) - threshold ) * ( 1 - ratio )
						+ ( ratio= Math.Sqrt( 1 - sampleB_steadiness * sampleB_steadiness ) ) + ( Math.Sqrt( sampleB ) * ( 1 + threshold ) - threshold ) * ( 1 - ratio );

			return  (float) ratio * 8 ;
		}


		public unsafe void Process(float*[] inputChannels, float*[] outputChannels, long hostSamplePosition, int sampleCount)
		{
			int L= inputChannels.Length,
				L2= outputChannels.Length;

			if ( bypass ) // if the VST host requests a bypass, we simply copy the input samples to the output buffers
			{
				if ( L > L2 )
					L= L2;
				for ( int i= 0; i < L; ++i )
				{
					float* currentInput= inputChannels[i],
						   currentOutput= outputChannels[i],
						   currentInputEnd= currentInput + sampleCount;
					for ( ; currentInput < currentInputEnd; ++currentInput, ++currentOutput )
						*currentOutput= *currentInput;
				}
				return ;
			}

			if ( currentStutterInterval < 0 )
				currentStutterInterval= (int)( ApplicationBase.StutterInterval.Value * sampleRate );

			int inputIndex= repetitionOffset,
				outputIndex= 0,
				sampleDelta= (int)( hostSamplePosition - expectedSamplePosition ); // an unexpected jump in the playback position forwards or backwards causes a sample delta, which we may have to compensate for
			expectedSamplePosition= hostSamplePosition + sampleCount;
			sampleCount+= buffer.Attach(inputChannels, sampleDelta); // 'attaches' the input buffers to the buffer class we use internally
			if ( sampleDelta != 0 && repetitionOffset != 0 ) // if the host's playback position jumped unexpectedly, cancels any stutter repetitions in progress
			{
				inputIndex= 0;
				repetitionOffset= 0;
				remainingRepetitions = 0;
				currentStutterInterval= (int)( ApplicationBase.StutterInterval.Value * sampleRate ); // the lower this random value is, the more likely we'll trigger a stutter sooner
			}
			while ( outputIndex < blockSize )
			{
				//System.Diagnostics.Debug.Assert( inputIndex >= 0 && inputIndex < sampleCount);

				buffer.Seek(inputIndex);
				for ( int channelIndex= 0; channelIndex < L; ++channelIndex )
					outputChannels[ channelIndex ][ outputIndex ]= buffer[ channelIndex ]; // copies samples from our hybrid buffer w/ possible stutters to output buffers provided by the VST host
				++outputIndex;

				if ( remainingRepetitions <= 0 )
					if ( --currentStutterInterval <= 0 ) // we trigger on or more stutters when `currentStutterInterval` reaches zero
					{
						repetitionOffset= 0;
						remainingRepetitions= (int) ApplicationBase.ConsecutiveRepetitions.Value;
						lastStutterDuration= (int)( ApplicationBase.StutterDuration.Value * sampleRate );
					}
					else currentStutterInterval-=
							getVolumeBias(buffer[0], buffer[1], buffer[0, inputIndex-1], buffer[1, inputIndex-1], creator.thresholdParameter);

				if ( remainingRepetitions > 0 && ++repetitionOffset >= lastStutterDuration ) // if a stutter has been triggered & its duration elapsed
				{
					--remainingRepetitions; // 1 less repetition to go
					inputIndex-= repetitionOffset - 1; // moves the input back to where the stutter started
					repetitionOffset= 0;
					if ( remainingRepetitions == 0 )
						currentStutterInterval= (int)( ApplicationBase.StutterInterval.Value * sampleRate ); // determines how soon our next stutter is likely to be
				}
				else ++inputIndex;

				//if ( ( outputIndex & 1 ) == 0 )
				//	++inputIndex;
			}
			//if ( repetitionOffset > inputIndex )
			//	System.Diagnostics.Debugger.Break();
			//inputIndex-= lastStutterDuration;

			//if ( repetitionOffset > 0 || remainingRepetitions > 0 )
			//	System.Diagnostics.Debugger.Break();

			buffer.Save(inputIndex-repetitionOffset, sampleCount); // saves samples we'll need for stutters in-progress to our internal buffers
			tailSize= buffer.Capacity;
			
		}

		public struct Pan
		{
			public bool equalPower;
			public float amount;
		}

		private unsafe float*[] inputIntermediary;
		private unsafe float*[] outputIntermediary;
		private IVstHostSequencer hostSequencer;
		unsafe void IVstPluginAudioProcessor.Process(VstAudioBuffer[] inputChannels, VstAudioBuffer[] outputChannels)
		{
			long hostSamplePosition;
			var creator= this.creator as Plugin;
			if ( creator != null )
				hostSamplePosition= (long) ( hostSequencer ?? ( hostSequencer= creator.Host.GetInstance<IVstHostSequencer>() ) ).GetTime(0).SamplePosition;
			else throw new NotSupportedException(); // the VST host should create an instance of the plugin before calling the `Process` method.
			int sampleCount= 0,
				L= inputChannels.Length,
				L2= outputChannels.Length;
			if ( inputIntermediary == null || inputIntermediary.Length != L )
				inputIntermediary= new float* [ L ];
			if ( outputIntermediary == null || outputIntermediary.Length != L2 )
				outputIntermediary= new float* [ L2 ];
			for ( int i= 0; i < L; ++i ) // copies only the references to each channel's buffer over to a managed array of (unmanaged) buffers 
			{
				if ( i == 0 )
					sampleCount= inputChannels[0].SampleCount; // ToDo: make sample count the smallest sample count of all arrays?
				inputIntermediary[i]= ( (IDirectBufferAccess32)inputChannels[i] ).Buffer;
			}
			for ( int i= 0; i < L2; ++i )
				outputIntermediary[i]= ( (IDirectBufferAccess32)outputChannels[i] ).Buffer;

			if ( L > 0 && L2 > 0 )
				Process(inputIntermediary, outputIntermediary, hostSamplePosition, sampleCount);
		}



		int IVstPluginAudioProcessor.BlockSize { get { return blockSize; } set {  blockSize= value; } }

		bool IVstPluginBypass.Bypass { get { return bypass; } set { bypass= value; } }

		int IVstPluginAudioProcessor.InputCount { get { return inputChannelCount; } }

		int IVstPluginAudioProcessor.OutputCount { get { return outputChannelCount; } }

		float IVstPluginAudioProcessor.SampleRate { get { return sampleRate; } set { sampleRate= value; } }

		int IVstPluginAudioProcessor.TailSize => tailSize;

		bool IVstPluginAudioProcessor.SetPanLaw(VstPanLaw type, float gain)
		{
			currentPan.equalPower= type == VstPanLaw.EqualPowerPanLaw;
			currentPan.amount= gain;
			return true;
		}

		void IDisposable.Dispose()
		{
			buffer= null;
		}

	}
}
