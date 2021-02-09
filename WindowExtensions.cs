using System;
using System.Windows;
using System.Security;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace StutterVst
{

	internal static class WindowExtensions
	{
		
		[SecurityCritical] [DllImport("user32.dll")]
		private static extern IntPtr SetParent(IntPtr childHandle, IntPtr newParentHandle); 

		[SecurityCritical]
		public static void SetParent(this Window @this, IntPtr newParentHandle)
		{
			SetParent( new WindowInteropHelper(@this).EnsureHandle(), newParentHandle );
		}

	}

}
