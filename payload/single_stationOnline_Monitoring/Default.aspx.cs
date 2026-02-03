using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication2
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void btnCall_Click(object sender, EventArgs e)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = @"C:\Users\Antonios Leisos\source\repos\conversion\Hantek\Hantek-6000_Ver2.2.7_D2021072120210731090805\Hantek-6000_Ver2.2.7_D20210721\SDK\Code Demo\VCDSO_orig\Debug\VCDSO.exe";
            info.Arguments = "";
            info.WindowStyle = ProcessWindowStyle.Normal;
            Process pro = Process.Start(info);
            pro.WaitForExit();
        }
        private static string Batchresults;
        private string BatchCommand(string cmd, string mapD)
        {
            Batchresults = "";

            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + cmd);
            procStartInfo.WorkingDirectory = mapD;
            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.RedirectStandardInput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process cmdProcess = new System.Diagnostics.Process();
            cmdProcess.StartInfo = procStartInfo;
            cmdProcess.ErrorDataReceived += cmd_Error;
            cmdProcess.OutputDataReceived += cmd_DataReceived;
            cmdProcess.EnableRaisingEvents = true;
            cmdProcess.Start();
            cmdProcess.BeginOutputReadLine();
            cmdProcess.BeginErrorReadLine();
            cmdProcess.StandardInput.WriteLine("exit");                  //Execute exit.
            cmdProcess.WaitForExit();

            // Get the output into a string

            return Batchresults;
        }
        static void cmd_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                Batchresults += Environment.NewLine + e.Data.ToString();

        }

        void cmd_Error(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Batchresults += Environment.NewLine + e.Data.ToString();

            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //string result = BatchCommand("script.cmd", @"C:\Users\Antonios@ Leisos\source\repos\WebApplication2\temp");
            string strRootRelativePathName = @"~\temp";

            string strPathName =
                Server.MapPath(strRootRelativePathName);
            if (System.IO.File.Exists(strPathName) == false)
            {
                //TextBox1.Text = "Error: File Not Found!";
            }
            string result = BatchCommand("script.cmd", strPathName);
        }

        protected void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}