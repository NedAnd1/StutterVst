<a name='assembly'></a>
# StutterVst

## Contents

- [ApplicationBase](#T-StutterVst-ApplicationBase 'StutterVst.ApplicationBase')
  - [ConsecutiveRepetitions](#F-StutterVst-ApplicationBase-ConsecutiveRepetitions 'StutterVst.ApplicationBase.ConsecutiveRepetitions')
  - [StutterDuration](#F-StutterVst-ApplicationBase-StutterDuration 'StutterVst.ApplicationBase.StutterDuration')
  - [StutterInterval](#F-StutterVst-ApplicationBase-StutterInterval 'StutterVst.ApplicationBase.StutterInterval')
  - [isPaused](#F-StutterVst-ApplicationBase-isPaused 'StutterVst.ApplicationBase.isPaused')
  - [thresholdParameter](#F-StutterVst-ApplicationBase-thresholdParameter 'StutterVst.ApplicationBase.thresholdParameter')
- [AudioProcessor](#T-StutterVst-AudioProcessor 'StutterVst.AudioProcessor')
  - [DefaultTailSize](#F-StutterVst-AudioProcessor-DefaultTailSize 'StutterVst.AudioProcessor.DefaultTailSize')
  - [getVolumeBias()](#M-StutterVst-AudioProcessor-getVolumeBias-System-Single,System-Single,System-Single,System-Single,System-Single- 'StutterVst.AudioProcessor.getVolumeBias(System.Single,System.Single,System.Single,System.Single,System.Single)')
- [BiasType](#T-StutterVst-ApplicationBase-RandomParameter-BiasType 'StutterVst.ApplicationBase.RandomParameter.BiasType')
  - [FullyCubic](#F-StutterVst-ApplicationBase-RandomParameter-BiasType-FullyCubic 'StutterVst.ApplicationBase.RandomParameter.BiasType.FullyCubic')
  - [LowerCircle](#F-StutterVst-ApplicationBase-RandomParameter-BiasType-LowerCircle 'StutterVst.ApplicationBase.RandomParameter.BiasType.LowerCircle')
  - [NegativelyCubed](#F-StutterVst-ApplicationBase-RandomParameter-BiasType-NegativelyCubed 'StutterVst.ApplicationBase.RandomParameter.BiasType.NegativelyCubed')
  - [NegativelySquared](#F-StutterVst-ApplicationBase-RandomParameter-BiasType-NegativelySquared 'StutterVst.ApplicationBase.RandomParameter.BiasType.NegativelySquared')
  - [None](#F-StutterVst-ApplicationBase-RandomParameter-BiasType-None 'StutterVst.ApplicationBase.RandomParameter.BiasType.None')
  - [PositivelyCubed](#F-StutterVst-ApplicationBase-RandomParameter-BiasType-PositivelyCubed 'StutterVst.ApplicationBase.RandomParameter.BiasType.PositivelyCubed')
  - [Squared](#F-StutterVst-ApplicationBase-RandomParameter-BiasType-Squared 'StutterVst.ApplicationBase.RandomParameter.BiasType.Squared')
  - [UpperCircle](#F-StutterVst-ApplicationBase-RandomParameter-BiasType-UpperCircle 'StutterVst.ApplicationBase.RandomParameter.BiasType.UpperCircle')
- [DynamicBuffer](#T-StutterVst-DynamicBuffer 'StutterVst.DynamicBuffer')
  - [attachedBase](#F-StutterVst-DynamicBuffer-attachedBase 'StutterVst.DynamicBuffer.attachedBase')
  - [capacityExponent](#F-StutterVst-DynamicBuffer-capacityExponent 'StutterVst.DynamicBuffer.capacityExponent')
  - [currentSampleIndex](#F-StutterVst-DynamicBuffer-currentSampleIndex 'StutterVst.DynamicBuffer.currentSampleIndex')
  - [internalPosition](#F-StutterVst-DynamicBuffer-internalPosition 'StutterVst.DynamicBuffer.internalPosition')
  - [Capacity](#P-StutterVst-DynamicBuffer-Capacity 'StutterVst.DynamicBuffer.Capacity')
  - [Item](#P-StutterVst-DynamicBuffer-Item-System-Int32- 'StutterVst.DynamicBuffer.Item(System.Int32)')
  - [Item](#P-StutterVst-DynamicBuffer-Item-System-Int32,System-Int32- 'StutterVst.DynamicBuffer.Item(System.Int32,System.Int32)')
  - [Attach()](#M-StutterVst-DynamicBuffer-Attach-System-Single*[],System-Int32- 'StutterVst.DynamicBuffer.Attach(System.Single*[],System.Int32)')
  - [Save()](#M-StutterVst-DynamicBuffer-Save-System-Int32,System-Int32- 'StutterVst.DynamicBuffer.Save(System.Int32,System.Int32)')
  - [Seek()](#M-StutterVst-DynamicBuffer-Seek-System-Int32- 'StutterVst.DynamicBuffer.Seek(System.Int32)')
- [MainView](#T-StutterVst-MainView 'StutterVst.MainView')
  - [InitializeComponent()](#M-StutterVst-MainView-InitializeComponent 'StutterVst.MainView.InitializeComponent')
- [Plugin](#T-StutterVst-Plugin 'StutterVst.Plugin')
  - [vstConstructorsByInterface](#F-StutterVst-Plugin-vstConstructorsByInterface 'StutterVst.Plugin.vstConstructorsByInterface')
  - [vstInstancesByInterface](#F-StutterVst-Plugin-vstInstancesByInterface 'StutterVst.Plugin.vstInstancesByInterface')
  - [AddVstClass\`\`1()](#M-StutterVst-Plugin-AddVstClass``1-System-Type- 'StutterVst.Plugin.AddVstClass``1(System.Type)')
  - [AddVstInterface\`\`1()](#M-StutterVst-Plugin-AddVstInterface``1-StutterVst-Plugin-VstConstructor- 'StutterVst.Plugin.AddVstInterface``1(StutterVst.Plugin.VstConstructor)')
- [PresetsManager](#T-StutterVst-PresetsManager 'StutterVst.PresetsManager')
- [RandomParameter](#T-StutterVst-ApplicationBase-RandomParameter 'StutterVst.ApplicationBase.RandomParameter')
  - [Value](#P-StutterVst-ApplicationBase-RandomParameter-Value 'StutterVst.ApplicationBase.RandomParameter.Value')
- [RequestHandler](#T-StutterVst-RequestHandler 'StutterVst.RequestHandler')
- [Resources](#T-StutterVst-Properties-Resources 'StutterVst.Properties.Resources')
  - [Culture](#P-StutterVst-Properties-Resources-Culture 'StutterVst.Properties.Resources.Culture')
  - [ResourceManager](#P-StutterVst-Properties-Resources-ResourceManager 'StutterVst.Properties.Resources.ResourceManager')
- [VstConstructor](#T-StutterVst-Plugin-VstConstructor 'StutterVst.Plugin.VstConstructor')
- [VstInterfaceManager\`1](#T-StutterVst-Plugin-VstInterfaceManager`1 'StutterVst.Plugin.VstInterfaceManager`1')

<a name='T-StutterVst-ApplicationBase'></a>
## ApplicationBase `type`

##### Namespace

StutterVst

##### Summary

The [Plugin](#T-StutterVst-Plugin 'StutterVst.Plugin') class extends this class to encapsulate application-wide parameters and data for each plugin instance.

<a name='F-StutterVst-ApplicationBase-ConsecutiveRepetitions'></a>
### ConsecutiveRepetitions `constants`

##### Summary

Currently always '1'.

<a name='F-StutterVst-ApplicationBase-StutterDuration'></a>
### StutterDuration `constants`

##### Summary

Random number between 1/24 and 1/4 (more likely to be lower).

<a name='F-StutterVst-ApplicationBase-StutterInterval'></a>
### StutterInterval `constants`

##### Summary

An even distribution of random numbers between 4 and 6.

<a name='F-StutterVst-ApplicationBase-isPaused'></a>
### isPaused `constants`

##### Remarks

Not really used by the plugin, but could be useful in the future.

<a name='F-StutterVst-ApplicationBase-thresholdParameter'></a>
### thresholdParameter `constants`

##### Summary

The threshold--used by the plugin's [AudioProcessor](#T-StutterVst-AudioProcessor 'StutterVst.AudioProcessor')--can be changed at any time on behalf of the VST host or by the plugin itself.

<a name='T-StutterVst-AudioProcessor'></a>
## AudioProcessor `type`

##### Namespace

StutterVst

##### Summary

This class is where the Stutter Magic happens.

The \`Process\` method is called by the VST host to apply our effect on the input audio.

<a name='F-StutterVst-AudioProcessor-DefaultTailSize'></a>
### DefaultTailSize `constants`

##### Summary

The tail size lets the VST host know how long the output audio will continue after the last input sample is given.

<a name='M-StutterVst-AudioProcessor-getVolumeBias-System-Single,System-Single,System-Single,System-Single,System-Single-'></a>
### getVolumeBias() `method`

##### Summary

A higher threshold causes the volume bias to be more sensitive to higher amplitudes, making stutters be more likely/frequent.

##### Parameters

This method has no parameters.

<a name='T-StutterVst-ApplicationBase-RandomParameter-BiasType'></a>
## BiasType `type`

##### Namespace

StutterVst.ApplicationBase.RandomParameter

<a name='F-StutterVst-ApplicationBase-RandomParameter-BiasType-FullyCubic'></a>
### FullyCubic `constants`

##### Summary

Random values tend especially toward the average value with a fully-cubic bias.

<a name='F-StutterVst-ApplicationBase-RandomParameter-BiasType-LowerCircle'></a>
### LowerCircle `constants`

##### Summary

Random values tend especially toward the minimum value with a circular bias.

<a name='F-StutterVst-ApplicationBase-RandomParameter-BiasType-NegativelyCubed'></a>
### NegativelyCubed `constants`

##### Summary

Random values tend especially toward the maximum value with a negated-cubic bias.

<a name='F-StutterVst-ApplicationBase-RandomParameter-BiasType-NegativelySquared'></a>
### NegativelySquared `constants`

##### Summary

Random values tend toward the maximum value with a squared bias.

<a name='F-StutterVst-ApplicationBase-RandomParameter-BiasType-None'></a>
### None `constants`

##### Summary

Random values are evenly distributed.

<a name='F-StutterVst-ApplicationBase-RandomParameter-BiasType-PositivelyCubed'></a>
### PositivelyCubed `constants`

##### Summary

Random values tend especially toward the minimum value with an positive-cubic bias.

<a name='F-StutterVst-ApplicationBase-RandomParameter-BiasType-Squared'></a>
### Squared `constants`

##### Summary

Random values tend toward the minimum value with a squared bias.

<a name='F-StutterVst-ApplicationBase-RandomParameter-BiasType-UpperCircle'></a>
### UpperCircle `constants`

##### Summary

Random values tend especially toward the maximum value with a circular bias.

<a name='T-StutterVst-DynamicBuffer'></a>
## DynamicBuffer `type`

##### Namespace

StutterVst

##### Summary

Makes the digital signal processing of buffers with multiple sources alot easier,
 whether external (created by the VST host) or internal (created by the plugin for repeated samples).

<a name='F-StutterVst-DynamicBuffer-attachedBase'></a>
### attachedBase `constants`

##### Summary

This array stores references to an unmanaged buffer for each channel (usually provided by the VST host)

<a name='F-StutterVst-DynamicBuffer-capacityExponent'></a>
### capacityExponent `constants`

##### Summary

The capacity of the plugin's internal buffers grow by powers of 2, so a growth increments the capacityExponent by 1

<a name='F-StutterVst-DynamicBuffer-currentSampleIndex'></a>
### currentSampleIndex `constants`

##### Summary

The relative index of the current sample in the external buffer (set by [Seek](#M-StutterVst-DynamicBuffer-Seek-System-Int32- 'StutterVst.DynamicBuffer.Seek(System.Int32)') when the sample is external).

<a name='F-StutterVst-DynamicBuffer-internalPosition'></a>
### internalPosition `constants`

##### Summary

Refers to the current sample in our internal buffer (set by [Seek](#M-StutterVst-DynamicBuffer-Seek-System-Int32- 'StutterVst.DynamicBuffer.Seek(System.Int32)') when the sample is internal).

<a name='P-StutterVst-DynamicBuffer-Capacity'></a>
### Capacity `property`

##### Summary

You don't need to worry about this property except when estimating the tail size.

<a name='P-StutterVst-DynamicBuffer-Item-System-Int32-'></a>
### Item `property`

##### Summary

Call seek for each sample before getting its value for a specific channel!

<a name='P-StutterVst-DynamicBuffer-Item-System-Int32,System-Int32-'></a>
### Item `property`

##### Summary

For accessing samples out of order.

<a name='M-StutterVst-DynamicBuffer-Attach-System-Single*[],System-Int32-'></a>
### Attach() `method`

##### Summary

Integrates the given external buffers for seamless use alongside any internal buffers.

##### Parameters

This method has no parameters.

<a name='M-StutterVst-DynamicBuffer-Save-System-Int32,System-Int32-'></a>
### Save() `method`

##### Summary

Saves the given input samples between \`minSampleIndex\` and \`maxSampleIndex\` to our internal buffer.

##### Parameters

This method has no parameters.

<a name='M-StutterVst-DynamicBuffer-Seek-System-Int32-'></a>
### Seek() `method`

##### Summary

Make sure \`sampleIndex\` is less than internal count returned by \`Attach\` plus external count attached.

##### Parameters

This method has no parameters.

<a name='T-StutterVst-MainView'></a>
## MainView `type`

##### Namespace

StutterVst

##### Summary

Since we're relying on the VST host for the GUI, this class doesn't really do much.

<a name='M-StutterVst-MainView-InitializeComponent'></a>
### InitializeComponent() `method`

##### Summary

InitializeComponent

##### Parameters

This method has no parameters.

<a name='T-StutterVst-Plugin'></a>
## Plugin `type`

##### Namespace

StutterVst

##### Summary

Here stands the gateway between the VST host and the plugin.

<a name='F-StutterVst-Plugin-vstConstructorsByInterface'></a>
### vstConstructorsByInterface `constants`

##### Summary

These are the VST interfaces our plugin supports with the contructors the VST host can call to create them.

<a name='F-StutterVst-Plugin-vstInstancesByInterface'></a>
### vstInstancesByInterface `constants`

##### Summary

Used by \`VstInterfaceManager\` to keep track of interfaces that have already been constructed.

<a name='M-StutterVst-Plugin-AddVstClass``1-System-Type-'></a>
### AddVstClass\`\`1() `method`

##### Summary

After figuring out the VST interface associated with the given class,
 this method adds that interface to a dictionary
 which \`VstInterfaceManager\` uses to let the VST host of the optional VST interfaces this plugin supports.

##### Parameters

This method has no parameters.

<a name='M-StutterVst-Plugin-AddVstInterface``1-StutterVst-Plugin-VstConstructor-'></a>
### AddVstInterface\`\`1() `method`

##### Summary

A more reliable and explicit way of adding support for VST interfaces to a plugin (besides placing entries directly in our dictionary's initializer).

##### Parameters

This method has no parameters.

<a name='T-StutterVst-PresetsManager'></a>
## PresetsManager `type`

##### Namespace

StutterVst

##### Summary

Allows the VST host to automate the plugin and create presets for it.

<a name='T-StutterVst-ApplicationBase-RandomParameter'></a>
## RandomParameter `type`

##### Namespace

StutterVst.ApplicationBase

<a name='P-StutterVst-ApplicationBase-RandomParameter-Value'></a>
### Value `property`

##### Summary

Retrieves a randomly generated value based on the parameter's preconditions.

<a name='T-StutterVst-RequestHandler'></a>
## RequestHandler `type`

##### Namespace

StutterVst

##### Summary

VST.Net's Interop uses this class to provide the VST host with commands it can give to the plugin.

<a name='T-StutterVst-Properties-Resources'></a>
## Resources `type`

##### Namespace

StutterVst.Properties

##### Summary

A strongly-typed resource class, for looking up localized strings, etc.

<a name='P-StutterVst-Properties-Resources-Culture'></a>
### Culture `property`

##### Summary

Overrides the current thread's CurrentUICulture property for all
  resource lookups using this strongly typed resource class.

<a name='P-StutterVst-Properties-Resources-ResourceManager'></a>
### ResourceManager `property`

##### Summary

Returns the cached ResourceManager instance used by this class.

<a name='T-StutterVst-Plugin-VstConstructor'></a>
## VstConstructor `type`

##### Namespace

StutterVst.Plugin

##### Summary

Represents a constructor for a VST interface that assumes it can be called concurrently for multiple plugin instances, but only once for a single plugin instance.

<a name='T-StutterVst-Plugin-VstInterfaceManager`1'></a>
## VstInterfaceManager\`1 `type`

##### Namespace

StutterVst.Plugin

##### Summary

This class is the glue that binds the \`vstConstructorsByInterface\` dictionary used by the plugin to the interface methods used by VST host.
