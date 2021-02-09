using System;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Core.Plugin;
using Jacobi.Vst.Framework.Plugin;

namespace StutterVst
{
	public class RequestHandler: StdPluginCommandStub, IVstPluginCommandStub
	{
		protected override IVstPlugin CreatePluginInstance()
		{
			return new Plugin();
		}
	}
}
