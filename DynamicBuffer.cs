﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace StutterVst {

	internal unsafe class DynamicBuffer
	{
		private int channelCount;
		private int internalStart;
		private int internalCount;
		private float* internalBase;
		private float** attachedBase;
		private int capacityExponent;
		private float* currentPosition;
		private int currentSampleIndex;
		public DynamicBuffer(int channelCount, uint initialCapacity= 1)
		{
			#region Set Capacity to Next Power of Two

				if ( initialCapacity > 1 )
				{
					--initialCapacity;
					capacityExponent= 1;
					if (  initialCapacity >> 16  ==  0  ) { capacityExponent+= 16; initialCapacity<<= 16; }
					if (  initialCapacity >> 24  ==  0  ) { capacityExponent+= 8; initialCapacity<<= 8; }
					if (  initialCapacity >> 28  ==  0  ) { capacityExponent+= 4; initialCapacity<<= 4; }
					if (  initialCapacity >> 30  ==  0  ) { capacityExponent+= 2; initialCapacity<<= 2; }
					capacityExponent= (int)( initialCapacity >> 31 ) - capacityExponent + 32;
					if ( capacityExponent > 31 )
						capacityExponent= 31;
					// initialCapacity= 1u << capacityExponent;
				}
				else {
					// initialCapacity= 1;
					capacityExponent= 0;
				}

			#endregion

			internalBase= (float*) Marshal.AllocHGlobal( sizeof(float) * channelCount << capacityExponent );
			attachedBase= (float**) Marshal.AllocHGlobal( sizeof(float*) * channelCount );
			this.channelCount= channelCount;
			currentSampleIndex= 0;
			currentPosition= null;
			internalStart= 0;
			internalCount= 0;
		}

		public int Attach(float*[] attachedBase, int sampleDelta)
		{
			if ( channelCount < (channelCount= attachedBase.Length) )
			{
				Marshal.FreeHGlobal( (IntPtr) this.attachedBase );
				this.internalBase= (float*) Marshal.ReAllocHGlobal( (IntPtr) this.internalBase, (IntPtr) ( sizeof(float) * channelCount << capacityExponent ) );
				this.attachedBase= (float**) Marshal.AllocHGlobal( sizeof(float*) * channelCount );
			}
			for ( int i= 0; i < channelCount; ++i )
				this.attachedBase[i]= attachedBase[i];
			if ( sampleDelta >= 0 && sampleDelta < internalCount )
			{
				internalCount-= sampleDelta;
				internalStart+= sampleDelta;
				if (  internalStart >> capacityExponent  !=  0  )
					internalStart-= 1 << capacityExponent;
			}
			else {
				internalStart= 0;
				internalCount= 0;
			}
			return internalCount;
		}

		/// <summary> Internally save the given input samples from `minSampleIndex` (inclusive) to `maxSampleIndex` (exclusive). </summary>
		public void Detach(int minSampleIndex, int maxSampleIndex)
		{
			internalStart+= minSampleIndex;
			minSampleIndex-= internalCount;
			maxSampleIndex-= internalCount; // min & max indices are now with respect to the attached buffer
			internalCount= maxSampleIndex - minSampleIndex;
			if ( internalCount < 0 )
				internalCount= 0;
			if ( minSampleIndex >= 0 ) 
				#region Overwrite Internal Buffer with Attached Buffer
			{
				internalStart= 0;
				if (  internalCount-1 >> capacityExponent  >  0  ) // increase buffer capacity as needed
				{
					while (  internalCount-1 >> ++capacityExponent  >  0  )
						;
					Marshal.FreeHGlobal( (IntPtr) internalBase );
					internalBase= (float*) Marshal.AllocHGlobal( sizeof(float) * channelCount << capacityExponent );
				}

				for ( int i= 0; i < channelCount; ++i )
					for ( float* attachedChannel= attachedBase[i] + minSampleIndex,
								 internalChannel= this.internalBase + ( i << capacityExponent ),
								 attachedChannelEnd= attachedChannel + internalCount
						;
							attachedChannel < attachedChannelEnd
						;
							++attachedChannel, ++internalChannel
					)
						*internalChannel= *attachedChannel; // say hello to my ugly forr loop ;)
			}
				#endregion
			else {
				#region Append Attached Buffer to End of Internal Buffer
			
					int internalBreak= 1 << capacityExponent;
						minSampleIndex= -minSampleIndex;
					if ( internalStart >= internalBreak )
						internalStart-= internalBreak;
					if ( internalCount > internalBreak )
						#region Increase Buffer Capacity
					{
						while (  internalCount-1 >> ++capacityExponent  >  0  )
							;
						float* internalBase= (float*) Marshal.AllocHGlobal( sizeof(float) * channelCount << capacityExponent );
						for ( int i= 0; i < channelCount; ++i )
						{
							float* sourceChannel= this.internalBase + internalBreak * i,
								   targetChannel= internalBase + ( i << capacityExponent ),
								   sourceChannelStart= sourceChannel + internalStart,
								   sourceChannelBreak= sourceChannel + internalBreak,
								   sourceChannelEnd= sourceChannelStart + minSampleIndex;
							if ( sourceChannelEnd > sourceChannelBreak )
							{
								for ( ; sourceChannelStart < sourceChannelBreak; ++sourceChannelStart, ++targetChannel )
									*targetChannel= *sourceChannelStart;
								sourceChannelStart= sourceChannel;
								sourceChannelEnd-= internalBreak;
							}
							for ( ; sourceChannelStart < sourceChannelEnd; ++sourceChannelStart, ++targetChannel )
								*targetChannel= *sourceChannelStart;
						}
						Marshal.FreeHGlobal( (IntPtr) this.internalBase );
						this.internalBase= internalBase;
						internalBreak= internalCount;
						internalStart= 0;
					}
						#endregion
					else {
						minSampleIndex+= internalStart; // minSampleIndex is now w/ respect to internal buffer
						if ( minSampleIndex >= internalBreak )
							minSampleIndex-= internalBreak;
						if ( minSampleIndex + maxSampleIndex < internalBreak )
							internalBreak= minSampleIndex + maxSampleIndex;
					}

					for ( int i= 0; i < channelCount; ++i )
					{
						float* attachedChannel= attachedBase[i],
							   internalChannel= this.internalBase + ( i << capacityExponent ),
							   internalChannelStart= internalChannel + minSampleIndex,
							   internalChannelBreak= internalChannel + internalBreak,
							   attachedChannelEnd= attachedChannel + maxSampleIndex;
						for ( ; internalChannelStart < internalChannelBreak; ++internalChannelStart, ++attachedChannel )
							*internalChannelStart= *attachedChannel;
						for ( internalChannelStart= internalChannel; attachedChannel < attachedChannelEnd; ++internalChannelStart, ++attachedChannel )
							*internalChannelStart= *attachedChannel;
					}

				#endregion
			}
		}

		/// <summary> Make sure `sampleIndex` is less than count returned by `Attach` plus count you attached. </summary>
		public void Seek(int sampleIndex)
		{
			if ( sampleIndex < internalCount )
			{
				sampleIndex+= internalStart;
				if (  sampleIndex >> capacityExponent  !=  0  ) // if index is greater than buffer capacity, rotate back to beginning
					sampleIndex-= 1 << capacityExponent;
				currentPosition= internalBase + sampleIndex;
			}
			else {
				currentSampleIndex= sampleIndex - internalCount;
				currentPosition= null;
			}
		}

		/// <summary> Call seek for each sample before getting its value for a specific channel! </summary>
		public float this [ int channelIndex ]
		{
			get {
				if ( currentPosition != null )
					return currentPosition [ channelIndex << capacityExponent ];
				else return attachedBase [ channelIndex ] [ currentSampleIndex ];
			}
		}

		/// <summary> Prioritize seeking to the sample instead of using this getter.  </summary>
		public float this [ int channelIndex, int sampleIndex ]
		{
			get {
				if ( sampleIndex < internalCount )
				{
					sampleIndex+= internalStart;
					if (  sampleIndex >> capacityExponent  !=  0  ) // if index is greater than buffer capacity, rotate back to beginning
						sampleIndex-= 1 << capacityExponent;
					return internalBase [  channelIndex << capacityExponent  |  sampleIndex  ] ;
				}
				else return attachedBase [ channelIndex ] [ sampleIndex - internalCount ];
			}
		} 

		/// <summary> You don't need to worry about this property except when estimating the tail size. </summary>
		public int Capacity  =>  1 << capacityExponent ;

		~DynamicBuffer()
		{
			if ( internalBase != null )
			{
				Marshal.FreeHGlobal( (IntPtr) internalBase );
				Marshal.FreeHGlobal( (IntPtr) attachedBase );
				internalBase= null;
			}
		}
	}
}
