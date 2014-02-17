using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace FLib
{
    public class SketchTypingClient : IDisposable
    {
        readonly System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] resBytes = new byte[256];
        System.Net.Sockets.TcpClient client;
        System.Net.Sockets.NetworkStream ns;
        System.IO.MemoryStream ms = new MemoryStream();

        public SketchTypingClient(string host, int port)
        {
            client = new System.Net.Sockets.TcpClient(host, port);
            ns = client.GetStream();
        }

        public void Dispose()
        {
            ms.Close();
            ns.Close();
            client.Close();
        }

        public string ReadString()
        {
            try
            {
                if (!client.Connected) return "";

                ms.Close();
                ms = new MemoryStream();

                //サーバーから送られたデータを受信する
                while (ns.CanRead && ns.DataAvailable)
                {
                    //データの一部を受信する
                    int resSize = ns.Read(resBytes, 0, resBytes.Length);
                    //Readが0を返した時はサーバーが切断したと判断
                    if (resSize == 0)
                    {
                        break;
                    }
                    //受信したデータを蓄積する
                    ms.Write(resBytes, 0, resSize);
                }

                if (ms.Length <= 0) return "";

                //受信したデータを文字列に変換
                string text = enc.GetString(ms.ToArray());

                // 受信確認用のシグナルを送信
                //            byte[] sendBytes = enc.GetBytes("s");
                //          ns.Write(sendBytes, 0, sendBytes.Length);

                return text.TrimEnd(';');
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + ":" + ex.StackTrace);
                return "";
            }
        }

        public void SendReceivedSignal()
        {
            try
            {
                if (!client.Connected) return;

                if (ns.CanWrite)
                {
                    byte[] sendBytes = enc.GetBytes("s");
                    ns.Write(sendBytes, 0, sendBytes.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + ":" + ex.StackTrace);
            }
        }
    }
}
