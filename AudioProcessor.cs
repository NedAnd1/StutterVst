using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;
using System.Threading.Tasks;

namespace StutterVst
{
	internal sealed class AudioProcessor: IVstPluginAudioProcessor, IVstPluginBypass, IDisposable
	{
		public const int DefaultTailSize= 0;
		public const int DefaultBlockSize= 512;
		public const int DefaultChannelCount= 2;
		public const float DefaultSampleRate= 44100.0f;

		bool bypass;
		int tailSize;
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
			sampleRate= DefaultSampleRate;
			inputChannelCount= DefaultChannelCount;
			outputChannelCount= DefaultChannelCount;
		}

		private int repetitionOffset= 0;
		private int lastStutterDuration= 0;
		private int remainingRepetitions= 0;
		private float currentStutterInterval= -1;
		private DynamicBuffer buffer= new DynamicBuffer(DefaultChannelCount);
		private static float _getVolumeBias(float sampleA, float sampleB, float lastSampleA, float lastSampleB)
		{
			lastSampleA-= sampleA;
			lastSampleB-= sampleB;
			if ( sampleA < 0 )
				sampleA= -sampleA;
			if ( sampleB < 0 )
				sampleB= -sampleB;
			float distanceA=  lastSampleA < 0 ?  -lastSampleA : lastSampleA ,
				  distanceB=  lastSampleB < 0 ?  -lastSampleB : lastSampleB ;
			float sign= 0;
			if ( distanceA < sampleA )
				if ( lastSampleA < 0 )
					sign+= 0.5f;
				else sign-= 0.5f;
			if ( distanceB < sampleB )
				if ( lastSampleB < 0 )
					sign+= 0.5f;
				else sign-= 0.5f;
			double ratio= ( sampleA * distanceA + sampleB * distanceB ) / 4; // ( sampleA * distanceA * distanceA + sampleB * distanceB * distanceB ) / 8;
			sampleA+= sampleB - 0.5f;
			if ( sampleA < 0 )
				sampleA= 0;
			return sampleA * 4;// ratio > 6.31e-8 ?  (float) ( Math.Log10(ratio) * 20 + 144 ) * sign : 0 ;
		}
		private static float getVolumeBias(float sampleA, float sampleB, float lastSampleA, float lastSampleB, float threshold)
		{
			lastSampleA-= sampleA;
			lastSampleB-= sampleB;
			if ( sampleA < 0 )
				sampleA= -sampleA;
			if ( sampleB < 0 )
				sampleB= -sampleB;
			if ( lastSampleA > 0 )
				lastSampleA= 1 - lastSampleA;
			else ++lastSampleA;
			if ( lastSampleB > 0 )
				lastSampleB= 1 - lastSampleB;
			else ++lastSampleB;
			double ratio= ( ratio= Math.Sqrt( 1 - lastSampleA * lastSampleA ) ) + ( Math.Sqrt( sampleA ) * ( 1 + threshold ) - threshold ) * ( 1 - ratio )
						+ ( ratio= Math.Sqrt( 1 - lastSampleB * lastSampleB ) ) + ( Math.Sqrt( sampleB ) * ( 1 + threshold ) - threshold ) * ( 1 - ratio );
			return  (float) ratio * 8 ;
		}
		public unsafe void Process(float*[] inputChannels, float*[] outputChannels, long hostSamplePosition, int sampleCount)
		{
			int L= inputChannels.Length,
				L2= outputChannels.Length;

			if ( bypass )
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
				sampleDelta= (int)( hostSamplePosition - expectedSamplePosition );
			expectedSamplePosition= hostSamplePosition + sampleCount;
			sampleCount+= buffer.Attach(inputChannels, sampleDelta);
			if ( sampleDelta != 0 && repetitionOffset != 0 )
			{
				inputIndex= 0;
				repetitionOffset= 0;
				remainingRepetitions = 0;
				currentStutterInterval= (int)( ApplicationBase.StutterInterval.Value * sampleRate );
			}
			while ( outputIndex < blockSize )
			{
				//System.Diagnostics.Debug.Assert( inputIndex >= 0 && inputIndex < sampleCount);

				buffer.Seek(inputIndex);
				for ( int i= 0; i < L; ++i )
					outputChannels[i][ outputIndex ]= buffer[i];
				++outputIndex;

				if ( remainingRepetitions <= 0 )
					if ( --currentStutterInterval <= 0 )
					{
						repetitionOffset= 0;
						remainingRepetitions= (int) ApplicationBase.ConsecutiveRepetitions.Value;
						lastStutterDuration= (int)( ApplicationBase.StutterDuration.Value * sampleRate );
					}
				else currentStutterInterval-=
						getVolumeBias(buffer[0], buffer[1], buffer[0,inputIndex>0?inputIndex-1:0], buffer[1,inputIndex>0?inputIndex-1:0], creator.thresholdParameter);

				if ( remainingRepetitions > 0 && ++repetitionOffset >= lastStutterDuration )
				{
					--remainingRepetitions;
					inputIndex-= repetitionOffset - 1;
					repetitionOffset= 0;
					if ( remainingRepetitions == 0 )
						currentStutterInterval= (int)( ApplicationBase.StutterInterval.Value * sampleRate );
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
			buffer.Detach(inputIndex-repetitionOffset, sampleCount);
			tailSize= buffer.Capacity;
			
		}

		/*
		private int internalBufferSize= 0;
		private int internalBufferStart= 0;
		private bool debugPanic= false;
		private float[][] internalBuffer; 
		private const int internalBufferCapacity= (int)( DefaultSampleRate * 240 );
		private unsafe float*[] internalBufferPointers= null; 
		private unsafe float*[] floatPointers= new float* [ DefaultChannelCount ];
		public unsafe void Process(float*[] inputChannels, float*[] outputChannels, long hostSamplePosition, int sampleCount)
		{
			int L= inputChannels.Length,
				L2= outputChannels.Length;

			if ( bypass )
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

			// ToDo: stutter effect process
			// at a randomized interval,
			// copy a random but small portion of input to output a small but random number of times
			// store leftover input in internal buffer which will undergo same process as it's transferred to output
			if ( currentStutterInterval < 0 )
				currentStutterInterval= (int)( ApplicationBase.StutterInterval.Value * sampleRate );

			#region Update `internalBufferPointers` and `currentInputs`

				if ( floatPointers.Length < L )
					floatPointers= new float* [ L ];
				float*[] currentInputs= floatPointers;

				if ( internalBuffer == null || internalBuffer.Length < L )
				{
					internalBuffer= new float [ L ] [];
					internalBufferPointers= new float* [ L ];
					for ( int c= 0; c < L; ++c )
						fixed ( float* buffer= internalBuffer[c]= new float [ internalBufferCapacity ] )
						{
							internalBufferPointers[c]= buffer;
							currentInputs[c]= buffer + internalBufferStart;
						}
				}
				else for ( int c= 0; c < L; ++c )
						currentInputs[c]= internalBufferPointers[c] + internalBufferStart;

			#endregion

			// if ( L <= 0 ) return ;

			float* currentInputA= currentInputs[0],
				   bufferStopA= currentInputs[0] + internalBufferSize,
				   bufferEndA= internalBufferPointers[0] + internalBufferCapacity;
			if ( bufferStopA > bufferEndA )
				bufferStopA-= internalBufferCapacity;
			int inputIndex= 0,
				outputIndex= 0;
			int remainingRepetitions= 0;
			while ( outputIndex < blockSize )
			{
				if ( inputIndex < 0
				if ( inputIndex > bufferSize )
					inp
				if ( currentInputA == bufferStopA )
				{
					if ( currentInputs != inputChannels )
					{
						internalBufferSize= 0;
						internalBufferStart= 0;
						currentInputs= inputChannels;
						currentInputA= currentInputs[0];
						bufferStopA= currentInputA + sampleCount;
					}
					else {

						break;
					}

				}
				else if ( currentInputA == bufferEndA )
				{
					currentInputs[0]= currentInputA= internalBufferPointers[0];
					for ( int c= 1; c < L; ++c )
						currentInputs[c]= internalBufferPointers[c];
					inputIndex= 0;
				}

				for ( int c= 0; c < L; ++c )
					outputChannels[c][ outputIndex ]= currentInputs[c][ inputIndex ];
			}
			if ( currentStutterInterval == 0 ) // stuttering at end of buffer
			{
			}
			if ( currentInputs != inputChannels )
			{
				internalBufferSize-= inputIndex;
				internalBufferStart+= inputIndex;
			}
			else {
				internalBufferSize= 0;
				internalBufferStart= 0;
			}
			for ( int c= 0; c < L; ++c )
			{
			}
			if ( currentInputs == inputChannels )
			{
			}
			else {
				internalBufferStart= currentInputA
			}

			fixed ( float* buffer= internalBuffer[0] )
			{
				float* currentInput= buffer + internalBufferStart,
					   bufferEnd= buffer + maxDelay,
					   bufferStop= currentInput + internalBufferSize;
				float*[] currentInputs= floatPointers;
				for ( int c= 0; c < L; ++c )
					fixed ( currentInputs[c]= internalBuffer[c] + internalBufferStart )
					{
					}
				if ( bufferStop > bufferEnd )
					bufferStop-= maxDelay;
				while ( true )
				{

					if ( ++currentInput == bufferEnd )
						currentInput
				}

			}
			// ...
			sampleCount+= internalBufferSize;
			for ( int i= internalBufferStart; i < internalBufferSize &&
			{

			}

			int k2= internalBufferSize;
			for ( int i= 0; i < inputChannels.Length; ++i )
				fixed ( float* currentBuffer= internalBuffer[i] )
				{
					float* currentInputChannel= inputChannels[i],
						   currentOutputChannel= outputChannels[i];
					int j= 0,
						k= 0;
					k2= internalBufferSize;
					for ( j= 0; j < internalBufferSize && k < blockSize; ++j, k+= 2 )
					{
						currentOutputChannel[k]= currentBuffer[j];
						if(k+1<blockSize)currentOutputChannel[k+1]= 0;
					}
					for ( k2= 0; j < internalBufferSize; ++j, ++k2 )
						currentBuffer[k2]= currentBuffer[j]; // move rest of internal buffer to beginning og
					for ( j= 0; j < sampleCount; ++j, k+= 2 )
					{
						if ( k > blockSize  )
						{
							if ( k2 < maxDelay )
							{
								currentBuffer[k2]= currentInputChannel[j];
								++k2;
							}
							else if ( ! debugPanic )
							{
								debugPanic= true;
								System.Windows.MessageBox.Show("INTERNAL BUFFER FULL!");
							}
						}
						else {
							currentOutputChannel[k]= currentInputChannel[j];
							if(k+1<blockSize)currentOutputChannel[k+1]= 0;
						}

					}
					if ( i == 0 )
						tailSize+= k - j;
					
				}
			internalBufferSize= k2;

		}
		*/

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
			else throw new NotImplementedException();
			int sampleCount= 0,
				L= inputChannels.Length,
				L2= outputChannels.Length;
			if ( inputIntermediary == null || inputIntermediary.Length != L )
				inputIntermediary= new float* [ L ];
			if ( outputIntermediary == null || outputIntermediary.Length != L2 )
				outputIntermediary= new float* [ L2 ];
			for ( int i= 0; i < L; ++i )
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
