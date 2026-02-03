using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication2
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["Station"] != null)
            {
                string station = HttpContext.Current.Session["Station"].ToString();
                if (!IsPostBack)
                    DropDownList1.SelectedIndex = (int)Convert.ChangeType(station, typeof(int));// radio1.SelectedValue = station;
            }
        }
        protected void Set_Station(object sender, EventArgs e)
        {
            //string n = String.Format("{0}", Request.Form["radiobutton"]);
            //HttpContext.Current.Session["Station"] = radio1.SelectedValue;
            string ss = HttpContext.Current.Session["Station"].ToString();
            int Station = (int)Convert.ChangeType(ss.Substring(7), typeof(int));
            int Hantek = -1;
            if (Station == 1) { Hantek = 1; }//234
            if (Station == 2) { Hantek = 1; }//134
            if (Station == 3) { Hantek = 1; }//124
            if (Station == 4) { Hantek = 1; }//123

            if (Station == 5) { Hantek = 2; }//234
            if (Station == 6) { Hantek = 2; }//134
            if (Station == 7) { Hantek = 2; }//124
            if (Station == 8) { Hantek = 2; }//123

            if (Station == 9) { Hantek = 3; }//234
            if (Station == 10) { Hantek = 3; }//134
            if (Station == 11) { Hantek = 3; }//124
            if (Station == 12) { Hantek = 3; }//123

            if (Station == 13) { Hantek = 4; }//234
            if (Station == 14) { Hantek = 4; }//134
            if (Station == 15) { Hantek = 4; }//124
            if (Station >= 16) { Hantek = 4; }//123

            HttpContext.Current.Session["Hantek"] = Hantek;

        }

        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpContext.Current.Session["Station"] = DropDownList1.SelectedIndex;
            string ss = HttpContext.Current.Session["Station"].ToString();
            int Station = (int)Convert.ChangeType(ss, typeof(int));
            int Hantek = -1;
            if (Station == 1) { Hantek = 1; }//234
            if (Station == 2) { Hantek = 1; }//134
            if (Station == 3) { Hantek = 1; }//124
            if (Station == 4) { Hantek = 1; }//123

            if (Station == 5) { Hantek = 2; }//234
            if (Station == 6) { Hantek = 2; }//134
            if (Station == 7) { Hantek = 2; }//124
            if (Station == 8) { Hantek = 2; }//123

            if (Station == 9) { Hantek = 3; }//234
            if (Station == 10) { Hantek = 3; }//134
            if (Station == 11) { Hantek = 3; }//124
            if (Station == 12) { Hantek = 3; }//123

            if (Station == 13) { Hantek = 4; }//234
            if (Station == 14) { Hantek = 4; }//134
            if (Station == 15) { Hantek = 4; }//124
            if (Station >= 16) { Hantek = 4; }//123

            HttpContext.Current.Session["Hantek"] = Hantek;
        }
    }
}