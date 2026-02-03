using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.IO;
namespace WebApplication2
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["Station"] == null) Response.Redirect("Station-Selection");

                //create directory
                string SessionId = HttpContext.Current.Session.SessionID;
           if (HttpContext.Current.Session["Calibration_folder"] == null)
            {
                HttpContext.Current.Session["Calibration_folder"] = "calibration_" + SessionId;
                string strRootRelativePathName = @"~\App_Data\"+ "calibration_" + SessionId;
                string datetime = System.DateTime.Now.ToString();
                HttpContext.Current.Session["Calibration_datetime"] = datetime;
                string strPathName =
                    Server.MapPath(strRootRelativePathName);
                if (System.IO.File.Exists(strPathName) == false)
                {
                    Directory.CreateDirectory(strPathName);
                }
                strRootRelativePathName = @"~\ProgramFiles\vhist.C"; strPathName += @"\vhist.C";
                File.Copy(Server.MapPath(strRootRelativePathName), strPathName,true);
            }
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

        protected void Start_Acquisition(object sender, EventArgs e)
        {
            string folder =HttpContext.Current.Session["Calibration_folder"].ToString();
            string thr1 = String.Format("{0}", Request.Form["trg_thr_1"]);
            string thr2 = String.Format("{0}", Request.Form["trg_thr_2"]);
            string thr3 = String.Format("{0}", Request.Form["trg_thr_3"]);
            HttpContext.Current.Session["Calibration_thr1"] = thr1;
            HttpContext.Current.Session["Calibration_thr2"] = thr2;
            HttpContext.Current.Session["Calibration_thr3"] = thr3;
            string strPathCalibrationFolder =
                Server.MapPath(folder);
            if (System.IO.File.Exists(strPathCalibrationFolder) == false)
            {
                Response.Write("<script>alert('" + "Directory does not exist. This should not happen! ErrorCode:001" + "')</script>");
            }
            string strRootRelativePathName = @"~\ProgramFiles\script_calib_start.cmd"; strPathCalibrationFolder += @"\script_calib_start.cmd";
            File.Copy(Server.MapPath(strRootRelativePathName), strPathCalibrationFolder, true);
            StreamWriter rs=File.CreateText(strPathCalibrationFolder + @"\info.txt");
            rs.WriteLine(thr1);
            rs.WriteLine(thr2);
            rs.WriteLine(thr3);
            rs.WriteLine(HttpContext.Current.Session["Calibration_datetime"].ToString());
            rs.WriteLine(HttpContext.Current.Session.SessionID);
            rs.WriteLine(HttpContext.Current.Session["Station"].ToString());
            string result = BatchCommand("script_calib_start.cmd", strPathCalibrationFolder);
            HttpContext.Current.Session["Calibration_State"] = result;
        }

        protected void Stop_Acquisition(object sender, EventArgs e)
        {
            string folder = HttpContext.Current.Session["Calibration_folder"].ToString();
            string strPathName =
                Server.MapPath(folder);
            if (System.IO.File.Exists(strPathName) == false)
            {
                //TextBox1.Text = "Error: File Not Found!";
            }
            string strRootRelativePathName = @"~\ProgramFiles\script_calib_stop.cmd"; strPathName += @"\script_calib_stop.cmd";
            File.Copy(Server.MapPath(strRootRelativePathName), strPathName, true);
            string result = BatchCommand("script_calib_stop.cmd", strPathName);
            HttpContext.Current.Session["Calibration_State"] = result;
        }
        protected void Clear_All(object sender, EventArgs e)
        {
            string folder = HttpContext.Current.Session["Calibration_folder"].ToString();
            string strPathName =
                Server.MapPath(folder);
            if (System.IO.File.Exists(strPathName) == false)
            {
                //TextBox1.Text = "Error: File Not Found!";
            }
            string strRootRelativePathName = @"~\ProgramFiles\script_calibclearall.cmd"; strPathName += @"\script_calibclearall.cmd";
            File.Copy(Server.MapPath(strRootRelativePathName), strPathName, true);
            string result = BatchCommand("script_calib_clearall.cmd", strPathName);
            HttpContext.Current.Session["Calibration_State"] = result;
        }
        protected void Clear_Pulses(object sender, EventArgs e)
        {
            string folder = HttpContext.Current.Session["Calibration_folder"].ToString();
            string strPathName =
                Server.MapPath(folder);
            if (System.IO.File.Exists(strPathName) == false)
            {
                //TextBox1.Text = "Error: File Not Found!";
            }
            string strRootRelativePathName = @"~\ProgramFiles\script_calib_clearpulses.cmd"; strPathName += @"\script_calib_clearpulses.cmd";
            File.Copy(Server.MapPath(strRootRelativePathName), strPathName, true);
            string result = BatchCommand("script_calib_clearpulses.cmd", strPathName);
            HttpContext.Current.Session["Calibration_State"] = result;
        }

        protected void Clear_Distributions(object sender, EventArgs e)
        {
            string folder = HttpContext.Current.Session["Calibration_folder"].ToString();
            string strPathName =
                Server.MapPath(folder);
            if (System.IO.File.Exists(strPathName) == false)
            {
                //TextBox1.Text = "Error: File Not Found!";
            }
            string strRootRelativePathName = @"~\ProgramFiles\script_calib_cleardistr.cmd"; strPathName += @"\script_calib_cleardistr.cmd";
            File.Copy(Server.MapPath(strRootRelativePathName), strPathName, true);
            string result = BatchCommand("script_calib_cleardistr.cmd", strPathName);
            HttpContext.Current.Session["Calibration_State"] = result;
        }

    }
}