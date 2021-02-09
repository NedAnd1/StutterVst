using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;

namespace StutterVst
{
	internal sealed class Plugin: ApplicationBase, IVstPlugin
	{
		/// <summary> Please don't make me have to track down the interfaceType all by myself. </summary>
		public static void AddVstClass<classType>(Type interfaceType= null)
		{
			vstConstructorsByInterface.Add( interfaceType ?? __findVstInterface( typeof(classType) ), VstConstructor.Get<classType>() );
		}

		/// <summary> A more reliable and explicit way of adding support for VST interfaces to a plugin. </summary>
		public static void AddVstInterface<interfaceType>(VstConstructor constructor)
		{
			vstConstructorsByInterface.Add( typeof(interfaceType), constructor );
		}

		private static Dictionary<Type, VstConstructor> vstConstructorsByInterface=
			new Dictionary<Type, VstConstructor> 
			{
				//{ typeof(IVstPluginEditor), new VstConstructor ( (Plugin creator) => creator.mainView= new MainView(creator) ) },
				{ typeof(IVstPluginAudioProcessor), new VstConstructor ( (Plugin creator) => creator.audioProcessor= new AudioProcessor(creator) ) },
				{
					typeof(IVstPluginParameters),
					new VstConstructor (
						(Plugin creator) =>  ( creator.presetManager ?? ( creator.presetManager= new PresetsManager(creator) ) ).ActiveProgram
					)
				},
				{
					typeof(IVstPluginPrograms),
					new VstConstructor (
						(Plugin creator) =>  creator.presetManager ?? ( creator.presetManager= new PresetsManager(creator) )
					)
				}
			};

		public IVstHost Host { get; private set; }

		public Plugin()
		{
		}

		#region Properties and Methods used by VST Host

			string IVstPlugin.Name => vstInfo.name;

			VstProductInfo IVstPlugin.ProductInfo => vstInfo.productInfo;

			VstPluginCategory IVstPlugin.Category => VstPluginCategory.Effect;

			VstPluginCapabilities IVstPlugin.Capabilities => VstPluginCapabilities.NoSoundInStop | VstPluginCapabilities.ReceiveTimeInfo;

			int IVstPlugin.InitialDelay => 0;

			int IVstPlugin.PluginID => 'N' << 24 | 'A' << 16 | 'S' << 8 | 'T';

			void IVstPlugin.Open(IVstHost host)
			{
				Host= host;
			}

			void IVstPlugin.Suspend()
			{
				isPaused= true;
			}

			void IVstPlugin.Resume()
			{
				isPaused= false;
			}

			T IExtensible.GetInstance<T>()
			{
				return VstInterfaceManager<T>.GetInstance(this);
			}

			bool IExtensible.Supports<T>()
			{
				return VstInterfaceManager<T>.Supported;
			}

			void IDisposable.Dispose()
			{
				Dispose( disposeAll: true );
			}

			private void Dispose(bool disposeAll)
			{
				if ( disposeAll )
				{
					Host= null;
					foreach ( object _obj in vstInstancesByInterface.Values )
					{
						var obj= _obj as IDisposable;
						if ( obj != null && obj != this )
							obj.Dispose();
					}
					vstInstancesByInterface.Clear();
					onDispose?.Invoke();
					onDispose= null;
				}
			}

		#endregion

		private Hashtable vstInstancesByInterface= new Hashtable();

		/// <summary> Represents a constructor for a VST interface that assumes it can be called concurrently by multiple plugins, but only once by a single plugin. </summary>
		public struct VstConstructor
		{
			private Delegate @base;
			public delegate object Default();
			public delegate object Extended(Plugin creator);
			public VstConstructor(Default @base)
			{
				this.@base= @base;
			}
			public VstConstructor(Extended @base)
			{
				this.@base= @base;
			}

			public object Invoke(Plugin creator)
			{
				Extended constructor= @base as Extended;
				if ( constructor != null )
					return constructor.Invoke(creator);
				else return ( @base as Default )?.Invoke();
			}

			public bool IsNull { get { return @base == null; } }

			#region Methods For Converting Delegates and ClassTypes to VstConstructors

				public static VstConstructor Get<ClassType>()
				{
					return new VstConstructor { @base= Helper<ClassType>.extended ?? Helper<ClassType>.@default };
				}
				public static implicit operator VstConstructor(Default defaultConstructor)
				{
					return new VstConstructor { @base= defaultConstructor };
				}
				public static implicit operator VstConstructor(Extended extendedConstructor)
				{
					return new VstConstructor { @base= extendedConstructor };
				}
				public static implicit operator VstConstructor(Func<object> defaultConstructor)
				{
					return new VstConstructor { @base= Delegate.CreateDelegate( typeof(Default), defaultConstructor.Target, defaultConstructor.Method ) };
				}
				public static implicit operator VstConstructor(Func<Plugin, object> extendedConstructor)
				{
					return new VstConstructor { @base= Delegate.CreateDelegate( typeof(Extended), extendedConstructor.Target, extendedConstructor.Method ) };
				}
			#endregion

			#region Helper to Dynamically Retrieve Fast Constructors

				private static Type[] defaultParameterTypes= { };
				private static Type[] extendedParameterTypes= { typeof(Plugin) };
				private static class Helper<T>
				{
					public static readonly Delegate @default;
					public static readonly Delegate extended;
					static Helper()
					{
						ILGenerator codeBuilder;
						DynamicMethod dynamicMethod;
						Type classType= typeof(T);
						ConstructorInfo ctorInfo= classType.GetConstructor(defaultParameterTypes);
						if ( ctorInfo != null )
						{
							dynamicMethod= new DynamicMethod(string.Empty, classType, defaultParameterTypes);
							codeBuilder= dynamicMethod.GetILGenerator();
							codeBuilder.Emit(OpCodes.Newobj, ctorInfo);
							codeBuilder.Emit(OpCodes.Ret);
							@default= dynamicMethod.CreateDelegate(typeof(Default));
						}
						if (  ( ctorInfo= classType.GetConstructor(extendedParameterTypes) )  !=  null  )
						{
							dynamicMethod= new DynamicMethod(string.Empty, classType, extendedParameterTypes);
							codeBuilder= dynamicMethod.GetILGenerator();
							codeBuilder.Emit(OpCodes.Ldarg_0);
							codeBuilder.Emit(OpCodes.Newobj, ctorInfo);
							codeBuilder.Emit(OpCodes.Ret);
							extended= dynamicMethod.CreateDelegate(typeof(Extended));
						}
					}
				}

			#endregion
		}

		#region Private Classes and Methods

			private static class VstInterfaceManager<T>
				where T : class // interface
			{
				private static object @lock;
				private static T firstInstance;
				private static VstConstructor @base;
				private static ApplicationBase firstCreator;
			
				static VstInterfaceManager()
				{
					@lock= new object();
					vstConstructorsByInterface.TryGetValue( typeof(T), out @base );
				}

				public static bool Supported { get { return @base.IsNull == false; } }

				public static T GetInstance(Plugin creator)
				{
					if ( creator == firstCreator )
						return firstInstance;
					else if ( firstCreator == null && Monitor.TryEnter(@lock) )
						try {
							firstCreator= creator;
							firstInstance= @base.Invoke(creator) as T;
							creator.onDispose+= onDispose;
							return firstInstance;
						}
						finally {
							Monitor.Exit(@lock);
						}
					else {
						T currentInstance= creator.vstInstancesByInterface[typeof(T)] as T;
						if ( currentInstance == null )
							creator.vstInstancesByInterface[typeof(T)]= currentInstance= @base.Invoke(creator) as T;
						return currentInstance;
					}
				}

				private static void onDispose()
				{
					( firstInstance as IDisposable )?.Dispose();
					firstInstance= null;
					firstCreator= null;
				}
			}

			private Action onDispose;

			private sealed class vstInfo: VstProductInfo
			{
				private static readonly Assembly assembly= typeof(Plugin).Assembly;
				public static readonly string name= ( Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute )?.Title; 
				public static readonly string product= ( Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute)) as AssemblyProductAttribute )?.Product; 
				public static readonly string company= ( Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute )?.Company; 
				public static readonly string version= ( Attribute.GetCustomAttribute(assembly, typeof(AssemblyVersionAttribute)) as AssemblyVersionAttribute )?.Version;
				public static readonly VstProductInfo productInfo= new vstInfo(); 
				public vstInfo() : base(product, company, 1) {}
				protected override string FormatVersion(int _) { return version; }
			}

			private static Type __findVstInterface(Type classType)
			{
				Type[] interfaceList= classType.GetInterfaces();
				for ( int i= 0; i < interfaceList.Length; ++i )
				{
					Type vstInterface= interfaceList[0];
					if ( vstInterface.Name.StartsWith("IVst", StringComparison.OrdinalIgnoreCase) )
						return vstInterface;
				}
				for ( int i= 0; i < interfaceList.Length; ++i )
				{
					Type vstInterface= interfaceList[0];
					if ( vstInterface.FullName.StartsWith("Jacobi.Vst", StringComparison.OrdinalIgnoreCase) )
						return vstInterface;
				}
				return null;
			}

		#endregion

	}
}
