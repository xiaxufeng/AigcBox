using System;
using System.IO;
using System.Windows.Forms;

namespace BananaBox
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!IsWebView2Available())
            {
                string installerPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "MicrosoftEdgeWebView2RuntimeInstallerX64.exe");

                if (File.Exists(installerPath))
                {
                    MessageBox.Show(
                        "检测到WebView2运行时缺失，正在尝试安装WebView2运行时...",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    var process = System.Diagnostics.Process.Start(installerPath);
                    process?.WaitForExit();
                }
                else
                {
                    MessageBox.Show(
                        "检测到WebView2运行时缺失，请前往以下地址下载安装后重新运行：\nhttps://developer.microsoft.com/microsoft-edge/webview2/",
                        "缺少依赖",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            Application.Run(new Form1());
        }

        private static bool IsWebView2Available()
        {
            try
            {
                string version = Microsoft.Web.WebView2.Core.CoreWebView2Environment
                    .GetAvailableBrowserVersionString();
                return !string.IsNullOrEmpty(version);
            }
            catch
            {
                return false;
            }
        }
    }
}
