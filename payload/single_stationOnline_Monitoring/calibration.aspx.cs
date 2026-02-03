  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web.UI.HtmlControls;
using System.Web.Caching;
using System.Threading.Tasks;
namespace WebApplication2
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        static int ic = 0;
        // static public StreamReader rs=null;
        protected void Page_Load(object sender, EventArgs e)
        {
            //HtmlAnchor HA = new HtmlAnchor();
            //HA.ServerClick += new EventHandler(Tab_Response_Click);
            //HtmlAnchor HA2 = new HtmlAnchor();
            //HA2.ServerClick += new EventHandler(Tab_Sync_Click);
            HttpContext.Current.Session["ReadAll"] = 0;// use 1 to read the previous file (all data up to 20000 events)
             if (HttpContext.Current.Session["Station"] == null) Response.Redirect("StationSelection");
            if (HttpContext.Current.Session["Station"] != null && HttpContext.Current.Session["Station"].ToString() == "0")
            {
                //Response.Write("<script>alert('" + "Choose station please!" + "')</script>");
                Response.Redirect("StationSelection");
                return;
            }
           

            if (HttpContext.Current.Session["Active_Tab"] != null)
            {
                if (HttpContext.Current.Session["Active_Tab"].ToString() == "0") Show_Response_Tab();
                if (HttpContext.Current.Session["Active_Tab"].ToString() == "1") Show_Sync_Tab();
            }
            // else Show_Response_Tab();
            //if (HttpContext.Current.Session["Station"] != null) Status_resp.Text ="Selected Station: "+ HttpContext.Current.Session["Station"].ToString();
            //if (HttpContext.Current.Session["Station"] != null) Status_sync.Text = "Selected Station: " + HttpContext.Current.Session["Station"].ToString();
            //if (HttpContext.Current.Session["Calibration_Response_Started"]!=null && HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "1") Status_resp.Text += ", Response Calibration started";
            //if (HttpContext.Current.Session["Calibration_Sync_Started"] != null && HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "1") Status_sync.Text += ", Timing Calibration started";
            ShowStatusResponse();
            ShowStatusSync();
            if (HttpContext.Current.Session["Calibration_thr1"] == null) HttpContext.Current.Session["Calibration_thr1"]="5.0";
            if (HttpContext.Current.Session["Calibration_thr2"] == null) HttpContext.Current.Session["Calibration_thr2"] = "5.0";
            if (HttpContext.Current.Session["Calibration_thr3"] == null) HttpContext.Current.Session["Calibration_thr3"] = "5.0";
            if (HttpContext.Current.Session["Calibration_thr1_sync"] == null) HttpContext.Current.Session["Calibration_thr1_sync"] = "5.0";
            if (HttpContext.Current.Session["Calibration_thr2_sync"] == null) HttpContext.Current.Session["Calibration_thr2_sync"] = "5.0";
            if (HttpContext.Current.Session["Calibration_thr3_sync"] == null) HttpContext.Current.Session["Calibration_thr3_sync"] = "5.0";
            if (HttpContext.Current.Session["Calibration_tim1_sync"] == null) HttpContext.Current.Session["Calibration_tim1_sync"] = "5.0";
            if (HttpContext.Current.Session["Calibration_tim2_sync"] == null) HttpContext.Current.Session["Calibration_tim2_sync"] = "5.0";
            if (HttpContext.Current.Session["Calibration_tim3_sync"] == null) HttpContext.Current.Session["Calibration_tim3_sync"] = "5.0";
            if (HttpContext.Current.Session["CFD"] == null) HttpContext.Current.Session["CFD"] = 0; //initial value

            if (!IsPostBack)
            {
                //retain the entried in the edit-boxes
                if (HttpContext.Current.Session["Calibration_thr1"] != null) text_thr1.Text = HttpContext.Current.Session["Calibration_thr1"].ToString();
                if (HttpContext.Current.Session["Calibration_thr2"] != null) text_thr2.Text = HttpContext.Current.Session["Calibration_thr2"].ToString();
                if (HttpContext.Current.Session["Calibration_thr3"] != null) text_thr3.Text = HttpContext.Current.Session["Calibration_thr3"].ToString();
                if (HttpContext.Current.Session["Calibration_thr1_sync"] !=null) text_thr1_sync.Text = HttpContext.Current.Session["Calibration_thr1_sync"].ToString();
                if (HttpContext.Current.Session["Calibration_thr2_sync"] != null) text_thr2_sync.Text = HttpContext.Current.Session["Calibration_thr2_sync"].ToString();
                if (HttpContext.Current.Session["Calibration_thr3_sync"] != null) text_thr3_sync.Text = HttpContext.Current.Session["Calibration_thr3_sync"].ToString();
                if (HttpContext.Current.Session["Calibration_tim1_sync"] != null) Tim1.Text = HttpContext.Current.Session["Calibration_tim1_sync"].ToString();
                if (HttpContext.Current.Session["Calibration_tim2_sync"] != null) Tim2.Text = HttpContext.Current.Session["Calibration_tim2_sync"].ToString();
                if (HttpContext.Current.Session["Calibration_tim3_sync"] != null) Tim3.Text = HttpContext.Current.Session["Calibration_tim3_sync"].ToString();
                if (HttpContext.Current.Session["CFD"] != null)
                {
                    checkbox1.Checked = false;
                    if (HttpContext.Current.Session["CFD"].ToString() == "1") checkbox1.Checked = true;
                }
            }

            //create directory
            string SessionId = HttpContext.Current.Session.SessionID;
            if (HttpContext.Current.Session["Calibration_folder"] == null)
            {
                Timer1.Enabled = false;
                //Timer2.Enabled = false;
                HttpContext.Current.Session["Calibration_Response_Started"] = 0;
                HttpContext.Current.Session["Calibration_Sync_Started"] = 0;

                DateTime lt = DateTime.Now;
                string dateflag = lt.Year.ToString() + "_" + lt.Month.ToString() + "_" + lt.Day.ToString() + "_" + lt.Hour.ToString() + "_"+lt.Minute.ToString() + "_" + lt.Second.ToString();
                HttpContext.Current.Session["Calibration_folder"] = "calibration_" + SessionId+"_"+ HttpContext.Current.Session["Station"].ToString()+"_"+dateflag;
                string strRootRelativePathName = @"~\App_Data\" + "calibration_" + SessionId + "_" + HttpContext.Current.Session["Station"].ToString() + "_" + dateflag;
                string datetime = System.DateTime.Now.ToString();
                HttpContext.Current.Session["Calibration_datetime"] = datetime;
                string strPathName =
                    Server.MapPath(strRootRelativePathName);
                if (System.IO.File.Exists(strPathName) == false)
                {
                    Directory.CreateDirectory(strPathName);
                }
                string strPathName1= strPathName + @"\vhist.C";
                string strPathName2 = strPathName + @"\timing.C";
                strRootRelativePathName = @"~\ProgramFiles\vhist.C"; //strPathName += @"\vhist.C";
                File.Copy(Server.MapPath(strRootRelativePathName), strPathName1, true);
                //strPathName = Server.MapPath(@"~\App_Data\" + "calibration_" + SessionId);
                strRootRelativePathName = @"~\ProgramFiles\timing.C"; //strPathName += @"\timing.C";
                File.Copy(Server.MapPath(strRootRelativePathName), strPathName2, true);
            }
            else
            {
                if (HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "1" || HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "1") Timer1.Enabled = true;
            }
            /*
            if (Request.QueryString["id"] == "1")
            {
                Start_Acquisition_Sync(sender, e);
            }
            if (Request.QueryString["id"] == "2")
            {
                Stop_Acquisition_Sync(sender, e);
            }
            if (Request.QueryString["id"] == "3")
            {
                Start_Acquisition_Response(sender, e);
            }
            if (Request.QueryString["id"] == "4")
            {
                Stop_Acquisition_Response(sender, e);
            }
            if (Request.QueryString["id"] == "5")
            {
                Clear_Pulses(sender, e);
            }
            if (Request.QueryString["id"] == "6")
            {
                Clear_Distributions(sender, e);
            }
            if (Request.QueryString["id"] == "7")
            {
                Clear_All(sender, e);
            }
            PropertyInfo Isreadonly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

            Isreadonly.SetValue(Request.QueryString, false, null);

            Request.QueryString.Clear();
            */
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

        protected void ShowStatusResponse()
        {
            if (HttpContext.Current.Session["Station"] != null) Status_resp.Text = "Selected Station: " + HttpContext.Current.Session["Station"].ToString();
            if (HttpContext.Current.Session["Calibration_Response_Started"] != null && HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "1") Status_resp.Text += ", Response Calibration running.";
            if (HttpContext.Current.Session["Calibration_Response_Started"] != null && HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "0") Status_resp.Text += ", Response Calibration not running.";

            if (HttpContext.Current.Session["Calibration_Response_Started"] != null && HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "1")
            {
                if (HttpContext.Current.Session["Calibration_thr1"] != null && HttpContext.Current.Session["Calibration_thr2"] != null && HttpContext.Current.Session["Calibration_thr3"] != null) Status_resp.Text += "Thres:" + HttpContext.Current.Session["Calibration_thr1"].ToString() + "," + HttpContext.Current.Session["Calibration_thr2"].ToString() + "," + HttpContext.Current.Session["Calibration_thr3"].ToString();
            }
        }
        protected void ShowStatusSync()
        {
            if (HttpContext.Current.Session["Station"] != null) Status_sync.Text = "Selected Station: " + HttpContext.Current.Session["Station"].ToString();
            if (HttpContext.Current.Session["Calibration_Sync_Started"] != null && HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "1") Status_sync.Text += ", Synchronization Calibration running.";
            if (HttpContext.Current.Session["Calibration_Sync_Started"] != null && HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "0") Status_sync.Text += ", Synchronization Calibration not running.";

            if (HttpContext.Current.Session["Calibration_Sync_Started"] != null && HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "1")
            {
                if (HttpContext.Current.Session["Calibration_thr1_sync"] != null && HttpContext.Current.Session["Calibration_thr2_sync"] != null && HttpContext.Current.Session["Calibration_thr3_sync"] != null) Status_sync.Text += "Thres:" + HttpContext.Current.Session["Calibration_thr1_sync"].ToString() + "," + HttpContext.Current.Session["Calibration_thr2_sync"].ToString() + "," + HttpContext.Current.Session["Calibration_thr3_sync"].ToString();
                if (HttpContext.Current.Session["Calibration_tim1_sync"] != null && HttpContext.Current.Session["Calibration_tim2_sync"] != null && HttpContext.Current.Session["Calibration_tim3_sync"] != null) Status_sync.Text += " Timing Thres:" + HttpContext.Current.Session["Calibration_tim1_sync"].ToString() + "," + HttpContext.Current.Session["Calibration_tim2_sync"].ToString() + "," + HttpContext.Current.Session["Calibration_tim3_sync"].ToString();
            }
        }

        protected bool check_Response_values()
        {
            string thr1 = text_thr1.Text;// String.Format("{0}", Request.Form["trg_thr_1"]);
            string thr2 = text_thr2.Text;//String.Format("{0}", Request.Form["trg_thr_2"]);
            string thr3 = text_thr3.Text;//String.Format("{0}", Request.Form["trg_thr_3"]);
            if (thr1 == "" || thr2 == "" || thr3 == "" || !GraterValue(thr1, 5.0) || !GraterValue(thr2, 5.0) || !GraterValue(thr3, 5.0))
            {
                Response.Write("<script>alert('" + "Please select threshold values grater than 5.0 mV" + "')</script>");
                return false;
            }

            HttpContext.Current.Session["Calibration_thr1"] = thr1;
            HttpContext.Current.Session["Calibration_thr2"] = thr2;
            HttpContext.Current.Session["Calibration_thr3"] = thr3;
            return true;
        }
            protected void Start_Acquisition_Response(object sender, EventArgs e)
        {
            if (!check_Response_values()) return;

            if (HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "1")
            {
                Response.Write("<script>alert('" + "Response Calibration allready started! Maybe you want to stop the acquisition first." + "')</script>");
                return;
            }
            Response.Write("<script>alert('" + "Response: Acquisition starting." + "')</script>");

            string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());

            float[,] myDoubleArray = new float[3, 400];
            if (HttpContext.Current.Session["spm"] == null) { HttpContext.Current.Session["spm"] = myDoubleArray; }
            if (HttpContext.Current.Session["npm"] == null) { HttpContext.Current.Session["npm"] = new int[] { 0, 0, 0 }; }

            if (Directory.Exists(strPathCalibrationFolder) == false)
            {
                Response.Write("<script>alert('" + "Directory does not exist. This should not happen! ErrorCode:001" + "')</script>");
            }
            string strRootRelativePathName = Server.MapPath(@"~\ProgramFiles\script_calib_start.cmd");
            if (File.Exists(strRootRelativePathName))
                File.Copy(strRootRelativePathName, strPathCalibrationFolder + @"\script_calib_start.cmd", true);
            StreamWriter rs = File.CreateText(strPathCalibrationFolder + @"\info_response.txt");
            rs.WriteLine(text_thr1.Text);
            rs.WriteLine(text_thr2.Text);
            rs.WriteLine(text_thr3.Text);
            rs.WriteLine(HttpContext.Current.Session["Calibration_datetime"].ToString());
            rs.WriteLine(HttpContext.Current.Session.SessionID);
            rs.WriteLine(HttpContext.Current.Session["Station"].ToString());
            rs.Close();

            if (HttpContext.Current.Session["SetFilePosition"]==null ||  (HttpContext.Current.Session["ReadAll"].ToString() == "1"))
            {   
                Set_File_Position();
                HttpContext.Current.Session["SetFilePosition"] = true;
            }
            HttpContext.Current.Session["Calibration_Response_Started"] = 1;
            ShowStatusResponse();

            if (HttpContext.Current.Session["ReadAll"].ToString() == "0") this.Timer1.Enabled = true;
            Read_Events_and_Show_Protected();
            //Show_Response_Tab();
        }

 
                protected bool check_Sync_values()
                {
                    string thr1 = text_thr1_sync.Text;// String.Format("{0}", Request.Form["trg_thr_1"]);
                    string thr2 = text_thr2_sync.Text;//String.Format("{0}", Request.Form["trg_thr_2"]);
                    string thr3 = text_thr3_sync.Text;//String.Format("{0}", Request.Form["trg_thr_3"]);
                    if (thr1 == "" || thr2 == "" || thr3 == "" || !GraterValue(thr1, 5.0) || !GraterValue(thr2, 5.0) || !GraterValue(thr3, 5.0))
                    {
                        Response.Write("<script>alert('" + "Please select threshold values grater than 5.0 mV" + "')</script>");
                        return false;
                    }

                    HttpContext.Current.Session["Calibration_thr1_sync"] = thr1;
                    HttpContext.Current.Session["Calibration_thr2_sync"] = thr2;
                    HttpContext.Current.Session["Calibration_thr3_sync"] = thr3;


                    float thr = (float)Convert.ChangeType(text_thr1_sync.Text, typeof(float));
                    float tim = (float)Convert.ChangeType(Tim1.Text, typeof(float));
                    if (tim > thr) { Response.Write("<script>alert('" + "Please select timing threshold less than trigger threshold (Detector 1)" + "')</script>"); return false; }
                    thr = (float)Convert.ChangeType(text_thr2_sync.Text, typeof(float));
                    tim = (float)Convert.ChangeType(Tim2.Text, typeof(float));
                    if (tim > thr) { Response.Write("<script>alert('" + "Please select timing threshold less than trigger threshold (Detector 2)" + "')</script>"); return false; }
                    thr = (float)Convert.ChangeType(text_thr3_sync.Text, typeof(float));
                    tim = (float)Convert.ChangeType(Tim3.Text, typeof(float));
                    if (tim > thr) { Response.Write("<script>alert('" + "Please select timing threshold less than trigger threshold (Detector 3)" + "')</script>"); return false; }

                    thr1 = Tim1.Text;// String.Format("{0}", Request.Form["trg_thr_1"]);
                    thr2 = Tim2.Text;//String.Format("{0}", Request.Form["trg_thr_2"]);
                    thr3 = Tim3.Text;//String.Format("{0}", Request.Form["trg_thr_3"]);
                    if (thr1 == "" || thr2 == "" || thr3 == "" || !GraterValue(thr1, 5.0) || !GraterValue(thr2, 5.0) || !GraterValue(thr3, 5.0))
                    {
                        Response.Write("<script>alert('" + "Please select timing threshold values grater than 5.0 mV" + "')</script>");
                        return false;
                    }

                    HttpContext.Current.Session["Calibration_tim1_sync"] = thr1;
                    HttpContext.Current.Session["Calibration_tim2_sync"] = thr2;
                    HttpContext.Current.Session["Calibration_tim3_sync"] = thr3;
                    return true;
                }
        protected void Start_Acquisition_Sync(object sender, EventArgs e)
        {
            if (!check_Sync_values()) return;

            if (HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "1")
            {
                Response.Write("<script>alert('" + "Syncronization Calibration allready started! Maybe you want to stop the acquisition first." + "')</script>");
                return;
            }
            Response.Write("<script>alert('" + "Synchronization: Acquisition starting." + "')</script>");

            string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());

            if (Directory.Exists(strPathCalibrationFolder) == false)
            {
                Response.Write("<script>alert('" + "Directory does not exist. This should not happen! ErrorCode:001" + "')</script>");
            }
            string strRootRelativePathName = Server.MapPath(@"~\ProgramFiles\script_sync_start.cmd");
            if (File.Exists(strRootRelativePathName))
                File.Copy(strRootRelativePathName, strPathCalibrationFolder + @"\script_sync_start.cmd", true);
            StreamWriter rs = File.CreateText(strPathCalibrationFolder + @"\info_timing.txt");
            rs.WriteLine(text_thr1_sync.Text);
            rs.WriteLine(text_thr2_sync.Text);
            rs.WriteLine(text_thr3_sync.Text);
            rs.WriteLine(Tim1.Text);
            rs.WriteLine(Tim2.Text);
            rs.WriteLine(Tim3.Text);
            rs.WriteLine(HttpContext.Current.Session["Calibration_datetime"].ToString());
            rs.WriteLine(HttpContext.Current.Session.SessionID);
            rs.WriteLine(HttpContext.Current.Session["Station"].ToString());
            rs.Close();

            if (HttpContext.Current.Session["SetFilePosition"] == null || (HttpContext.Current.Session["ReadAll"].ToString() == "1")) // at first time we want to set the position either in response or in syncronization. 
            {                                                           // if the other calibarion is still running we dont want to disturb the position
                Set_File_Position();
                HttpContext.Current.Session["SetFilePosition"] = true;
            }
            HttpContext.Current.Session["Calibration_Sync_Started"] = 1;
            ShowStatusSync();

            if (HttpContext.Current.Session["ReadAll"].ToString() == "0") this.Timer1.Enabled = true;
            Read_Events_and_Show_Protected();
            //Show_Sync_Tab();
        }

        protected void Set_File_Position()
        {
            if (HttpContext.Current.Session["Hantek"] == null)
            {
                Response.Write("<script>alert('" + "Hantek does not exist. This should not happen! ErrorCode:002" + "')</script>");
            }
            string Hantek = HttpContext.Current.Session["Hantek"].ToString();

            DateTime lt = DateTime.Now;
            string filename = Hantek+"_"+lt.Year.ToString() + "_" + lt.Month.ToString() + "_" + lt.Day.ToString() + "_" + lt.Hour.ToString()+".data";
            if (HttpContext.Current.Session["ReadAll"].ToString() == "1") filename = Hantek + "_" + lt.Year.ToString() + "_" + lt.Month.ToString() + "_" +  "0_"  + "0.data";

            //string strRootRelativePathName = Server.MapPath(@"~\ProgramFiles\Save_Pulses_Calibration\" + filename);
            string strRootRelativePathName = @"D:\Save_Pulses_Calibration_Phase2\" + filename;
            if (File.Exists(strRootRelativePathName))
            {
                var file = new FileStream(strRootRelativePathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader rs = new StreamReader(file);
                HttpContext.Current.Session["Calibration_rs"] = rs;
                {
                    //rs = File.OpenText(strRootRelativePathName);
                    //rs.BaseStream.Seek(-4221, SeekOrigin.End);
                    //return;
                    rs.ReadToEnd();
                    long pos = rs.BaseStream.Position;
                    decimal events = (pos / 5050) - System.Math.Floor((decimal)(pos / 5050));
                    if (events > 0)
                    {
                        long p = (long)System.Math.Floor((decimal)(pos / 5050));
                        rs.BaseStream.Seek(p, SeekOrigin.Begin);
                    }
                    if (HttpContext.Current.Session["ReadAll"].ToString() == "1") rs.BaseStream.Seek(0, SeekOrigin.Begin);
                    // string line = rs.ReadLine();
                    // long kkk=rs.BaseStream.Seek(-2, SeekOrigin.End);
                    // line = rs.ReadLine();
                }
            }

            //pulselastfile = fopen(filename, "a");
        }
        private static Object thisLock = new Object();
        protected void Save_Peaks(int counter, float peak)
        {
            string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());
            string path = strPathCalibrationFolder + @"\test" + counter.ToString() + ".txt";
            try
            {
                lock (thisLock)
                {
                    File.AppendAllText(path, peak.ToString() + "\n");
                }
            }
            catch
            {; }
        }

        protected void Save_Mean_Pulse(int Counter, int CH, float[,] vv)
        {
            int counter = Counter - 1;
            int[] npm = (int[])(Session["npm"]);
            float[,] spm = (float[,])(Session["spm"]);
            float[] pm = new float[400];
            for (int k = 0; k < 400; k++) pm[k] = 0;


            int kmax = -1;
            float amax = -1;
            for (int k = 0; k < 200; k++)
            {
                if (Math.Abs(vv[CH, k])<100 && vv[CH, k] > amax)
                {
                    amax = vv[CH, k];
                    kmax = k;
                }
            }
            bool fail = false;
            if (kmax < 0)
                fail = true;
            //save mean pulse
            int k0 = -1;
            for (int k = kmax; k > -1; k--)
            {
                if (Math.Abs(vv[CH, k]) < 100 &&  vv[CH, k] < amax / 2)
                {
                    k0 = k; break;
                }
            }
            if (k0 < 0)
                fail = true;
            if (!fail && amax > 3.0)
            {
                int lll;
                if (fail == true)
                    lll = 0;
                npm[counter] = npm[counter] + 1;
                int ib = 0;
                for (int k = k0; k < 200; k++)
                {
                    pm[200 + ib] = vv[CH, k];
                    ib++;
                }
                ib = 0;
                for (int k = k0; k > -1; k--)
                {
                    if (200 - ib > -1) pm[200 - ib] = vv[CH, k]; ib++;
                }
                for (int k = 0; k < 200 - (ib - 1); k++) pm[k] = 0;
                //for (int k = 1200; k < 4096; k++) pm[k] = 0;

                for (int k = 0; k < 400; k++)
                    spm[counter, k] = spm[counter, k] + pm[k];
            //} commented aout on 16/04/22

            // if ((int)npm[counter] % 100 == 0)
            //{ commented out on 16/04/2022
                string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());
                //StreamWriter rs = File.CreateText(strPathCalibrationFolder + @"\pulse" + Counter.ToString() + ".txt");
                string name = strPathCalibrationFolder + @"\pulse" + Counter.ToString() + ".txt";
                try
                {
                    var file = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    StreamWriter rs = new StreamWriter(file);
                    float ss;
                    for (int k = 0; k < 400; k++)
                    {
                        ss = spm[counter, k] / (float)(npm[counter]);
                        rs.WriteLine(ss.ToString());
                    }
                    rs.Close();
                }
                catch {; }
            }

        }
        protected void Read_events(int mode)
        {
            StreamReader rs = (StreamReader)HttpContext.Current.Session["Calibration_rs"];
            if (rs == null) return;
            float[] peak = new float[4] { -1, -1, -1, 1 };
            float[] vflag = new float[4] { -1, -1, -1, 1 };
            long record = rs.BaseStream.Length - rs.BaseStream.Position;
            float events = record / 5050;
            if (record < 5050) //this will update the file if the hour is changed?
            {
                Set_File_Position();
                return;
            }
            long this_event_start = -1;
            int evts_read = 0;
            int maxevt = 2000;
            bool bypass = false;
            int offset = 0;
            if (HttpContext.Current.Session["ReadAll"].ToString() == "1") maxevt = 99999999; 
                while (evts_read<maxevt)
            {
                offset += 25;
                string line = rs.ReadLine();
                if (line == null) return;
                string[] flags = new string[4];
                try
                {
                    flags[0] = line.Substring(0, 5);
                    flags[1] = line.Substring(6, 5);
                    flags[2] = line.Substring(12, 5);
                    flags[3] = line.Substring(18);
                    //string[] elements = line.Split(' ');
                    for (int i = 0; i < 4; i++) vflag[i] = (float)Convert.ChangeType(flags[i], typeof(float));
                    this_event_start=rs.BaseStream.Position;
                    if (vflag[0] != -99 || vflag[1] != -99 || vflag[2] != -99 || vflag[3] != -99)
                    {
                        if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                        {
                            while (vflag[0] != -99 || vflag[1] != -99 || vflag[2] != -99 || vflag[3] != -99)
                            {
                                line = rs.ReadLine();
                                if (line == null) return;
                                flags[0] = line.Substring(0, 5);
                                flags[1] = line.Substring(6, 5);
                                flags[2] = line.Substring(12, 5);
                                flags[3] = line.Substring(18);
                                //string[] elements = line.Split(' ');
                                for (int i = 0; i < 4; i++) vflag[i] = (float)Convert.ChangeType(flags[i], typeof(float));
                            }
                            // rs.BaseStream.Seek(this_event_start + offset, SeekOrigin.Begin);
                            bypass = true;
                        }
                        else Set_File_Position();
                    }
                }
                catch (Exception)
                {

                    if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                    {
                        //rs.BaseStream.Seek(this_event_start + offset, SeekOrigin.Begin);
                        bypass=true;
                    }
                    else throw;
                }

                ic++;
                if (line == null) return;
                offset += 25;
                line = rs.ReadLine();
                if (line == null) return;
                string[] elements = new string[4];
                try
                {
                    elements[0] = line.Substring(0, 5);
                    elements[1] = line.Substring(6, 5);
                    elements[2] = line.Substring(12, 5);
                    elements[3] = line.Substring(18);
                    //string[] elements = line.Split(' ');
                    for (int i = 0; i < 4; i++) peak[i] = (float)Convert.ChangeType(elements[i], typeof(float));
                }
                catch (Exception)
                {

                    if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                    {
                        //rs.BaseStream.Seek(this_event_start + offset, SeekOrigin.Begin);
                        bypass =true;
                    }
                    else throw;
                }
                string ss = HttpContext.Current.Session["Station"].ToString();
                int Station = (int)Convert.ChangeType(ss, typeof(int));
                int ch1 = -1, ch2 = -1, ch3 = -1;

                if (Station == 1) { ch1 = 1; ch2 = 2; ch3 = 3;}//234
                if (Station == 2) { ch1 = 0; ch2 = 2; ch3 = 3;}//134
                if (Station == 3) { ch1 = 0; ch2 = 1; ch3 = 3;}//124
                if (Station == 4) { ch1 = 0; ch2 = 1; ch3 = 2;}//123

                if (Station == 5) { ch1 = 1; ch2 = 2; ch3 = 3;}//234
                if (Station == 6) { ch1 = 0; ch2 = 2; ch3 = 3;}//134
                if (Station == 7) { ch1 = 0; ch2 = 1; ch3 = 3;}//124
                if (Station == 8) { ch1 = 0; ch2 = 1; ch3 = 2;}//123

                if (Station == 9)  { ch1 = 1; ch2 = 2; ch3 = 3;}//234
                if (Station == 10) { ch1 = 0; ch2 = 2; ch3 = 3;}//134
                if (Station == 11) { ch1 = 0; ch2 = 1; ch3 = 3;}//124
                if (Station == 12) { ch1 = 0; ch2 = 1; ch3 = 2;}//123

                if (Station == 13) { ch1 = 1; ch2 = 2; ch3 = 3;}//234
                if (Station == 14) { ch1 = 0; ch2 = 2; ch3 = 3;}//134
                if (Station == 15) { ch1 = 0; ch2 = 1; ch3 = 3;}//124
                if (Station >= 16) { ch1 = 0; ch2 = 1; ch3 = 2;}//123


                float[,] vv = new float[4, 200];
                for (int k = 0; k < 200; k++)
                {
                    offset += 25;
                    line = rs.ReadLine(); ic++;
                    while (line == null)
                    {
                        line = rs.ReadLine();
                        string ss9 = line;
                    }
                    try
                    {

                        elements[0] = line.Substring(0, 5);
                        elements[1] = line.Substring(6, 5);
                        elements[2] = line.Substring(12, 5);
                        elements[3] = line.Substring(18);
                        for (int i = 0; i < 4; i++) vv[i, k] = (float)Convert.ChangeType(elements[i], typeof(float));
                    }
                    catch (Exception)
                    {

                        if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                        {
                          //  rs.BaseStream.Seek(this_event_start + offset, SeekOrigin.Begin);
                          bypass = true;
                        }
                        else throw;
                    }
                }
                if (mode==0 || mode==2)
                {
                    float[] thres = new float[4];
                    thres[ch1] = (float)Convert.ChangeType(HttpContext.Current.Session["Calibration_thr1"].ToString(), typeof(float));
                    thres[ch2] = (float)Convert.ChangeType(HttpContext.Current.Session["Calibration_thr2"].ToString(), typeof(float));
                    thres[ch3] = (float)Convert.ChangeType(HttpContext.Current.Session["Calibration_thr3"].ToString(), typeof(float));
                    try
                    {
                        if (peak[ch1] > thres[ch1] && peak[ch2] > thres[ch2]) 
                        {
                            try
                            {
                                Save_Peaks(3, peak[ch3]);
                                if (HttpContext.Current.Session["ReadAll"].ToString() == "0")  Save_Mean_Pulse(3, ch3, vv);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        if (peak[ch1] > thres[ch1] && peak[ch3] > thres[ch3]) 
                        {
                            try
                            {
                                Save_Peaks(2, peak[ch2]);
                                if (HttpContext.Current.Session["ReadAll"].ToString() == "0") Save_Mean_Pulse(2, ch2, vv);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        if (peak[ch2] > thres[ch2] && peak[ch3] > thres[ch3]) 
                        {
                            try
                            {
                                Save_Peaks(1, peak[ch1]);
                                if (HttpContext.Current.Session["ReadAll"].ToString() == "0") Save_Mean_Pulse(1, ch1, vv);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                if (mode == 1 || mode==2)
                {
                    try
                    {
                        SaveDataTiming(ch1, ch2, ch3, vv, peak[ch1], peak[ch2], peak[ch3]);
                    }
                    catch (Exception)
                    {

                        if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                        {
                            rs.BaseStream.Seek(this_event_start + 5050, SeekOrigin.Begin);
                        }
                        else throw;
                    }
                }
                evts_read++;
            }
            Set_File_Position(); //we always set the position after read
        }

        void SaveDataTiming(int CH1, int CH2, int CH3, float[,] vv, float p1, float p2, float p3)
        {
            int Trigger = 0;
            int kmax = -1;
            double vmax = -1.0;
            int[] trg = new int[3] {-1,-1,-1 };
            float[] thr = new float[3];
            thr[0] = (float) Convert.ChangeType(HttpContext.Current.Session["Calibration_thr1_sync"].ToString(), typeof(float));
            thr[1] = (float)Convert.ChangeType(HttpContext.Current.Session["Calibration_thr2_sync"].ToString(), typeof(float));
            thr[2] = (float)Convert.ChangeType(HttpContext.Current.Session["Calibration_thr3_sync"].ToString(), typeof(float));
            for (int k = 0; k < 200; k++)
            {
                if (vv[CH3, k] > thr[2]) //always the last counter is the reference
                {
                    Trigger = 1;trg[2] = 1; break;
                }
            }
            if (Trigger == 0) return;
                        float[] tim = new float[3];
            tim[0] = (float)Convert.ChangeType(HttpContext.Current.Session["Calibration_tim1_sync"].ToString(), typeof(float));
            tim[1] = (float)Convert.ChangeType(HttpContext.Current.Session["Calibration_tim2_sync"].ToString(), typeof(float));
            tim[2] = (float)Convert.ChangeType(HttpContext.Current.Session["Calibration_tim3_sync"].ToString(), typeof(float));
            if (HttpContext.Current.Session["CFD"].ToString() == "1")
            {
                tim[0] = (float)(p1 * 0.2); //if (tim[0] < thr[0]) tim[0] = thr[0];  
                tim[1] = (float)(p2 * 0.2); //if (tim[1] < thr[1]) tim[1] = thr[1];
                tim[2] = (float)(p3 * 0.2); //if (tim[2] < thr[2]) tim[2] = thr[2];
            }
            float[] time = new float[3] { -1, -1, -1 };
            ////////////////////1st detector
            for (int k = 0; k < 200; k++)
            {
                if (vv[CH1, k] > thr[0])
                {
                    vmax = vv[CH1, k];
                    kmax = k; trg[0] = 1;
                    break;
                }
            }

            if (vmax>thr[0] )
            {
                for (int k = 1; k < 200; k++)
                {
                    if (time[0] < 0 && vv[CH1,k] > tim[0])
                    {
                        float v2 = vv[CH1,k];// vv; //vmax;
                        float v1 = vv[CH1, k-1];
                        float DV = v2 - v1;
                        float DT = 4;// (kmax - (k - 1)) * 4.0;
                        float dv = tim[0] - v1;
                        float dt = DT * dv / DV;
                        time[0] = (k - 1) * 4 + dt;
                        //tim[mych] = time[mych]; tim2[mych] = float(k) * 4.0;
                    }
                }
            }
            ////////////////////2nd detector
            for (int k = 0; k < 200; k++)
            {
                if (vv[CH2, k] > thr[1])
                {
                    vmax = vv[CH2, k];
                    kmax = k; trg[1] = 1;
                    break;
                }
            }

            if (vmax > thr[1])
            {
                for (int k = 1; k < 200; k++)
                {
                    if (time[1] < 0 && vv[CH2, k] > tim[1])
                    {
                        float v2 = vv[CH2, k];// vv; //vmax;
                        float v1 = vv[CH2, k - 1];
                        float DV = v2 - v1;
                        float DT = 4;// (kmax - (k - 1)) * 4.0;
                        float dv = tim[1] - v1;
                        float dt = DT * dv / DV;
                        time[1] = (k - 1) * 4 + dt;
                        //tim[mych] = time[mych]; tim2[mych] = float(k) * 4.0;
                    }
                }
            }

            ////////////////////3rd detector (Reference)
 
            if (trg[2]>0)
            {
                for (int k = 1; k < 200; k++)
                {
                    if (time[2] < 0 && vv[CH3, k] > tim[2])
                    {
                        float v2 = vv[CH3, k];// vv; //vmax;
                        float v1 = vv[CH3, k - 1];
                        float DV = v2 - v1;
                        float DT = 4;// (kmax - (k - 1)) * 4.0;
                        float dv = tim[2] - v1;
                        float dt = DT * dv / DV;
                        time[2] = (k - 1) * 4 + dt;
                        //tim[mych] = time[mych]; tim2[mych] = float(k) * 4.0;
                    }
                }
            }
            {
                float dummy = -9999;
                string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());
                string path = strPathCalibrationFolder + @"\timing.txt";
                if (trg[0] > 0 && trg[1] < 1)
                {
                    string ss1 = (time[2] - time[0]).ToString() + " " + dummy.ToString();
                    try
                    {
                        double ff = Convert.ToDouble((time[2] - time[0]).ToString());
                        File.AppendAllText(path, ss1 + "\n");
                    }catch (Exception ex)
                    {
                        ;
                    }

                }
                if (trg[0] < 1 && trg[1] > 0)
                {
                    string ss1 =  dummy.ToString()+" "+ (time[2] - time[1]).ToString();
                    try
                    {
                        double ff = Convert.ToDouble((time[2] - time[1]).ToString());
                        File.AppendAllText(path, ss1 + "\n");
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
                if (trg[0] > 0 && trg[1] > 0)
                {
                    string ss1 = (time[2] - time[0]).ToString() + " " + (time[2] - time[1]).ToString();
                    try
                    {
                        double gg = Convert.ToDouble((time[2] - time[1]).ToString());
                        double ff = Convert.ToDouble((time[2] - time[0]).ToString());
                        if (Math.Abs(ff) < 999 && Math.Abs(gg) <999) File.AppendAllText(path, ss1 + "\n");
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
            }
        }

        protected void Read_Events_and_Show_Protected()
        {
            string IsResponseStarted = HttpContext.Current.Session["Calibration_Response_Started"].ToString();
            string IsSyncStarted = HttpContext.Current.Session["Calibration_Sync_Started"].ToString();
            StreamReader rs = (StreamReader)HttpContext.Current.Session["Calibration_rs"];
            if (rs == null) { Set_File_Position(); rs = (StreamReader)HttpContext.Current.Session["Calibration_rs"]; }
            if (rs == null) return;
            long pos = rs.BaseStream.Position;
            try
            {
                Read_Events_and_Show();
            }
            catch (Exception)
            {
                long record = rs.BaseStream.Length - (pos + 5050);
                if (record > 5050)
                    rs.BaseStream.Seek(pos + 5050, SeekOrigin.Begin);
                else
                    rs.BaseStream.Seek(pos, SeekOrigin.Begin);
                //throw;
                if(IsResponseStarted=="1") //Read_Events_and_Show changes the session variable in case of failure. Catch the failure and revert
                HttpContext.Current.Session["Calibration_Response_Started"] = 1;
                if (IsSyncStarted == "1") //Read_Events_and_Show changes the session variable in case of failure. Catch the failure and revert
                    HttpContext.Current.Session["Calibration_Sync_Started"] = 1;
            }
        }

        protected void Read_Events_and_Show()
        {
            if (HttpContext.Current.Session["Calibration_Response_Started"] == null) return;
            if (HttpContext.Current.Session["Calibration_Sync_Started"] == null) return;
            int cal_res_started = (int)HttpContext.Current.Session["Calibration_Response_Started"];
            int cal_syn_started = (int)HttpContext.Current.Session["Calibration_Sync_Started"];
            if (cal_res_started == 1 && cal_syn_started == 0)
            {
                HttpContext.Current.Session["Calibration_Response_Started"] = 0;
                try
                {
                    Read_events(0);

                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());
                    string result = BatchCommand("script_calib_start.cmd", strPathCalibrationFolder);
                    HttpContext.Current.Session["Calibration_State"] = result;
                    string SessionId = HttpContext.Current.Session.SessionID;
                    string image = "outroot_" + SessionId + ".jpg";
                    string strPathName = @"~\images\" + image; string strRootRelativePathName = strPathCalibrationFolder.ToString() + @"\outroot.jpg";
                    File.Copy(strRootRelativePathName, Server.MapPath(strPathName), true);

                    string ss = img0.ImageUrl;
                    //img0.Attributes["src"] = "images/outroot1.jpg";
                    img0.ImageUrl = "images/" + image;
                    ss = img0.ImageUrl;
                    //img0.
                    HttpContext.Current.Session["Calibration_Response_Started"] = 1;
                }
            }
            if (cal_syn_started == 1 && cal_res_started == 0)
            {
                HttpContext.Current.Session["Calibration_Sync_Started"] = 0;
                try
                {
                    Read_events(1);
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());
                    string result = BatchCommand("script_sync_start.cmd", strPathCalibrationFolder);
                    HttpContext.Current.Session["Calibration_State"] = result;
                    string SessionId = HttpContext.Current.Session.SessionID;
                    string image = "outroot2_" + SessionId + ".jpg";
                    string strPathName = @"~\images\" + image; string strRootRelativePathName = strPathCalibrationFolder.ToString() + @"\outroot2.jpg";
                    File.Copy(strRootRelativePathName, Server.MapPath(strPathName), true);

                    string ss = img1.ImageUrl;
                    //img0.Attributes["src"] = "images/outroot1.jpg";
                    img1.ImageUrl = "images/" + image;
                    ss = img1.ImageUrl;
                    //img0.
                    HttpContext.Current.Session["Calibration_Sync_Started"] = 1;
                }
            }
            if (cal_res_started == 1 && cal_syn_started == 1)
            {
                HttpContext.Current.Session["Calibration_Response_Started"] = 0;
                HttpContext.Current.Session["Calibration_Sync_Started"] = 0;
                try
                {
                    Read_events(2);

                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());
                    string result = BatchCommand("script_calib_start.cmd", strPathCalibrationFolder);
                    HttpContext.Current.Session["Calibration_State"] = result;
                    string SessionId = HttpContext.Current.Session.SessionID;
                    string image = "outroot_" + SessionId + ".jpg";
                    string strPathName = @"~\images\" + image; string strRootRelativePathName = strPathCalibrationFolder.ToString() + @"\outroot.jpg";
                    File.Copy(strRootRelativePathName, Server.MapPath(strPathName), true);

                    string ss = img0.ImageUrl;
                    //img0.Attributes["src"] = "images/outroot1.jpg";
                    img0.ImageUrl = "images/" + image;
                    ss = img0.ImageUrl;
                    //img0.
                    HttpContext.Current.Session["Calibration_Response_Started"] = 1;
                    result = BatchCommand("script_sync_start.cmd", strPathCalibrationFolder);
                    HttpContext.Current.Session["Calibration_State"] = result;
                    image = "outroot2_" + SessionId + ".jpg";
                    strPathName = @"~\images\" + image; strRootRelativePathName = strPathCalibrationFolder.ToString() + @"\outroot2.jpg";
                    File.Copy(strRootRelativePathName, Server.MapPath(strPathName), true);

                    ss = img1.ImageUrl;
                    //img0.Attributes["src"] = "images/outroot1.jpg";
                    img1.ImageUrl = "images/" + image;
                    ss = img1.ImageUrl;
                    //img0.
                    HttpContext.Current.Session["Calibration_Sync_Started"] = 1;

                }
            }

        }
        protected void Timer1_Tick(object sender, EventArgs e)
        {
            //Timer1.Enabled = false;
            //Timer1.Interval=9999999;
            Read_Events_and_Show_Protected();
            //Timer1.Interval = 120000;
            Timer1.Enabled = true;
            //Show_Response_Tab();
        }

        protected void Tab_Response_Click(object sender, EventArgs e)
        {
          HttpContext.Current.Session["Active_Tab"] = 0;
            Show_Response_Tab();
        }
        protected void Tab_Sync_Click(object sender, EventArgs e)
        {
           HttpContext.Current.Session["Active_Tab"] = 1;
            Show_Sync_Tab();
        }
        protected void Show_Response_Tab()
        {
            tab14b7.Attributes["class"] = "u-align-left u-black u-container-style u-tab-pane u-tab-pane-2";
            tab0da5.Attributes["class"] = "u-black u-container-style u-tab-active u-tab-pane u-tab-pane-1";
            linktab14b7.Attributes["class"] = "u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";
            linktab0da5.Attributes["class"] = "active u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1 u-tab-link-1";
        }
        protected void Show_Sync_Tab()
        {
            //tab14b7.Attributes["class"] = "u-align-left u-black u-container-style u-tab-pane u-tab-pane-2";
            //tab0da5.Attributes["class"] = "u-black u-container-style u-tab-active u-tab-pane u-tab-pane-1";
            string s1 = tab14b7.Attributes["class"].ToString();
            string s2 = tab0da5.Attributes["class"].ToString();
            tab14b7.Attributes["class"] = "u-align-left u-black u-container-style u-tab-active u-tab-pane u-tab-pane-2";
            tab0da5.Attributes["class"] = "u-black u-container-style  u-tab-pane u-tab-pane-1";
            //linktab14b7.Attributes["class"] = "u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1 u-tab-link u-tab-link-2";
            //linktab0da5.Attributes["class"] = "active u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1 u-tab-link u-tab-link-1"
            linktab14b7.Attributes["class"] = "active u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";
            linktab0da5.Attributes["class"] = "u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1 u-tab- u-tab-link-1";
        }
        //protected void Timer2_Tick(object sender, EventArgs e)
        //{
        //    Read_Events_and_Show_Protected();
        //    //Show_Sync_Tab();
        //}
        protected void Stop_Acquisition_Response(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "0") return;
            HttpContext.Current.Session["Calibration_Response_Started"] = 0;
            if (HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "0") this.Timer1.Enabled = false;
            if (HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "0") HttpContext.Current.Session["SetFilePosition"] = "";// if he resumes he will not change the position if the other is running
            ShowStatusResponse();
            Response.Write("<script>alert('" + "Acquisition stopped. To resume without resetting the plots, press start." + "')</script>");
        }
        protected void Stop_Acquisition_Sync(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "0") return;
                HttpContext.Current.Session["Calibration_Sync_Started"] = 0;
            if (HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "0") this.Timer1.Enabled = false;
            if (HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "0") HttpContext.Current.Session["SetFilePosition"] = "";// if he resumes he will not change the position if the other is running
            ShowStatusSync();
            Response.Write("<script>alert('" + "Acquisition stopped. To resume without resetting the plots, press start." + "')</script>");
        }
        protected void Clear_All(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["Calibration_Sync_Started"].ToString() == "1")
            {
                Response.Write("<script>alert('" + "You should stop the acquisition first." + "')</script>");
                return;
            }
            string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());
            string path = strPathCalibrationFolder + @"\timing.txt";
            File.Delete(path);
            Response.Write("<script>alert('" + "Distributions Cleared!" + "')</script>");
            //var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        }
        protected void Clear_Pulses(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "1")
            {
                Response.Write("<script>alert('" + "You should stop the acquisition first." + "')</script>");
                return;
            }
            float[,] myDoubleArray = new float[3, 400];
            HttpContext.Current.Session["spm"] = myDoubleArray;
            HttpContext.Current.Session["npm"] = new int[] { 0, 0, 0 };
            
            string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());
            string path = strPathCalibrationFolder + @"\pulse1.txt";
            File.Delete(path);
            //var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            path = strPathCalibrationFolder + @"\pulse2.txt";
            File.Delete(path);
            //file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            path = strPathCalibrationFolder + @"\pulse3.txt";
            File.Delete(path);
            //file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            Response.Write("<script>alert('" + "Pulses cleared!" + "')</script>");

        }

        protected void Clear_Distributions(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["Calibration_Response_Started"].ToString() == "1")
            {
                Response.Write("<script>alert('" + "You should stop the acquisition first." + "')</script>");
                return;
            }
            lock (thisLock)
            {
            string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Calibration_folder"].ToString());
            string path = strPathCalibrationFolder + @"\test1.txt";
            File.Delete(path);
            //var file = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
            path = strPathCalibrationFolder + @"\test2.txt";
                File.Delete(path);
            //file = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
            path = strPathCalibrationFolder + @"\test3.txt";
            File.Delete(path);
            //file = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
            }
            Response.Write("<script>alert('" + "Distributions Cleared!" + "')</script>");

        }

        protected void text_thr1_changed(object sender, EventArgs e)
        {
            if (!GraterValue(text_thr1.Text, 5.0)) return;
            HttpContext.Current.Session["Calibration_thr1"] = text_thr1.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            Stop_Acquisition_Response(sender, e);
        }
        protected void text_thr2_changed(object sender, EventArgs e)
        {
            if (!GraterValue(text_thr2.Text, 5.0)) return;
            HttpContext.Current.Session["Calibration_thr2"] = text_thr2.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            Stop_Acquisition_Response(sender, e);
        }
        protected void text_thr3_changed(object sender, EventArgs e)
        {
            if (!GraterValue(text_thr2.Text, 5.0)) return;
            HttpContext.Current.Session["Calibration_thr3"] = text_thr3.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            Stop_Acquisition_Response(sender, e);
        }
        protected void text_thr1_sync_changed(object sender, EventArgs e)
        {
            if (!GraterValue(text_thr1_sync.Text, 5.0)) return;
            if (!GraterValue(text_thr1_sync.Text, Convert.ToDouble(Tim1.Text))) return;
            HttpContext.Current.Session["Calibration_thr1_sync"] = text_thr1_sync.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            Stop_Acquisition_Response(sender, e);
        }
        protected void text_thr2_sync_changed(object sender, EventArgs e)
        {
            if (!GraterValue(text_thr2_sync.Text, 5.0)) return;
            if (!GraterValue(text_thr2_sync.Text, Convert.ToDouble(Tim2.Text))) return;
            HttpContext.Current.Session["Calibration_thr2_sync"] = text_thr2_sync.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            Stop_Acquisition_Response(sender, e);
        }
        protected void text_thr3_sync_changed(object sender, EventArgs e)
        {
            if (!GraterValue(text_thr3_sync.Text, 5.0)) return;
            if (!GraterValue(text_thr3_sync.Text, Convert.ToDouble(Tim3.Text))) return;
            HttpContext.Current.Session["Calibration_thr3_sync"] = text_thr3_sync.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            Stop_Acquisition_Response(sender, e);
        }

        protected void text_tim1_sync_changed(object sender, EventArgs e)
        {
            if (!GraterValue(Tim1.Text, 5.0)) { return; }
            if (!LessValue(Tim1.Text,Convert.ToDouble(text_thr1_sync.Text))) return;
            HttpContext.Current.Session["Calibration_tim1_sync"] = Tim1.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            Stop_Acquisition_Response(sender, e);
        }
        protected void text_tim2_sync_changed(object sender, EventArgs e)
        {
            if (!GraterValue(Tim2.Text, 5.0)) return;
            if (!LessValue(Tim2.Text, Convert.ToDouble(text_thr2_sync.Text))) return;
            HttpContext.Current.Session["Calibration_tim2_sync"] = Tim2.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            Stop_Acquisition_Response(sender, e);
        }
        protected void text_tim3_sync_changed(object sender, EventArgs e)
        {
            if (!GraterValue(Tim3.Text, 5.0)) return;
            if (!LessValue(Tim3.Text, Convert.ToDouble(text_thr3_sync.Text))) return;
            HttpContext.Current.Session["Calibration_tim3_sync"] = Tim3.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            Stop_Acquisition_Response(sender, e);
        }

        protected bool GraterValue(string text, double threshold)
        {
            double t = Convert.ToDouble(text);
            string ll =Convert.ToString(threshold);
            if (t <threshold)
            {
                Response.Write("<script>alert('" + "Select value grater than " + ll+" ')</script>");
                return false;
            }
            return true;
        }

        protected bool LessValue(string text, double threshold)
        {
            double t = Convert.ToDouble(text);
            string ll = Convert.ToString(threshold);
            if (t > threshold)
            {
                Response.Write("<script>alert('" + "Select timing value less than " + ll + " (detector threshold) or change detector threshold')</script>");
                return false;
            }
            return true;
        }

        protected void Check_Clicked(Object sender, EventArgs e)
        {
            if (checkbox1.Checked == true)
            {
                HttpContext.Current.Session["CFD"] = 1;
            }
            else
                HttpContext.Current.Session["CFD"] = 0;
        }


    }
}