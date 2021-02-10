using System;
using System.Windows;
using System.Security;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace StutterVst
{

	/// <summary>
	///  Since we're relying on the VST host for the GUI, this class doesn't really do much.
	/// </summary>
    public partial class MainView: Window, IVstPluginEditor, IDisposable
    {
		ApplicationBase creator;

		internal MainView(ApplicationBase creator)
        {
			this.creator= creator;
            InitializeComponent();
        }

		[SecuritySafeCritical]
		void IVstPluginEditor.Open(IntPtr parentHandle)
		{
			//this.SetParent(parentHandle);
			//Show();
			//Visibility= Visibility.Visible;
			//System.Windows.MessageBox.Show("HI!");
		}

		bool IVstPluginEditor.KeyDown(byte charCode, VstVirtualKey currentKey, VstModifierKeys modifierKey)
		{
			return false;
		}
	
		bool IVstPluginEditor.KeyUp(byte charCode, VstVirtualKey currentKey, VstModifierKeys modifierKey)
		{
			return false;
		}

		VstKnobMode IVstPluginEditor.KnobMode {
			get { return vstKnobMode; }
			set { vstKnobMode= value; }
		}
		private VstKnobMode vstKnobMode= VstKnobMode.LinearMode;

		System.Drawing.Rectangle IVstPluginEditor.Bounds => new System.Drawing.Rectangle ( (int)Left, (int)Top, (int)Width, (int)Height );

		void IDisposable.Dispose()
		{
			Close();
		}

		void IVstPluginEditor.ProcessIdle() {
			
		}

		void IVstPluginEditor.Close() {
			Visibility= Visibility.Collapsed;
		}
	}
}
