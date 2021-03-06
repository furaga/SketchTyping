using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;

namespace SketchTypingVSAddin
{
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{

		public Connect()
		{
		}

		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;
		
            if(connectMode == ext_ConnectMode.ext_cm_UISetup)
			{
				object []contextGUIDS = new object[] { };
				Commands2 commands = (Commands2)_applicationObject.Commands;
				string toolsMenuName = "Tools";

				//コマンドを [ツール] メニューに配置します。
				//メイン メニュー項目のすべてを保持するトップレベル コマンド バーである、MenuBar コマンド バーを検索します:
				Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

				//MenuBar コマンド バーで [ツール] コマンド バーを検索します:
				CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
				CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

				//アドインによって処理する複数のコマンドを追加する場合、この try ブロックおよび catch ブロックを重複できます。
				//  ただし、新しいコマンド名を含めるために QueryStatus メソッドおよび Exec メソッドの更新も実行してください。
				try
				{
					//コマンド コレクションにコマンドを追加します:
					Command command = commands.AddNamedCommand2(_addInInstance, "SketchTypingVSAddin", "SketchTypingVSAddin", "Executes the command for SketchTypingVSAddin", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

					//コマンドのコントロールを [ツール] メニューに追加します:
					if((command != null) && (toolsPopup != null))
					{
						command.AddControl(toolsPopup.CommandBar, 1);
					}
                }
				catch(System.ArgumentException)
				{
					//同じ名前のコマンドが既に存在しているため、例外が発生した可能性があります。
					//  その場合、コマンドを再作成する必要はありません。 例外を 
                    //  無視しても安全です。
				}
			}
		}

		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		public void OnAddInsUpdate(ref Array custom)
		{
		}

		public void OnStartupComplete(ref Array custom)
		{
		}

		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				if(commandName == "SketchTypingVSAddin.Connect.SketchTypingVSAddin")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
			}
		}

        int cnt = 0;

		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
                       
            if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
				if(commandName == "SketchTypingVSAddin.Connect.SketchTypingVSAddin")
				{
                    if (sketchTypingWindow != null)
                    {
                        sketchTypingControl.Dispose();
                        sketchTypingWindow.Close();
//                        System.Threading.Thread.Sleep(1000);
                    }

                    // 追加 by furaga
                    string ctlProgID = "SketchTypingVSAddin.SketchTypingControl";
                    Assembly asm = Assembly.GetExecutingAssembly();
                    string asmPath = asm.Location;
                    string guidStr = Guid.NewGuid().ToString();
                    object tmpObj = null;

                    EnvDTE80.Windows2 toolWins = (Windows2)_applicationObject.Windows;
                    sketchTypingWindow = toolWins.CreateToolWindow2(_addInInstance, asmPath, ctlProgID, "MyNewToolwindow" + cnt, guidStr, ref tmpObj);
                    sketchTypingControl = tmpObj as SketchTypingControl;
                    sketchTypingControl.Initialize(_applicationObject);

                    if (sketchTypingWindow != null)
                    {
                        sketchTypingWindow.Visible = true;
                    }
					handled = true;
					return;
				}
			}
        }

        EnvDTE.Window sketchTypingWindow;
        SketchTypingControl sketchTypingControl;


		private DTE2 _applicationObject;
		private AddIn _addInInstance;
	}
}