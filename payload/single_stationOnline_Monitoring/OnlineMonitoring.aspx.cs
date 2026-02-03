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
using System.Threading;
namespace WebApplication2
{
    public partial class WebForm4 : System.Web.UI.Page
    {
        static int ic = 0;
        static int myYear = 2022;
        static int myMonth = 7;
        static int myDay =21;//6,6 21,6  6,7  21,7
        static int myHour = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpContext.Current.Session["ReadAll"] = 0;// use 1 to read  all previous files and save them in D:Save_Pulses_Showers_Rec
            HttpContext.Current.Session["scan"] = 0;// use 1 to scan all pulses by pressing start_acquisition
            if (HttpContext.Current.Session["Station"] == null) Response.Redirect("StationSelection");
            if (HttpContext.Current.Session["Station"] != null && HttpContext.Current.Session["Station"].ToString() == "0")
            {
                //Response.Write("<script>alert('" + "Choose station please!" + "')</script>");
                Response.Redirect("StationSelection");
                return;
            }
            // activetabs?????
            if (HttpContext.Current.Session["Active_Tab"] != null)
            {
                if (HttpContext.Current.Session["Active_Tab"].ToString() == "0") Show_Parameters_Tab();
                if (HttpContext.Current.Session["Active_Tab"].ToString() == "1") Show_Event_Tab();
                if (HttpContext.Current.Session["Active_Tab"].ToString() == "2") Show_Histograms_Tab();
            }

            if(Date1.Text =="")
            {
                ;
            }

            if (HttpContext.Current.Session["Shower_thr1"] == null) HttpContext.Current.Session["Shower_thr1"] = "5.0";
            if (HttpContext.Current.Session["Shower_thr2"] == null) HttpContext.Current.Session["Shower_thr2"] = "5.0";
            if (HttpContext.Current.Session["Shower_thr3"] == null) HttpContext.Current.Session["Shower_thr3"] = "5.0";
            if (HttpContext.Current.Session["Shower_peak1"] == null) HttpContext.Current.Session["Shower_peak1"] = "15.0";
            if (HttpContext.Current.Session["Shower_peak2"] == null) HttpContext.Current.Session["Shower_peak2"] = "15.0";
            if (HttpContext.Current.Session["Shower_peak3"] == null) HttpContext.Current.Session["Shower_peak3"] = "15.0";
            if (HttpContext.Current.Session["Shower_tim1"] == null) HttpContext.Current.Session["Shower_tim1"] = "5.0";
            if (HttpContext.Current.Session["Shower_tim2"] == null) HttpContext.Current.Session["Shower_tim2"] = "5.0";
            if (HttpContext.Current.Session["Shower_tim3"] == null) HttpContext.Current.Session["Shower_tim3"] = "5.0";
            if (HttpContext.Current.Session["Shower_off1"] == null) HttpContext.Current.Session["Shower_off1"] = "0.0";
            if (HttpContext.Current.Session["Shower_off2"] == null) HttpContext.Current.Session["Shower_off2"] = "0.0";
            if (HttpContext.Current.Session["Shower_off3"] == null) HttpContext.Current.Session["Shower_off3"] = "0.0";
            if (HttpContext.Current.Session["CFD_showers"] == null) HttpContext.Current.Session["CFD_showers"] = 0; //initial value
            if (HttpContext.Current.Session["Run_StartTime"] == null) HttpContext.Current.Session["Run_StartTime"]="00/00/00 00:00:00";
            if (HttpContext.Current.Session["Run_RunningTime"] == null)  HttpContext.Current.Session["Run_RunningTime"]="0 days, 0 hours, 0 minutes";
            if (HttpContext.Current.Session["Run_DetectionRate"] == null) HttpContext.Current.Session["Run_DetectionRate"]="0 per hour";
            if (HttpContext.Current.Session["Run_ReconstructionRate"] == null) HttpContext.Current.Session["Run_ReconstructionRate"]="0 per hour";
            if (HttpContext.Current.Session["Run_TotalEvents"] == null) HttpContext.Current.Session["Run_TotalEvents"]="0";
            if (HttpContext.Current.Session["Run_ReconstructedEvents"] == null) HttpContext.Current.Session["Run_ReconstructedEvents"]="0";
            if (HttpContext.Current.Session["Run_ReconstructionFailureRate"] == null) HttpContext.Current.Session["Run_ReconstructionFailureRate"]="0%";
            if (HttpContext.Current.Session["event"] == null) { HttpContext.Current.Session["event"] = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; } //times, peaks, charge,theta,phi
            double[,] offvalues = new double[16, 3];
            double[,] offvaluesCDF = new double[16, 3];
            if (HttpContext.Current.Session["positions"] == null) 
            {
                string DetectorPositions = Server.MapPath(@"~\ProgramFiles\positions.txt");
                double[,] position = new double[4, 3];
                if (File.Exists(DetectorPositions))
                {
                    var file = new FileStream(DetectorPositions, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader rs = new StreamReader(file);
                    rs.BaseStream.Seek(0, SeekOrigin.Begin);

                    for (int i = 0; i < 3; i++)
                    {
                        string line = rs.ReadLine();
                        string[] flags = new string[3];
                        flags = line.Split(',');
                        //flags[1] = line.Substring(5, 4);
                        //flags[2] = line.Substring(10);
                        position[i, 0] = (float)Convert.ChangeType(flags[0], typeof(float));
                        position[i, 1] = (float)Convert.ChangeType(flags[1], typeof(float));
                        position[i, 2] = (float)Convert.ChangeType(flags[2], typeof(float));
                    }

                }
                else 
                {
                    position[0, 0] = -15.0; position[0, 1] = 525.0; position[0, 2] = 0.0;// where we have the offfice
                    position[1, 0] = 0.0; position[1, 1] = 0.0; position[1, 2] = 0.0;// where we have the cafeteria
                    position[2, 0] = 480.0; position[2, 1] = -70.0; position[2, 2] = 0.0;// where we have the phone
                    position[3, 0] = 540.0; position[3, 1] = 525.0; position[3, 2] = 0.0;// where we have the window

                }


                HttpContext.Current.Session["positions"] = position;
                string ss = HttpContext.Current.Session["Station"].ToString();
                int Station = (int)Convert.ChangeType(ss, typeof(int));
                int ch1 = -1, ch2 = -1, ch3 = -1;
                if (Station == 1) { ch1 = 1; ch2 = 2; ch3 = 3; }//234
                if (Station == 2) { ch1 = 0; ch2 = 2; ch3 = 3; }//134
                if (Station == 3) { ch1 = 0; ch2 = 1; ch3 = 3; }//124
                if (Station == 4) { ch1 = 0; ch2 = 1; ch3 = 2; }//123

                if (Station == 5) { ch1 = 1; ch2 = 2; ch3 = 3; }//234
                if (Station == 6) { ch1 = 0; ch2 = 2; ch3 = 3; }//134
                if (Station == 7) { ch1 = 0; ch2 = 1; ch3 = 3; }//124
                if (Station == 8) { ch1 = 0; ch2 = 1; ch3 = 2; }//123

                if (Station == 9) { ch1 = 1; ch2 = 2; ch3 = 3; }//234
                if (Station == 10) { ch1 = 0; ch2 = 2; ch3 = 3; }//134
                if (Station == 11) { ch1 = 0; ch2 = 1; ch3 = 3; }//124
                if (Station == 12) { ch1 = 0; ch2 = 1; ch3 = 2; }//123

                if (Station == 13) { ch1 = 1; ch2 = 2; ch3 = 3; }//234
                if (Station == 14) { ch1 = 0; ch2 = 2; ch3 = 3; }//134
                if (Station == 15) { ch1 = 0; ch2 = 1; ch3 = 3; }//124
                if (Station >= 16) { ch1 = 0; ch2 = 1; ch3 = 2; }//123
                double[] xx = new double[3] { 0, 0, 0 };
                double[] yy = new double[3] { 0, 0, 0 };
                double[] zz = new double[3] { 0, 0, 0 };
                xx[0] = position[ch1, 0]; yy[0] = position[ch1, 1]; zz[0] = position[ch1, 2];
                xx[1] = position[ch2, 0]; yy[1] = position[ch2, 1]; zz[1] = position[ch2, 2];
                xx[2] = position[ch3, 0]; yy[2] = position[ch3, 1]; zz[2] = position[ch3, 2];
                string pos1 = String.Format("(x1,y1,z1)=({0,2:0.00},{1,2:0.00},{2,2:0.00})", xx[0] / 100.0, yy[0] / 100.0, zz[0] / 100.0);
                string pos2 = String.Format("(x2,y2,z2)=({0,2:0.00},{1,2:0.00},{2,2:0.00})", xx[1] / 100.0, yy[1] / 100.0, zz[1] / 100.0);
                string pos3 = String.Format("(x3,y3,z3)=({0,2:0.00},{1,2:0.00},{2,2:0.00})", xx[2] / 100.0, yy[2] / 100.0, zz[2] / 100.0);
                HttpContext.Current.Session["Detector_Pos1"] = pos1;
                HttpContext.Current.Session["Detector_Pos2"] = pos2;
                HttpContext.Current.Session["Detector_Pos3"] = pos3;

                if (Station == 1) { offvalues[Station - 1, 0] = 1.1; offvalues[Station - 1, 1] = 0.02; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 2) {offvalues[Station - 1, 0] = 1.9; offvalues[Station - 1, 1] = 0.02; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 3) {offvalues[Station - 1, 0] = 1.9; offvalues[Station - 1, 1] = 1.12; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 4) {offvalues[Station - 1, 0] = 1.94; offvalues[Station - 1, 1] = 1.15; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 5) {offvalues[Station - 1, 0] = 2.1; offvalues[Station - 1, 1] = 0.7; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 6) {offvalues[Station - 1, 0] = 0.94; offvalues[Station - 1, 1] = 0.72; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 7) {offvalues[Station - 1, 0] = 0.94; offvalues[Station - 1, 1] = 2.11; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 8) {offvalues[Station - 1, 0] = 0.26; offvalues[Station - 1, 1] = 1.43; offvalues[Station - 1, 2] = 0.0; }

                if (Station == 9) {offvalues[Station - 1, 0] = -0.01; offvalues[Station - 1, 1] = -0.02; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 10) {offvalues[Station - 1, 0] = 14.37; offvalues[Station - 1, 1] = -0.02; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 11) {offvalues[Station - 1, 0] = 14.37; offvalues[Station - 1, 1] = -0.01; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 12) {offvalues[Station - 1, 0] = 14.45; offvalues[Station - 1, 1] = 0.08; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 13) {offvalues[Station - 1, 0] = 24.93; offvalues[Station - 1, 1] = 27.85; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 14) {offvalues[Station - 1, 0] = -1.5; offvalues[Station - 1, 1] = 27.85; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 15) {offvalues[Station - 1, 0] = -1.5; offvalues[Station - 1, 1] = 24.93; offvalues[Station - 1, 2] = 0.0; }
                if (Station == 16) {offvalues[Station - 1, 0] = -29.31; offvalues[Station - 1, 1] = -2.78; offvalues[Station - 1, 2] = 0.0; }
                HttpContext.Current.Session["OffValues"] = offvalues;

                if (Station == 1) {offvaluesCDF[Station - 1, 0] = 0.68; offvaluesCDF[Station - 1, 1] = 0.79; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 2) {offvaluesCDF[Station - 1, 0] = 0.87; offvaluesCDF[Station - 1, 1] = 0.79; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 3) {offvaluesCDF[Station - 1, 0] = 0.87; offvaluesCDF[Station - 1, 1] = 0.68; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 4) {offvaluesCDF[Station - 1, 0] = 0.06; offvaluesCDF[Station - 1, 1] = -0.12; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 5) {offvaluesCDF[Station - 1, 0] = 0.84; offvaluesCDF[Station - 1, 1] = 0.56; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 6) {offvaluesCDF[Station - 1, 0] = 0.56; offvaluesCDF[Station - 1, 1] = 0.56; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 7) {offvaluesCDF[Station - 1, 0] = 0.56; offvaluesCDF[Station - 1, 1] = 0.84; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 8) {offvaluesCDF[Station - 1, 0] = 0.02; offvaluesCDF[Station - 1, 1] = 0.28; offvaluesCDF[Station - 1, 2] = 0.0; }

                if (Station == 9) {offvaluesCDF[Station - 1, 0] = -0.49; offvaluesCDF[Station - 1, 1] = 0.07; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 10) {offvaluesCDF[Station - 1, 0] = 13.93; offvaluesCDF[Station - 1, 1] = 0.07; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 11) {offvaluesCDF[Station - 1, 0] = 13.93; offvaluesCDF[Station - 1, 1] = -0.49; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 12) {offvaluesCDF[Station - 1, 0] = 13.89; offvaluesCDF[Station - 1, 1] = -0.55; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 13) {offvaluesCDF[Station - 1, 0] = 25.17; offvaluesCDF[Station - 1, 1] = 25.28; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 14) {offvaluesCDF[Station - 1, 0] = 0.33; offvaluesCDF[Station - 1, 1] = 25.28; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 15) {offvaluesCDF[Station - 1, 0] = 0.33; offvaluesCDF[Station - 1, 1] = 25.17; offvaluesCDF[Station - 1, 2] = 0.0; }
                if (Station == 16) {offvaluesCDF[Station - 1, 0] = -24.93; offvaluesCDF[Station - 1, 1] = -0.12; offvaluesCDF[Station - 1, 2] = 0.0; }
                HttpContext.Current.Session["OffValuesCDF"] = offvaluesCDF;
                HttpContext.Current.Session["Shower_off1"]= offvalues[Station - 1, 0].ToString();
                HttpContext.Current.Session["Shower_off2"] = offvalues[Station - 1, 1].ToString();
                HttpContext.Current.Session["Shower_off3"] = offvalues[Station - 1, 2].ToString();
            }
            if (HttpContext.Current.Session["Event_DateTime"] == null) HttpContext.Current.Session["Event_DateTime"]= "00/00/00 00:00:00";
            if (HttpContext.Current.Session["Event_PulseArrivalTime"] == null) HttpContext.Current.Session["Event_PulseArrivalTime"] = " (T1,T2,T3)=(0.00,0.00,0.00) ns";
            if (HttpContext.Current.Session["Event_PulsePeaks"] == null) HttpContext.Current.Session["Event_PulsePeaks"] = " (p1,p2,p3)=(0.00,0.00,0.00) mV";
            if (HttpContext.Current.Session["Event_PulseCharges"] == null) HttpContext.Current.Session["Event_PulseCharges"] = " (ch1,ch2,ch3)=(0.00,0.00,0.00) pC";
            if (HttpContext.Current.Session["Event_Zenith"] == null) HttpContext.Current.Session["Event_Zenith"] = "-";
            if (HttpContext.Current.Session["Event_Azimuth"] == null) HttpContext.Current.Session["Event_Azimuth"] = "-";
            HttpContext.Current.Session["Shower_Current_File"] = "nofile";

            if (!IsPostBack)
            {
                //retain the entried in the edit-boxes
                if (HttpContext.Current.Session["Shower_thr1"] != null) text_thr1.Text = HttpContext.Current.Session["Shower_thr1"].ToString();
                if (HttpContext.Current.Session["Shower_thr2"] != null) text_thr2.Text = HttpContext.Current.Session["Shower_thr2"].ToString();
                if (HttpContext.Current.Session["Shower_thr3"] != null) text_thr3.Text = HttpContext.Current.Session["Shower_thr3"].ToString();
                if (HttpContext.Current.Session["Shower_peak1"] != null) Peak1.Text = HttpContext.Current.Session["Shower_peak1"].ToString();
                if (HttpContext.Current.Session["Shower_peak2"] != null) Peak2.Text = HttpContext.Current.Session["Shower_peak2"].ToString();
                if (HttpContext.Current.Session["Shower_peak3"] != null) Peak3.Text = HttpContext.Current.Session["Shower_peak3"].ToString();
                if (HttpContext.Current.Session["Shower_tim1"] != null) Tim1.Text = HttpContext.Current.Session["Shower_tim1"].ToString();
                if (HttpContext.Current.Session["Shower_tim2"] != null) Tim2.Text = HttpContext.Current.Session["Shower_tim2"].ToString();
                if (HttpContext.Current.Session["Shower_tim3"] != null) Tim3.Text = HttpContext.Current.Session["Shower_tim3"].ToString();
                if (HttpContext.Current.Session["Shower_off1"] != null) Off1.Text = HttpContext.Current.Session["Shower_off1"].ToString();
                if (HttpContext.Current.Session["Shower_off2"] != null) Off2.Text = HttpContext.Current.Session["Shower_off2"].ToString();
                if (HttpContext.Current.Session["Shower_off3"] != null) Off3.Text = HttpContext.Current.Session["Shower_off3"].ToString();
                if (HttpContext.Current.Session["CFD_showers"] != null)
                {
                    checkbox1.Checked = false;
                    if (HttpContext.Current.Session["CFD_showers"].ToString() == "1") checkbox1.Checked = true;
                }
                ShowRunInfo();
                ShowEventInfo();
            }

            //create directory
            string SessionId = HttpContext.Current.Session.SessionID;
            if (HttpContext.Current.Session["Shower_folder"] == null)
            {
                Timer1.Enabled = false;
                HttpContext.Current.Session["Shower_Acquisition_Started"] = 0;

                DateTime lt = DateTime.Now;
                string dateflag = lt.Year.ToString() + "_" + lt.Month.ToString() + "_" + lt.Day.ToString() + "_" + lt.Hour.ToString() + "_" + lt.Minute.ToString() + "_" + lt.Second.ToString();
                HttpContext.Current.Session["Shower_folder"] = "shower_" + SessionId + "_" + HttpContext.Current.Session["Station"].ToString() + "_" + dateflag;
                string strRootRelativePathName = @"~\App_Data\" + "shower_" + SessionId + "_" + HttpContext.Current.Session["Station"].ToString() + "_" + dateflag;
                string datetime = System.DateTime.Now.ToString();
                HttpContext.Current.Session["Shower_datetime"] = datetime;
                string strPathName =
                    Server.MapPath(strRootRelativePathName);
                if (System.IO.File.Exists(strPathName) == false)
                {
                    Directory.CreateDirectory(strPathName);
                }
                string strPathName1 = strPathName + @"\pulses.C";
                string strPathName2 = strPathName + @"\monitoring.C";
                strRootRelativePathName = @"~\ProgramFiles\pulses.C";
                File.Copy(Server.MapPath(strRootRelativePathName), strPathName1, true);
                strRootRelativePathName = @"~\ProgramFiles\monitoring.C";
                File.Copy(Server.MapPath(strRootRelativePathName), strPathName2, true);
            }
            else
            {
                if  (HttpContext.Current.Session["Shower_Acquisition_Started"].ToString() == "1") Timer1.Enabled = true ;
            }

        }

        protected void Start_Acquisition(object sender, EventArgs e)
        {
            if (!check_run_values())
            {
                Response.Write("<script>alert('" + "Parametere values are wrong! Please correct them." + "')</script>");
                return;
            }

            if (HttpContext.Current.Session["scan"].ToString()=="0" && HttpContext.Current.Session["Shower_Acquisition_Started"].ToString() == "1") // in case of scan dont check
            {
                Response.Write("<script>alert('" + "Acquisition allready started! Maybe you want to stop the acquisition first." + "')</script>");
                return;
            }
            Response.Write("<script>alert('" + "Shower Acquisition starting." + "')</script>");

            string strPathShowerFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Shower_folder"].ToString());

            if (Directory.Exists(strPathShowerFolder) == false)
            {
                Response.Write("<script>alert('" + "Directory does not exist. This should not happen! ErrorCode:001" + "')</script>");
            }
            string strRootRelativePathName = Server.MapPath(@"~\ProgramFiles\script_shower_start.cmd");
            if (File.Exists(strRootRelativePathName))
                File.Copy(strRootRelativePathName, strPathShowerFolder + @"\script_shower_start.cmd", true);
            StreamWriter rs = File.CreateText(strPathShowerFolder + @"\info_shower.txt");

            DateTime lt = DateTime.Now;
            string dateflag = string.Format("{0,2:d2}/{1,2:d2}/{2,2:d2} {3,2:d2}:{4,2:d2}:{5,2:d2}", lt.Day, lt.Month, lt.Year, lt.Hour, lt.Minute, lt.Second);
            HttpContext.Current.Session["Run_StartTime"] = dateflag;
            HttpContext.Current.Session["Run_DateTime"] = lt;
            rs.WriteLine(dateflag);
            rs.WriteLine(text_thr1.Text);
            rs.WriteLine(text_thr2.Text);
            rs.WriteLine(text_thr3.Text);
            rs.WriteLine(Tim1.Text);
            rs.WriteLine(Tim2.Text);
            rs.WriteLine(Tim3.Text);
            rs.WriteLine(Off1.Text);
            rs.WriteLine(Off2.Text);
            rs.WriteLine(Off3.Text);
            rs.WriteLine(Peak1.Text);
            rs.WriteLine(Peak2.Text);
            rs.WriteLine(Peak3.Text);
            if (checkbox1.Checked == true) rs.WriteLine("CDF selected");  else rs.WriteLine("Threshold timing selected"); 
            rs.WriteLine(HttpContext.Current.Session["Run_StartTime"].ToString());
            rs.WriteLine(HttpContext.Current.Session["Station"].ToString());
            rs.Close();

            if (HttpContext.Current.Session["SetFilePositionShower"] == null || (HttpContext.Current.Session["ReadAll"].ToString() == "1"))
            {
                if (HttpContext.Current.Session["scan"].ToString() == "0") Set_File_Position();
                HttpContext.Current.Session["SetFilePositionShower"] = true;
            }
            HttpContext.Current.Session["Shower_Acquisition_Started"] = 1;

            if (HttpContext.Current.Session["ReadAll"].ToString() == "0") this.Timer1.Enabled = true;
            Read_Events_and_Show_Protected();
            //Show_Response_Tab();
            ShowRunInfo();
            ShowEventInfo();
        }

        protected void Stop_Acquisition(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["Shower_Acquisition_Started"].ToString() == "0") return;
            HttpContext.Current.Session["Shower_Acquisition_Started"] = 0;
            this.Timer1.Enabled = false;
            Response.Write("<script>alert('" + "Acquisition stopped. To resume, press start." + "')</script>");
        }
        protected bool Set_File_Position()
        {
            if (HttpContext.Current.Session["Hantek"] == null)
            {
                Response.Write("<script>alert('" + "Hantek does not exist. This should not happen! ErrorCode:002" + "')</script>");
                return false;
            }
            string Hantek = HttpContext.Current.Session["Hantek"].ToString();

            DateTime lt = DateTime.Now;
            string filename = Hantek + "_" + lt.Year.ToString() + "_" + lt.Month.ToString() + "_" + lt.Day.ToString() + "_" + lt.Hour.ToString() + ".showerdata";

            int[] eventTime = new int[7];
            eventTime[0] = lt.Year;
            eventTime[1] = lt.Month;
            eventTime[2] = lt.Day;
            eventTime[3] = lt.Hour;
            HttpContext.Current.Session["EventTime"] = eventTime;
            //string strRootRelativePathName = Server.MapPath(@"~\ProgramFiles\Save_Pulses_Calibration\" + filename);

            if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
            {
                if (HttpContext.Current.Session["Shower_rs"]!=null) 
                {
                    StreamReader rs1 = (StreamReader)HttpContext.Current.Session["Shower_rs"];
                    if(rs1.BaseStream.Position != rs1.BaseStream.Length)  // IF WE CAME HERE WITHOUT READING ALL FILE
                    {
                        myHour++;
                        if (myHour == 24) { myHour = 0; myDay++; }
                        if (myDay == 32) { myDay = 1; myMonth++; }
                        if (myMonth == 13) { myMonth = 1; myYear++; }
                    }
                }
                filename = Hantek + "_" + myYear.ToString() + "_" + myMonth.ToString() + "_" + myDay.ToString() + "_" + myHour.ToString() + ".showerdata";
                string strRootRelativePathName1 = @"D:\Save_Pulses_Showers_Phase2\" + filename;
                while (!File.Exists(strRootRelativePathName1))
                {
                    myHour++;
                    if (myHour == 24) { myHour = 0; myDay++; }
                    if (myDay == 32) { myDay = 1; myMonth++; }
                    if (myMonth == 13) { myMonth = 1; myYear++; }
                    filename = Hantek + "_" + myYear.ToString() + "_" + myMonth.ToString() + "_" + myDay.ToString() + "_" + myHour.ToString() + ".showerdata";
                    strRootRelativePathName1 = @"D:\Save_Pulses_Showers_Phase2\" + filename;
                    if(myYear>2025)
                    {
                        return false;
                    }
                }

                eventTime[0] = myYear;
                eventTime[1] = myMonth;
                eventTime[2] = myDay;
                eventTime[3] = myHour;
                HttpContext.Current.Session["EventTime"] = eventTime;
                HttpContext.Current.Session["SaveFile"] = @"D:\Save_Pulses_Showers_Rec_Phase2\events_" + HttpContext.Current.Session["Station"].ToString() + "_" + myYear.ToString() + "_" + myMonth.ToString() + "_" + myDay.ToString() + "_" + myHour.ToString();
                //HttpContext.Current.Session["SaveFile"] = @"D:\tmp\events_" + HttpContext.Current.Session["Station"].ToString() + "_" + myYear.ToString() + "_" + myMonth.ToString() + "_" + myDay.ToString() + "_" + myHour.ToString();
                myHour++;
                if (myHour == 24) { myHour = 0; myDay++; }
                if (myDay == 32) { myDay = 1; myMonth++; }
                if (myMonth == 13) { myMonth = 1; myYear++; }
                HttpContext.Current.Session["NextFile"] = Hantek + "_" + myYear.ToString() + "_" + myMonth.ToString() + "_" + myDay.ToString() + "_" + myHour.ToString() + ".showerdata";

                var file = new FileStream(strRootRelativePathName1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader rs = new StreamReader(file);
                rs.BaseStream.Seek(0, SeekOrigin.Begin);
                HttpContext.Current.Session["Shower_Current_File"] = filename;
                HttpContext.Current.Session["Shower_rs"] = rs;
                return true;
            }

            string strRootRelativePathName = @"D:\Save_Pulses_Showers_Phase2\" + filename;
            bool filechanged = false;
            if (File.Exists(strRootRelativePathName))
            {
                var file = new FileStream(strRootRelativePathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader rs = new StreamReader(file);
                string old_filename = HttpContext.Current.Session["Shower_Current_File"].ToString();
                if (old_filename != filename) filechanged = true;
                HttpContext.Current.Session["Shower_Current_File"] = filename;
                HttpContext.Current.Session["Shower_rs"] = rs;
                if (!filechanged || old_filename=="nofile")
                {
                    //rs = File.OpenText(strRootRelativePathName);
                    //rs.BaseStream.Seek(-4221, SeekOrigin.End);
                    //return;
                    rs.ReadToEnd();
                    long pos = rs.BaseStream.Position;
                    decimal events = (pos / 5061) - System.Math.Floor((decimal)(pos / 5061));
                    if (events > 0)
                    {
                        long p = (long)System.Math.Floor((decimal)(pos / 5061));
                        rs.BaseStream.Seek(p, SeekOrigin.Begin);
                    }
                }
                else
                {
                    ;
                }
            }
            return true;
        }

        bool Process_Pulses(int CH1, int CH2, int CH3, float[,] vv, float p1, float p2, float p3)
        {
            int Trigger = 0;
            int kmax = -1;
            double vmax = -1.0;
            int[] trg = new int[3] { -1, -1, -1 };
            float[] thr = new float[3];
            thr[0] = (float)Convert.ChangeType(HttpContext.Current.Session["Shower_thr1"].ToString(), typeof(float));
            thr[1] = (float)Convert.ChangeType(HttpContext.Current.Session["Shower_thr2"].ToString(), typeof(float));
            thr[2] = (float)Convert.ChangeType(HttpContext.Current.Session["Shower_thr3"].ToString(), typeof(float));
            float[] tim = new float[6];
            tim[0] = (float)Convert.ChangeType(HttpContext.Current.Session["Shower_tim1"].ToString(), typeof(float));
            tim[1] = (float)Convert.ChangeType(HttpContext.Current.Session["Shower_tim2"].ToString(), typeof(float));
            tim[2] = (float)Convert.ChangeType(HttpContext.Current.Session["Shower_tim3"].ToString(), typeof(float));
            //if (HttpContext.Current.Session["CFD_showers"].ToString() == "1")
            {
                tim[3] = (float)(p1 * 0.2); //if (tim[0] < thr[0]) tim[0] = thr[0];  
                tim[4] = (float)(p2 * 0.2); //if (tim[1] < thr[1]) tim[1] = thr[1];
                tim[5] = (float)(p3 * 0.2); //if (tim[2] < thr[2]) tim[2] = thr[2];
            }
            float[] time = new float[6] { -1, -1, -1, -1, -1, -1 };
            double[] charge = new double[3] { 0, 0, 0 };
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

            if (vmax > thr[0])
            {
                for (int k = 1; k < 200; k++)
                {
                    charge[0] = charge[0] + vv[CH1, k] * 0.08; //pC 0.08=4/50
                    if (time[0] < 0 && vv[CH1, k] > tim[0])
                    {
                        float v2 = vv[CH1, k];// vv; //vmax;
                        float v1 = vv[CH1, k - 1];
                        float DV = v2 - v1;
                        float DT = 4;// (kmax - (k - 1)) * 4.0;
                        float dv = tim[0] - v1;
                        float dt = DT * dv / DV;
                        time[0] = (k - 1) * 4 + dt;
                        //tim[mych] = time[mych]; tim2[mych] = float(k) * 4.0;
                    }
                    if (time[3] < 0 && vv[CH1, k] > tim[3])
                    {
                        float v2 = vv[CH1, k];// vv; //vmax;
                        float v1 = vv[CH1, k - 1];
                        float DV = v2 - v1;
                        float DT = 4;// (kmax - (k - 1)) * 4.0;
                        float dv = tim[3] - v1;
                        float dt = DT * dv / DV;
                        time[3] = (k - 1) * 4 + dt;
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
                    charge[1] = charge[1] + vv[CH2, k] * 0.08; //pC 0.08=4/50
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
                    if (time[4] < 0 && vv[CH2, k] > tim[4])
                    {
                        float v2 = vv[CH2, k];// vv; //vmax;
                        float v1 = vv[CH2, k - 1];
                        float DV = v2 - v1;
                        float DT = 4;// (kmax - (k - 1)) * 4.0;
                        float dv = tim[4] - v1;
                        float dt = DT * dv / DV;
                        time[4] = (k - 1) * 4 + dt;
                        //tim[mych] = time[mych]; tim2[mych] = float(k) * 4.0;
                    }
                }
            }

            ////////////////////3rd detector (Reference)

            for (int k = 0; k < 200; k++)
            {
                if (vv[CH3, k] > thr[2]) //always the last counter is the reference
                {
                    Trigger = 1; trg[2] = 1; break;
                }
            }
            if (trg[2] > 0)
            {
                for (int k = 1; k < 200; k++)
                {
                    charge[2] = charge[2] + vv[CH3, k] * 0.08; //pC 0.08=4/50
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
                    if (time[5] < 0 && vv[CH3, k] > tim[5])
                    {
                        float v2 = vv[CH3, k];// vv; //vmax;
                        float v1 = vv[CH3, k - 1];
                        float DV = v2 - v1;
                        float DT = 4;// (kmax - (k - 1)) * 4.0;
                        float dv = tim[5] - v1;
                        float dt = DT * dv / DV;
                        time[5] = (k - 1) * 4 + dt;
                        //tim[mych] = time[mych]; tim2[mych] = float(k) * 4.0;
                    }
                }
            }
            bool evt_rec_raised = false;
            if (trg[0] > 0 && trg[1] > 0 && trg[2] > 0)
            {
                int evt_all = (int)Convert.ChangeType(HttpContext.Current.Session["Run_TotalEvents"].ToString(), typeof(int));
                evt_all++; HttpContext.Current.Session["Run_TotalEvents"] = evt_all;
                int evt_rec = (int)Convert.ChangeType(HttpContext.Current.Session["Run_ReconstructedEvents"].ToString(), typeof(int));
                double theta_thr = -1.0; double phi_thr = -1.0;
                if (ThetaPhi(CH1, CH2, CH3, time[0], time[1], time[2], (double)p1, (double)p2, (double)p3, charge[0], charge[1], charge[2]))
                {
                    evt_rec_raised = true;
                    double[] eventInfo = (double[])(Session["event"]);//times, peaks, charge,theta,ph
                    theta_thr = eventInfo[9];
                    phi_thr = eventInfo[10];
                    evt_rec++;
                    HttpContext.Current.Session["Run_ReconstructedEvents"] = evt_rec;
                }
                //else
                {
                    double offsetthr1 = Convert.ToDouble(HttpContext.Current.Session["Shower_off1"].ToString());
                    double offsetthr2 = Convert.ToDouble(HttpContext.Current.Session["Shower_off2"].ToString());
                    double offsetthr3 = Convert.ToDouble(HttpContext.Current.Session["Shower_off3"].ToString());

                    double[,] off = (double[,])(Session["OffValuesCDF"]);
                    int station = (int)(Session["Station"]);
                    double offset1 = off[station - 1, 0];
                    double offset2 = off[station - 1, 1];
                    double offset3 = off[station - 1, 2];
                    HttpContext.Current.Session["Shower_off1"] = offset1.ToString();
                    HttpContext.Current.Session["Shower_off2"] = offset2.ToString();
                    HttpContext.Current.Session["Shower_off3"] = offset3.ToString();

                    if (ThetaPhi(CH1, CH2, CH3, time[3], time[4], time[5], (double)p1, (double)p2, (double)p3, charge[0], charge[1], charge[2]))
                    {
                        evt_rec_raised = true;
                            if (theta_thr<0 && phi_thr<0)
                         {
                            evt_rec++;
                           HttpContext.Current.Session["Run_ReconstructedEvents"] = evt_rec;
                        }
                    }
                    HttpContext.Current.Session["Shower_off1"] = offsetthr1.ToString();
                    HttpContext.Current.Session["Shower_off2"] = offsetthr2.ToString();
                    HttpContext.Current.Session["Shower_off3"] = offsetthr3.ToString();
                }
                if (evt_all > 0) HttpContext.Current.Session["Run_ReconstructionFailureRate"] = (100-evt_rec * 100 / evt_all).ToString() + "%";
                //}
                //write out
                string strPathCalibrationFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Shower_folder"].ToString());
                string path = strPathCalibrationFolder + @"\events.txt";
                if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                {
                    path = HttpContext.Current.Session["SaveFile"].ToString();
                    if (HttpContext.Current.Session["scan"].ToString() == "1") path = strPathCalibrationFolder + @"\events.txt";
                }
                bool written = false;
                try
                {
                    int[] eventTime = (int[])(Session["EventTime"]);
                    double[] eventInfo = (double[])(Session["event"]);//times, peaks, charge,theta,ph
                    bool badEvent = false;
                    if (eventInfo[3] > 60 && eventInfo[4] > 60 && eventInfo[5] > 60 && (eventInfo[6] + eventInfo[7] + eventInfo[8]) < 1000) badEvent = true;
                    if (((theta_thr > 0 && phi_thr > 0) && (eventInfo[9] > 0 && eventInfo[10] > 0)) && !badEvent)
                    {
                        string time_event = String.Format("{0} {1} {2} {3} {4} {5} {6}", eventTime[0], eventTime[1], eventTime[2], eventTime[3], eventTime[4], eventTime[5], eventTime[6]);
                        string info_event = String.Format("{0,2:0.00} {1,2:0.00} {2,2:0.00} {3,2:0.00} {4,2:0.00} {5,2:0.00} {6,2:0.00} {7,2:0.00} {8,2:0.00} {9,2:0.00} {10,2:0.00} {11,2:0.00} {12,2:0.00}", eventInfo[0], eventInfo[1], eventInfo[2], eventInfo[3], eventInfo[4], eventInfo[5], eventInfo[6], eventInfo[7], eventInfo[8], eventInfo[9], eventInfo[10], theta_thr, phi_thr);
                        File.AppendAllText(path, time_event + "\n");
                        File.AppendAllText(path, info_event + "\n");
                        for (int k = 0; k < 200; k++)
                        {
                            string voltages = String.Format("{0,2:0.00} {1,2:0.00} {2,2:0.00}", vv[CH1, k], vv[CH2, k], vv[CH3, k]);
                            File.AppendAllText(path, voltages + "\n");
                        }
                        string dateflag = string.Format("{0,2:d2}/{1,2:d2}/{2,2:d2} {3,2:d2}:{4,2:d2}:{5,2:d2}", eventTime[2], eventTime[1], eventTime[0], eventTime[3], eventTime[4], eventTime[5]);
                        string PulseArrivalTime = string.Format(" (t1, t2, t3) = ({0,2:0.00}, {1,2:0.00}, {2,2:0.00}) ns", eventInfo[0], eventInfo[1], eventInfo[2]);
                        string PulsePeaks = string.Format(" (p1, p2, p3) = ({0,2:0.00}, {1,2:0.00}, {2,2:0.00}) mV", eventInfo[3], eventInfo[4], eventInfo[5]);
                        string PulseCharges = string.Format(" (q1, q2, q3) = ({0,2:0.00}, {1,2:0.00}, {2,2:0.00}) pC", eventInfo[6], eventInfo[7], eventInfo[8]);
                        string zenith = string.Format("{0,2:0.00} deg", eventInfo[9]);
                        string azimuth = string.Format("{0,2:0.00} deg", eventInfo[10]);
                        HttpContext.Current.Session["Event_DateTime"] = dateflag;
                        HttpContext.Current.Session["Event_PulseArrivalTime"] = PulseArrivalTime;
                        HttpContext.Current.Session["Event_PulsePeaks"] = PulsePeaks;
                        HttpContext.Current.Session["Event_PulseCharges"] = PulseCharges;
                        HttpContext.Current.Session["Event_Zenith"] = zenith;
                        HttpContext.Current.Session["Event_Azimuth"] = azimuth;
                        written = true;
                    }
                    else
                    {
                        if (evt_rec_raised==true) evt_rec--;
                        HttpContext.Current.Session["Run_ReconstructedEvents"] = evt_rec;
                    }
                }
                catch
                {
                    ;
                }
                return written;
            }
            return false;
        }

        protected int Read_events()
        {
            StreamReader rs = (StreamReader)HttpContext.Current.Session["Shower_rs"];
            if (rs == null) return 0;//this is caught in the previous calling routine
            float[] peak = new float[4] { -1, -1, -1, 1 };
            float[] vflag = new float[4] { -1, -1, -1, 1 };
            long record = rs.BaseStream.Length - rs.BaseStream.Position;
            if (record > 5060)
                ;
            float events = record / 5061;
            if (record < 5061) //this will update the file if the hour is changed? Yes. Also if we have not a completed event, it will give time to be completed 
            {
                Set_File_Position();//set to the beggining of the last event if it is partially written or to the end of fully written event
                return 0;
            }
            // there are at least one full event
            long this_event_start = -1;
            int evts_read = 0;
            int maxevt = 2000;
            if (HttpContext.Current.Session["ReadAll"].ToString() == "1") maxevt = 99999999;
            while (evts_read < maxevt) //
            {
                try
                {
                    string line = rs.ReadLine();
                    if (line == null) 
                        return evts_read;//this should happen when al events are read (see line 357)
                    string[] flags = new string[4];
                    flags[0] = line.Substring(0, 5);
                    flags[1] = line.Substring(6, 5);
                    flags[2] = line.Substring(12, 5);
                    flags[3] = line.Substring(18);
                    for (int i = 0; i < 4; i++) vflag[i] = (float)Convert.ChangeType(flags[i], typeof(float));
                    this_event_start = rs.BaseStream.Position;
                    if (vflag[0] != -99 || vflag[1] != -99 || vflag[2] != -99 || vflag[3] != -99)
                    {
                        if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                        {
                            while (vflag[0] != -99 || vflag[1] != -99 || vflag[2] != -99 || vflag[3] != -99) //just keep reding until you find the flag
                            {
                                line = rs.ReadLine();
                                if (line == null) return evts_read;
                                flags[0] = line.Substring(0, 5);
                                flags[1] = line.Substring(6, 5);
                                flags[2] = line.Substring(12, 5);
                                flags[3] = line.Substring(18);
                                //string[] elements = line.Split(' ');
                                for (int i = 0; i < 4; i++) vflag[i] = (float)Convert.ChangeType(flags[i], typeof(float));
                            }
                        }
                        else { Set_File_Position(); return evts_read; }//this is called again and again until a new good event is written
                    }
                }
                catch (Exception)
                {

                    if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                    {
                        ;
                    }
                    else throw;
                }

                try
                {
                    string line = rs.ReadLine();
                    if (line == null) 
                    { Set_File_Position(); return evts_read; }
                    string[] flags = new string[4];
                    flags[0] = line.Substring(0, 2);
                    flags[1] = line.Substring(3, 2);
                    flags[2] = line.Substring(5, 4);
                    for (int i = 0; i < 3; i++) vflag[i] = (float)Convert.ChangeType(flags[i], typeof(float));
                    int[] eventInfo = (int[])(Session["EventTime"]);
                    eventInfo[4]=(int)Convert.ChangeType(flags[0], typeof(int));
                    eventInfo[5] = (int)Convert.ChangeType(flags[1], typeof(int));
                    eventInfo[6] = (int)Convert.ChangeType(flags[2], typeof(int));
                    HttpContext.Current.Session["EventTime"] = eventInfo;
                }
                catch (Exception)
                {

                    if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                    {
                        ;
                    }
                    else throw;
                }

                ic++;
                try
                {
                    string line = rs.ReadLine();
                    if (line == null) 
                    { Set_File_Position(); return evts_read; }
                    string[] elements = new string[4];
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
                        ;
                    }
                    else throw;
                }

                string ss = HttpContext.Current.Session["Station"].ToString();
                int Station = (int)Convert.ChangeType(ss, typeof(int));
                int ch1 = -1, ch2 = -1, ch3 = -1;

                if (Station == 1) { ch1 = 1; ch2 = 2; ch3 = 3; }//234
                if (Station == 2) { ch1 = 0; ch2 = 2; ch3 = 3; }//134
                if (Station == 3) { ch1 = 0; ch2 = 1; ch3 = 3; }//124
                if (Station == 4) { ch1 = 0; ch2 = 1; ch3 = 2; }//123

                if (Station == 5) { ch1 = 1; ch2 = 2; ch3 = 3; }//234
                if (Station == 6) { ch1 = 0; ch2 = 2; ch3 = 3; }//134
                if (Station == 7) { ch1 = 0; ch2 = 1; ch3 = 3; }//124
                if (Station == 8) { ch1 = 0; ch2 = 1; ch3 = 2; }//123

                if (Station == 9) { ch1 = 1; ch2 = 2; ch3 = 3; }//234
                if (Station == 10) { ch1 = 0; ch2 = 2; ch3 = 3; }//134
                if (Station == 11) { ch1 = 0; ch2 = 1; ch3 = 3; }//124
                if (Station == 12) { ch1 = 0; ch2 = 1; ch3 = 2; }//123

                if (Station == 13) { ch1 = 1; ch2 = 2; ch3 = 3; }//234
                if (Station == 14) { ch1 = 0; ch2 = 2; ch3 = 3; }//134
                if (Station == 15) { ch1 = 0; ch2 = 1; ch3 = 3; }//124
                if (Station >= 16) { ch1 = 0; ch2 = 1; ch3 = 2; }//123

                float[,] vv = new float[4, 200];
                for (int k = 0; k < 200; k++)
                {
                    string line = rs.ReadLine(); ic++;
                    if (line == null)
                    {
                        Set_File_Position(); return evts_read; 
                        //line = rs.ReadLine();
                        //string ss9 = line;
                    }
                    try
                    {

                        string[] elements = new string[4];
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
                            ;
                        }
                        else throw;
                    }
                }
                for (int ch=0;ch<4;ch++)
                {
                    float amax = -100;
                    for(int k=0;k<200;k++)
                    {
                        if (Math.Abs(vv[ch, k]) > amax) amax = Math.Abs(vv[ch, k]);
                    }
                    peak[ch] = amax;
                }
                bool reconstructed=false;
                if (true)
                {
                    float[] thres = new float[4];
                    thres[ch1] = (float)Convert.ChangeType(HttpContext.Current.Session["Shower_thr1"].ToString(), typeof(float));
                    thres[ch2] = (float)Convert.ChangeType(HttpContext.Current.Session["Shower_thr2"].ToString(), typeof(float));
                    thres[ch3] = (float)Convert.ChangeType(HttpContext.Current.Session["Shower_thr3"].ToString(), typeof(float));
                    try
                    {
                        if (peak[ch1] > thres[ch1] || peak[ch2] > thres[ch2] || peak[ch3] > thres[ch3])
                        {
                            try
                            {
                                reconstructed=Process_Pulses(ch1,ch2,ch3,vv, peak[ch1], peak[ch2], peak[ch3]);
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
                if (reconstructed)
                {
                    evts_read++; // in case of scan return without changing the posiition
                    if (HttpContext.Current.Session["scan"].ToString() == "1") return evts_read;
                }
            }
             Set_File_Position();
            return evts_read;  //we always set the position after read
        }
        protected void Read_Events_and_Show_Protected()
        {
            string IsAcquisitionStarted = HttpContext.Current.Session["Shower_Acquisition_Started"].ToString();
            StreamReader rs = (StreamReader)HttpContext.Current.Session["Shower_rs"];
            if (rs == null) 
            { Set_File_Position(); rs = (StreamReader)HttpContext.Current.Session["Shower_rs"]; }//this cannot happen
            if (rs == null) return;
            long pos = rs.BaseStream.Position;
            try
            {
                Read_Events_and_Show(); 
            }
            catch (Exception)
            {
                long record = rs.BaseStream.Length - (pos + 5061);
                if (record > 5061)
                    rs.BaseStream.Seek(pos + 5061, SeekOrigin.Begin);
                else
                    rs.BaseStream.Seek(pos, SeekOrigin.Begin);
                throw;
                if (IsAcquisitionStarted == "1") //Read_Events_and_Show changes the session variable in case of failure. Catch the failure and revert
                    HttpContext.Current.Session["Shower_Acquisition_Started"] = 1;
            }
        }

        protected void Read_Events_and_Show()
        {
            int read_events = 0;
            if (HttpContext.Current.Session["Shower_Acquisition_Started"] == null) return ;
            int shower_acquisition_started = (int)HttpContext.Current.Session["Shower_Acquisition_Started"];
            if (shower_acquisition_started == 1)
            {
                HttpContext.Current.Session["Shower_Acquisition_Started"] = 0;
                try
                {
                    read_events=Read_events();//in case of scan only 1 event is read (reconstrcted)
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    if (read_events>0 && HttpContext.Current.Session["ReadAll"].ToString() == "0")
                    { 
                    string strPathShowerFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Shower_folder"].ToString());
                    string result = BatchCommand("script_shower_start.cmd", strPathShowerFolder);
                    HttpContext.Current.Session["Shower_State"] = result;
                    string SessionId = HttpContext.Current.Session.SessionID;
                    string image = "pulses_" + SessionId + ".jpg";
                    string strPathName = @"~\images\" + image; string strRootRelativePathName = strPathShowerFolder.ToString() + @"\pulses1.jpg";
                    File.Copy(strRootRelativePathName, Server.MapPath(strPathName), true);
                    string ss = img0.ImageUrl;
                    img0.ImageUrl = "images/" + image;
                    ss = img0.ImageUrl;

                    image = "plots_" + SessionId + ".jpg";
                    strPathName = @"~\images\" + image; strRootRelativePathName = strPathShowerFolder.ToString() + @"\plots.jpg";
                    File.Copy(strRootRelativePathName, Server.MapPath(strPathName), true);
                    ss = img1.ImageUrl;
                    img1.ImageUrl = "images/" + image;
                    ss = img1.ImageUrl;
                    }
                    if (HttpContext.Current.Session["ReadAll"].ToString() == "1")
                    {

                        //if scan set the above and return. if scan and read_events==0 set_file_position and return
                        if (HttpContext.Current.Session["scan"].ToString()=="1")
                        {
                            if(read_events > 0)
                            {
                                string strPathShowerFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Shower_folder"].ToString());
                                string result = BatchCommand("script_shower_start.cmd", strPathShowerFolder);
                                HttpContext.Current.Session["Shower_State"] = result;
                                string SessionId = HttpContext.Current.Session.SessionID;
                                string image = "pulses_" + SessionId + ".jpg";
                                string strPathName = @"~\images\" + image; string strRootRelativePathName = strPathShowerFolder.ToString() + @"\pulses1.jpg";
                                File.Copy(strRootRelativePathName, Server.MapPath(strPathName), true);
                                string ss = img0.ImageUrl;
                                img0.ImageUrl = "images/" + image;
                                ss = img0.ImageUrl;

                                image = "plots_" + SessionId + ".jpg";
                                strPathName = @"~\images\" + image; strRootRelativePathName = strPathShowerFolder.ToString() + @"\plots.jpg";
                                File.Copy(strRootRelativePathName, Server.MapPath(strPathName), true);
                                ss = img1.ImageUrl;
                                img1.ImageUrl = "images/" + image;
                                ss = img1.ImageUrl;
                            }
                            else
                                Set_File_Position();//goto the next file
                        }
                        else
                        {
                            if (HttpContext.Current.Session["NextFile"].ToString() != HttpContext.Current.Session["Hantek"].ToString() + "_2023_1_1_0.showerdata")
                            {
                                string ss = HttpContext.Current.Session["NextFile"].ToString();
                                bool contin=Set_File_Position();//goto the next file
                                if (contin)
                                {
                                    HttpContext.Current.Session["Shower_Acquisition_Started"] = 1;
                                    Read_Events_and_Show();
                                }
                                else
                                    ;
                            }
                            else
                            {
                                ;
                            }
                        }

                    }
                    HttpContext.Current.Session["Shower_Acquisition_Started"] = 1;
                }
            }
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            //HttpContext.Current.Session["Run_StartTime"] OK 
            //HttpContext.Current.Session["Run_RunningTime"]
            //HttpContext.Current.Session["Run_DetectionRate"]
            //HttpContext.Current.Session["Run_ReconstructionRate"]
            //HttpContext.Current.Session["Run_TotalEvents"] OK
            //HttpContext.Current.Session["Run_ReconstructedEvents"] OK
            //HttpContext.Current.Session["Run_ReconstructionFailureRate"] OK
            Read_Events_and_Show_Protected();
            DateTime dateTime1 = DateTime.Now;
            DateTime dateTime2 = (DateTime)HttpContext.Current.Session["Run_DateTime"];
            int diffInDays = (int)Math.Floor((dateTime1 - dateTime2).TotalDays);
            int diffInHours = (int)Math.Floor((dateTime1 - dateTime2).TotalHours)-24*diffInDays;
            int diffInMinutes = (int)Math.Floor((dateTime1 - dateTime2).TotalMinutes)- 24 * diffInDays*60-60*diffInHours;
            int diffInSeconds = (int)Math.Floor((dateTime1 - dateTime2).TotalSeconds)-24*diffInDays*3600-3600*diffInHours-60*diffInMinutes;
            string ttt = String.Format("{0} Days, {1} Hours, {2} Minutes, {3} Seconds",diffInDays, diffInHours, diffInMinutes, diffInSeconds);
            HttpContext.Current.Session["Run_RunningTime"] = ttt;
            double tt = Convert.ToDouble(HttpContext.Current.Session["Run_TotalEvents"])/(dateTime1 - dateTime2).TotalHours;
            string tts = String.Format("{0,2:0.00}", tt);
            HttpContext.Current.Session["Run_DetectionRate"] = tts+" per hour";
            tt = Convert.ToDouble(HttpContext.Current.Session["Run_ReconstructedEvents"]) / (dateTime1 - dateTime2).TotalHours;
            tts = String.Format("{0,2:0.00}", tt);
            HttpContext.Current.Session["Run_ReconstructionRate"] = tts+" per hour";
            ShowRunInfo();
            ShowEventInfo();
        }

        protected bool check_run_values()
        {
            if (!GraterValue(text_thr1.Text, 5.0)) return false;
            if (!GraterValue(Tim1.Text, 5.0)) { return false; }
            if (!LessValue(Tim1.Text, Convert.ToDouble(text_thr1.Text))) return false;
            if (!GraterValue(Peak1.Text, 0.0)) return false;

            if (!GraterValue(text_thr2.Text, 5.0)) return false;
            if (!GraterValue(Tim2.Text, 5.0)) { return false; }
            if (!LessValue(Tim2.Text, Convert.ToDouble(text_thr2.Text))) return false;
            if (!GraterValue(Peak2.Text, 0.0)) return false;

            if (!GraterValue(text_thr3.Text, 5.0)) return false;
            if (!GraterValue(Tim3.Text, 5.0)) { return false; }
            if (!LessValue(Tim3.Text, Convert.ToDouble(text_thr3.Text))) return false;
            if (!GraterValue(Peak3.Text, 0.0)) return false;

            return true;
        }

        protected void ShowRunInfo()
        {
            if (HttpContext.Current.Session["Run_StartTime"] != null) LbRunStartTime.Text = HttpContext.Current.Session["Run_StartTime"].ToString();
            if (HttpContext.Current.Session["Run_RunningTime"] != null) LbRunningTime.Text = HttpContext.Current.Session["Run_RunningTime"].ToString();
            if (HttpContext.Current.Session["Run_DetectionRate"] != null) LbDetectionRate.Text = HttpContext.Current.Session["Run_DetectionRate"].ToString();
            if (HttpContext.Current.Session["Run_ReconstructionRate"] != null) LbReconstructionRate.Text = HttpContext.Current.Session["Run_ReconstructionRate"].ToString();
            if (HttpContext.Current.Session["Run_TotalEvents"] != null) LbTotalEvents.Text = HttpContext.Current.Session["Run_TotalEvents"].ToString();
            if (HttpContext.Current.Session["Run_ReconstructedEvents"] != null) LbReconstructedEvents.Text = HttpContext.Current.Session["Run_ReconstructedEvents"].ToString();
            if (HttpContext.Current.Session["Run_ReconstructionFailureRate"] != null) LbReconstructionFailureRate.Text = HttpContext.Current.Session["Run_ReconstructionFailureRate"].ToString();
        }

        protected void ShowEventInfo()
        {

            if (HttpContext.Current.Session["Event_DateTime"] != null) lbEventDateTime.Text = HttpContext.Current.Session["Event_DateTime"].ToString();
            if (HttpContext.Current.Session["Event_PulseArrivalTime"] != null) lbArrivalTimes.Text = HttpContext.Current.Session["Event_PulseArrivalTime"].ToString();
            if (HttpContext.Current.Session["Event_PulsePeaks"] != null) lbPulsePeaks.Text= HttpContext.Current.Session["Event_PulsePeaks"].ToString();
            if (HttpContext.Current.Session["Event_PulseCharges"] != null) lbPulseCharges.Text= HttpContext.Current.Session["Event_PulseCharges"].ToString();
            if (HttpContext.Current.Session["Event_Zenith"] != null) lbZenith.Text= HttpContext.Current.Session["Event_Zenith"].ToString();
            if (HttpContext.Current.Session["Event_Azimuth"] != null) lbAzimuth.Text= HttpContext.Current.Session["Event_Azimuth"].ToString();
            if (HttpContext.Current.Session["Detector_Pos1"] != null) lbPos1.Text = HttpContext.Current.Session["Detector_Pos1"].ToString();
            if (HttpContext.Current.Session["Detector_Pos2"] != null) lbPos2.Text = HttpContext.Current.Session["Detector_Pos2"].ToString();
            if (HttpContext.Current.Session["Detector_Pos3"] != null) lbPos3.Text = HttpContext.Current.Session["Detector_Pos3"].ToString();

        }
        protected void text_thr1_changed(object sender, EventArgs e)
        {
            if (!GraterValue(text_thr1.Text, 5.0)) return;
            HttpContext.Current.Session["Shower_thr1"] = text_thr1.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_thr2_changed(object sender, EventArgs e)
        {
            if (!GraterValue(text_thr2.Text, 5.0)) return;
            HttpContext.Current.Session["Shower_thr2"] = text_thr2.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_thr3_changed(object sender, EventArgs e)
        {
            if (!GraterValue(text_thr3.Text, 5.0)) return;
            HttpContext.Current.Session["Shower_thr3"] = text_thr3.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_tim1_changed(object sender, EventArgs e)
        {
            if (!GraterValue(Tim1.Text, 5.0)) { return; }
            if (!LessValue(Tim1.Text, Convert.ToDouble(text_thr1.Text))) return;
            HttpContext.Current.Session["Shower_tim1"] = Tim1.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_tim2_changed(object sender, EventArgs e)
        {
            if (!GraterValue(Tim2.Text, 5.0)) return;
            if (!LessValue(Tim2.Text, Convert.ToDouble(text_thr2.Text))) return;
            HttpContext.Current.Session["Shower_tim2"] = Tim2.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_tim3_changed(object sender, EventArgs e)
        {
            if (!GraterValue(Tim3.Text, 5.0)) return;
            if (!LessValue(Tim3.Text, Convert.ToDouble(text_thr3.Text))) return;
            HttpContext.Current.Session["Shower_tim3"] = Tim3.Text;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_off1_changed(object sender, EventArgs e)
        {
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            HttpContext.Current.Session["Shower_off1"] = Off1.Text;
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_off2_changed(object sender, EventArgs e)
        {
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            HttpContext.Current.Session["Shower_off2"] = Off2.Text;
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_off3_changed(object sender, EventArgs e)
        {
            Response.Write("<script>alert('" + "Reference Value changed" + "')</script>");
            HttpContext.Current.Session["Shower_off3"] = Off3.Text;
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_peak1_changed(object sender, EventArgs e)
        {
            if (!GraterValue(Peak1.Text, 0.0)) return;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_peak2_changed(object sender, EventArgs e)
        {
            if (!GraterValue(Peak2.Text, 0.0)) return;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            //Stop_Acquisition_Response(sender, e);
        }
        protected void text_peak3_changed(object sender, EventArgs e)
        {
            if (!GraterValue(Peak3.Text, 0.0)) return;
            Response.Write("<script>alert('" + "Value changed" + "')</script>");
            //Stop_Acquisition_Response(sender, e);
        }

        protected void text_date1_changed(object sender, EventArgs e)
        {
            Show_Reference(sender, e);
        }

        protected void text_date2_changed(object sender, EventArgs e)
        {
            Show_Reference(sender, e);
        }

        protected void Show_Reference(object sender, EventArgs e)
        {
            if(Date1.Text=="" || Date2.Text=="")
            {
                Response.Write("<script>alert('" + "Select dates first!" + "')</script>");
                return;
            }
            string year = Date1.Text.Substring(0, 4);
            string month = Date1.Text.Substring(5, 2);
            string day = Date1.Text.Substring(8, 2);
            string strPathShowerFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Shower_folder"].ToString());

            if (Directory.Exists(strPathShowerFolder) == false)
            {
                Response.Write("<script>alert('" + "Directory does not exist. This should not happen! ErrorCode:001" + "')</script>");
            }
            StreamWriter rs = File.CreateText(strPathShowerFolder + @"\reference_shower_start.txt");
            rs.WriteLine(HttpContext.Current.Session["Station"].ToString());
            rs.WriteLine(year);
            rs.WriteLine(month);
            rs.WriteLine(day);
            rs.Close();

            year = Date2.Text.Substring(0, 4);
            month = Date2.Text.Substring(5, 2);
            day = Date2.Text.Substring(8, 2);
            strPathShowerFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Shower_folder"].ToString());

            if (Directory.Exists(strPathShowerFolder) == false)
            {
                Response.Write("<script>alert('" + "Directory does not exist. This should not happen! ErrorCode:001" + "')</script>");
            }
            rs = File.CreateText(strPathShowerFolder + @"\reference_shower_end.txt");
            rs.WriteLine(HttpContext.Current.Session["Station"].ToString());
            rs.WriteLine(year);
            rs.WriteLine(month);
            rs.WriteLine(day);
            rs.Close();
        }

        protected void Hide_Reference(object sender, EventArgs e)
        {
            string strPathShowerFolder = Server.MapPath(@"~\App_Data\" + HttpContext.Current.Session["Shower_folder"].ToString());

            if (Directory.Exists(strPathShowerFolder) == false)
            {
                Response.Write("<script>alert('" + "Directory does not exist. This should not happen! ErrorCode:001" + "')</script>");
            }
           
            File.Delete(strPathShowerFolder + @"\reference_shower_start.txt");
            File.Delete(strPathShowerFolder + @"\reference_shower_end.txt");

        }

        protected bool GraterValue(string text, double threshold)
        {
            double t = Convert.ToDouble(text);
            string ll = Convert.ToString(threshold);
            if (t < threshold)
            {
                Response.Write("<script>alert('" + "Select value grater than " + ll + " ')</script>");
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
                HttpContext.Current.Session["CFD_showers"] = 1;
            }
            else
                HttpContext.Current.Session["CFD_showers"] = 0;
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

        protected void Tab_Parameters_Click(object sender, EventArgs e)
        {
            HttpContext.Current.Session["Active_Tab"] = 0;
            Show_Parameters_Tab();
        }
        protected void Tab_Event_Click(object sender, EventArgs e)
        {
            HttpContext.Current.Session["Active_Tab"] = 1;
            if (HttpContext.Current.Session["scan"].ToString() == "1")
            {
                int evt_rec = (int)Convert.ChangeType(HttpContext.Current.Session["Run_ReconstructedEvents"].ToString(), typeof(int));
                int evt_rec_next = evt_rec;
                while(evt_rec_next==evt_rec)
                {
                    Read_Events_and_Show_Protected();
                    evt_rec_next = (int)Convert.ChangeType(HttpContext.Current.Session["Run_ReconstructedEvents"].ToString(), typeof(int));
                }

                //Show_Response_Tab();
                ShowRunInfo();
                ShowEventInfo();
            }
                Show_Event_Tab();
        }
        protected void Tab_Histograms_Click(object sender, EventArgs e)
        {
            HttpContext.Current.Session["Active_Tab"] = 2;
            Show_Histograms_Tab();
        }
        protected void Show_Parameters_Tab()
        {
            tab14b7.Attributes["class"] = "u-align-left u-black u-container-style u-tab-active u-tab-pane u-tab-pane-2";
            linktab14b7.Attributes["class"] = "active u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";


            tabe201.Attributes["class"] = "u-align-left u-black u-container-style u-tab-pane u-tab-pane-2";
            linktabe201.Attributes["class"] = "u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";

            taba29a.Attributes["class"] = "u-align-left u-black u-container-style u-tab-pane u-tab-pane-2";
            linktaba29a.Attributes["class"] = "u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";
        }

        protected void Show_Event_Tab()
        {
            tabe201.Attributes["class"] = "u-align-left u-black u-container-style u-tab-active u-tab-pane u-tab-pane-2";
            linktabe201.Attributes["class"] = "active u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";


            tab14b7.Attributes["class"] = "u-align-left u-black u-container-style u-tab-pane u-tab-pane-2";
            linktab14b7.Attributes["class"] = "u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";

            taba29a.Attributes["class"] = "u-align-left u-black u-container-style u-tab-pane u-tab-pane-2";
            linktaba29a.Attributes["class"] = "u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";
        }

        protected void Show_Histograms_Tab()
        {
            taba29a.Attributes["class"] = "u-align-left u-black u-container-style u-tab-active u-tab-pane u-tab-pane-2";
            linktaba29a.Attributes["class"] = "active u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";


            tabe201.Attributes["class"] = "u-align-left u-black u-container-style u-tab-pane u-tab-pane-2";
            linktabe201.Attributes["class"] = "u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";

            tab14b7.Attributes["class"] = "u-align-left u-black u-container-style u-tab-pane u-tab-pane-2";
            linktab14b7.Attributes["class"] = "u-active-white u-button-style u-hover-palette-5-light-1 u-palette-5-dark-1  u-tab-link-2";
        }

        bool ThetaPhi(int ch1, int ch2, int ch3, double RT1, double RT2, double RT3, double p1, double p2, double p3, double charge1, double charge2, double charge3)
        {
            int icc = Session.Count;
            double offset1 = 0;
            double offset2 =0 ;
            double offset3 =0;
                offset1 = Convert.ToDouble(HttpContext.Current.Session["Shower_off1"].ToString());
                offset2 = Convert.ToDouble(HttpContext.Current.Session["Shower_off2"].ToString());
                offset3 = Convert.ToDouble(HttpContext.Current.Session["Shower_off3"].ToString());
            double th =-1.0; double ph=-1.0;
            int maximum, Kmin;
            double clight, aaa, bbb, ccc, ddd, gamma, delta, ALPHA, BETA, GAMMA, phi, thita, DT1, DT2, Phi2, Thita2, xx1, yy1, zz1, xx2, yy2, zz2, minimum;

            double[] rti = new double[3] { 0, 0, 0 };
            double[] xx = new double[3] { 0, 0, 0 };
            double[] yy = new double[3] { 0, 0, 0 };
            double[] zz = new double[3] { 0, 0, 0 };
            double[] xxfixed = new double[3] { 0, 0, 0 };
            double[] yyfixed = new double[3] { 0, 0, 0 };
            double[] zzfixed = new double[3] { 0, 0, 0 };
            double[,] dr = new double[2, 3];
            double[] timepulsefixed = new double[4] { 0, 0, 0, 0 };

            //long double timepulsefixed[4];

            int counter = 0;
            xx1 = 0.0; yy1 = 0.0; zz1 = 0.0; xx2 = 0.0; yy2 = 0.0; zz2 = 0.0;
            //  RT1 = 0.; RT2 = 0.; RT3 = 0.;
            gamma = 0.0; delta = 0.0;

            rti[0] = RT1 + offset1; rti[1] = RT2 + offset2; rti[2] = RT3 + offset3;

            double[,] position = (double[,])(Session["positions"]);

            xx[0] = position[ch1, 0]; yy[0] = position[ch1, 1]; zz[0] = position[ch1, 2];
            xx[1] = position[ch2, 0]; yy[1] = position[ch2, 1]; zz[1] = position[ch2, 2];
            xx[2] = position[ch3, 0]; yy[2] = position[ch3, 1]; zz[2] = position[ch3, 2];

            dr[0,0] = 0.0; dr[0,1] = 0.0; dr[0,2] = 0.0;
            dr[1,0] = 0.0; dr[1,1] = 0.0; dr[1,2] = 0.0;

            minimum = 999999999;
            maximum = -99999999;
            Kmin = 0;

            for (int ip = 0; ip < 3; ip++)
            {
                if (rti[ip] < minimum)
                {
                    minimum = rti[ip];
                    Kmin = ip;
                }
            }

            DT1 = 0; DT2 = 0;
            int K = 0;
            for (int ip = 0; ip < 3; ip++)
            {
                timepulsefixed[ip] = rti[ip] - minimum;
                xxfixed[ip] = xx[ip] - xx[Kmin];
                yyfixed[ip] = yy[ip] - yy[Kmin];
                zzfixed[ip] = zz[ip] - zz[Kmin];
                if (ip != Kmin)
                {
                    K = 3 - Kmin - ip;
                    if (rti[ip] < rti[K])
                    {
                        xx1 = xxfixed[ip];
                        yy1 = yyfixed[ip];
                        zz1 = zzfixed[ip];
                        DT1 = -timepulsefixed[ip];
                        xx2 = xxfixed[K];
                        yy2 = yyfixed[K];
                        zz2 = zzfixed[K];
                        DT2 = -timepulsefixed[K];
                    }
                    else if (rti[K] < rti[ip])
                    {
                        xx2 = xxfixed[ip];
                        yy2 = yyfixed[ip];
                        zz2 = zzfixed[ip];
                        DT2 = -timepulsefixed[ip];
                        xx1 = xxfixed[K];
                        yy1 = yyfixed[K];
                        zz1 = zzfixed[K];
                        DT1 = -timepulsefixed[K];
                    }
                }
            }

            clight = 29.9792458;
            aaa = ((yy1 * DT2 - yy2 * DT1) / (xx2 * yy1 - xx1 * yy2)) * clight;
            bbb = ((xx1 * DT2 - xx2 * DT1) / (yy2 * xx1 - yy1 * xx2)) * clight;
            gamma = ((zz1 * xx2) - (xx1 * zz2)) / ((xx1 * yy2) - (yy1 * xx2));
            delta = ((zz1 * yy2) - (yy1 * zz2)) / ((xx2 * yy1) - (yy2 * xx1));
            //cout<<"gamma "<<gamma<<" "<<delta<<" "<<RT1<<" "<<RT2<<" "<<RT3<<" "<<aaa<<" "<<bbb<<endl;
            if ((gamma != 0.0) && (delta != 0.0))
            {
                // THE CALCULATIONS ARE DESCRIBED BELOW:

                //xx1*a+yy1*b+zz1*c=clight*dt1
                //xx2*a+yy2*b+zz2*c=clight*dt2
                //a=a+c*(zz1*xx2-yy1*zz2)/(xx2*yy1-xx1*yy2)
                //b=b+c*(zz1*yy2-yy1*zz2)/(xx2*yy1-xx1*yy2)
                //gamma=(zz1*yy2-yy1-zz2)/(xx1*yy2-yy1*xx2)
                //delta=(zz1*yy2-yy1*zz2)/(xx2*yy1-yy2*xx1)
                //a^2+b^2=a^2+b^2+(gamma^2+delta^2)*c^2+(2*b*gamma+2*a*delta)*c
                //c^2=1 & +-c^2
                //(1+gamma^2+delta^2)*c^2+(2*b*gamma+2*a*delta)*c+a^2+b^2-1=0 --> c

                BETA = 2.0* bbb * gamma + 2.0* aaa * delta;
                ALPHA = 1.0 + Math.Pow(gamma, 2) + Math.Pow(delta, 2);
                GAMMA = Math.Pow(aaa, 2) + Math.Pow(bbb, 2) - 1.0;
                double sqrtcheck = Math.Pow(BETA, 2) - (4 * ALPHA * GAMMA);
                if ((sqrtcheck < 0.0) && (sqrtcheck > -0.01))
                {
                    sqrtcheck = 0.0;
                }
                double Diakrinousa = Math.Sqrt(sqrtcheck);
                if (sqrtcheck >= 0.0)
                {
                    ccc = ((-BETA) + Diakrinousa) / (2.0* ALPHA);
                    aaa = aaa + delta * ccc;
                    bbb = bbb + gamma * ccc;
                    phi = Math.Atan2(bbb, aaa);
                    Phi2 = phi;
                    phi = phi * 180.0 / 3.14159;
                    if (phi < 0.0)
                    {
                        phi = phi + 360.0;
                    }
                    ddd = Math.Pow(aaa, 2) + Math.Pow(bbb, 2);
                    if (ddd <= 1.0)
                    {
                        thita = Math.Asin(Math.Sqrt(ddd));
                    }
                    else
                    {
                        //cout<<"ddd "<<ddd<<endl;	
                        thita = 0.0;
                    }
                    Thita2 = thita;
                    thita = thita * 180.0 / 3.14159;
                    th = thita;
                    ph = phi;
                }
            }
            else
            {
                //cout<<gamma<<" "<<delta<<endl;
                phi = Math.Atan2(bbb, aaa);
                Phi2 = phi;
                phi = phi * 180.0 / 3.14159;
                if (phi < 0.0)
                {
                    phi = phi + 360.0;
                }
                ddd = Math.Pow(aaa, 2) + Math.Pow(bbb, 2);
                if (ddd < 1.0)
                {
                    thita = Math.Asin(Math.Sqrt(ddd));
                }
                else
                {
                    //cout<<"ddd2 "<<ddd<<endl;	
                    thita = -1.0;
                }
                Thita2 = thita;
                thita = thita * 180.0 / 3.14159;
                ph = phi;
                th = thita; if (th < 0) { th = -1.0; ph = -1; }
            }
           double[] eventInfo = (double[])(Session["event"]);//times, peaks, charge,theta,ph
                                                               //            void ThetaPhi(int ch1, int ch2, int ch3, double RT1, double RT2, double RT3, double p1, double p2, double p3, double charge1, double charge2, double charge3)
            eventInfo[0] = RT1; eventInfo[1] = RT2;eventInfo[2] = RT3;
            eventInfo[3] = p1; eventInfo[4] = p2; eventInfo[5] = p3;
            eventInfo[6] = charge1; eventInfo[7] = charge2; eventInfo[8] = charge3;
            eventInfo[9] = th; eventInfo[10] = ph;
            HttpContext.Current.Session["event"] = eventInfo;
            if (th > 0 && ph > 0) return true; else return false;
        }
    }
}