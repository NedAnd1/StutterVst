﻿using System;

namespace StutterVst
{
	using static ApplicationBase.RandomParameter;

	/// <summary>
	///  The <see cref="Plugin"/> class extends this class to encapsulate application-wide parameters and data for each plugin instance.
	/// </summary>
	internal abstract class ApplicationBase
	{
		public MainView mainView;
		public AudioProcessor audioProcessor;
		public PresetsManager presetManager;

		/// <remarks>
		///  Not really used by the plugin, but could be useful in the future.
		/// </remarks>
		public volatile bool isPaused;

		/// <summary>
		///  The threshold--used by the plugin's <see cref="AudioProcessor"/>--can be changed at any time on behalf of the VST host or by the plugin itself.
		/// </summary>
		public volatile float thresholdParameter= 0.125f;

		/*
		// more extreme stutters
		public static RandomParameter StutterInterval= new RandomParameter(10, 30, BiasType.None);
		public static RandomParameter StutterDuration= new RandomParameter(1.0/24, 1, BiasType.LowerCircle);
		public static RandomParameter ConsecutiveRepetitions= new RandomParameter(1, 5, BiasType.Squared);
		*/

		/// <summary>
		///  An even distribution of random numbers between 4 and 6.
		/// </summary>
		public static RandomParameter StutterInterval= new RandomParameter(4, 6, BiasType.None);

		/// <summary>
		///  Random number between 1/24 and 1/4 (more likely to be lower).
		/// </summary>
		public static RandomParameter StutterDuration= new RandomParameter(1.0/24, 0.25, BiasType.LowerCircle);


		/// <summary>
		///  Currently always '1'.
		/// </summary>
		public static RandomParameter ConsecutiveRepetitions= new RandomParameter(1, 1, BiasType.LowerCircle);

		
		public struct RandomParameter
		{
			BiasType bias;
			double range;
			double minValue;
			private Random rGen;
			public RandomParameter(double minValue, double maxValue, BiasType bias)
			{
				this.bias= bias;
				this.minValue= minValue;
				range= maxValue - minValue;
				rGen= new Random();
			}

			/// <summary>
			///  Retrieves a randomly generated value based on the parameter's preconditions.
			/// </summary>
			public double Value
			{
				get {
					double ratio= rGen.NextDouble();
					switch ( bias )
					{
						case BiasType.Squared:
							ratio*= ratio;
							break;
						case BiasType.PositivelyCubed:
							ratio*= ratio * ratio;
							break;
						case BiasType.NegativelySquared:
							ratio= 1 - ( ratio - 1 ) * ( ratio - 1 );
							break;
						case BiasType.NegativelyCubed:
							ratio= 1 - ( ratio= 1 - ratio ) * ratio * ratio;
							break;
						case BiasType.FullyCubic:
							ratio= ( ratio= ratio*2 - 1 ) * ratio * ratio * 0.5 + 0.5;
							break;
						case BiasType.LowerCircle:
							ratio= 1 - Math.Sqrt( 1 - ratio * ratio );
							break;
						case BiasType.UpperCircle:
							ratio= Math.Sqrt( 1 - ( 1 - ratio ) * ( 1 - ratio ) );
							break;
						default:
							break;
					}
					return minValue + ratio * range;
				}
			} 

			public enum BiasType
			{
				/// <summary> Random values are evenly distributed. </summary>
				None,

				/// <summary> Random values tend toward the minimum value with a squared bias. </summary>
				Squared,

				/// <summary> Random values tend toward the maximum value with a squared bias. </summary>
				NegativelySquared,

				/// <summary> Random values tend especially toward the minimum value with an positive-cubic bias. </summary>
				PositivelyCubed,

				/// <summary> Random values tend especially toward the maximum value with a negated-cubic bias. </summary>
				NegativelyCubed,

				/// <summary> Random values tend especially toward the average value with a fully-cubic bias. </summary>
				FullyCubic,

				/// <summary> Random values tend especially toward the minimum value with a circular bias. </summary>
				LowerCircle,

				/// <summary> Random values tend especially toward the maximum value with a circular bias. </summary>
				UpperCircle

			}
		}
	}
}
