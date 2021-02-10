using System;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Core.Plugin;
using Jacobi.Vst.Framework.Plugin;

namespace StutterVst
{
	/// <summary>
	///  VST.Net's Interop uses this class to provide the VST host with commands it can give to the plugin.
	/// </summary>
	public class RequestHandler: StdPluginCommandStub, IVstPluginCommandStub
	{
		protected override IVstPlugin CreatePluginInstance()
		{
			return new Plugin();
		}
	}
}
