using System;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;
using System.Threading.Tasks;
using System.ComponentModel;

namespace StutterVst {
	internal sealed class PresetsManager: VstPluginProgramsBase
	{
		private ApplicationBase creator;
		private VstParameterInfo thresholdParameterInfo;
		public PresetsManager(ApplicationBase creator)
		{
			this.creator= creator;
			thresholdParameterInfo= new VstParameterInfo { CanBeAutomated= true, CanRamp= true, DefaultValue= 0.125f, Label= "Threshold" };
			var thresholdParamMgr= new VstParameterManager(thresholdParameterInfo);
			thresholdParamMgr.PropertyChanged+= onThresholdChanged;
		}

		private void onThresholdChanged(object sender, PropertyChangedEventArgs e)
		{
			var param= sender as VstParameterManager;
			if ( param != null )
				creator.thresholdParameter= param.CurrentValue;
		}

		protected override VstProgramCollection CreateProgramCollection()
		{
			var programs= new VstProgramCollection();
			var defaultProgram= new VstProgram();
			defaultProgram.Parameters.Add(new VstParameter(thresholdParameterInfo));
			defaultProgram.Name= "Default";
			programs.Add(defaultProgram);
			return programs;
		}
	}
}
