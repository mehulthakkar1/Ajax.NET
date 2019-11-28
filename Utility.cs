using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Ajax
{
    public class Utility
    {

        public static string HandlerExtention = ".ashx";
        private static bool configAtClientSide = false;
        public static bool ConfigureAtClientSide
        {
            get { return configAtClientSide; }
            set { configAtClientSide = value; }
        }


        public static void GenerateMethodScripts(object oType)
        {
            GenerateMethodScripts(oType, false);
        }

        public static void GenerateMethodScripts(object oType, bool configureAtClientSide)
        {
            Type objType = oType.GetType();
            string path = objType.FullName + "," + objType.Assembly.FullName.Substring(0, objType.Assembly.FullName.IndexOf(",")) + "__ajax";
            Page objPage = ((Control)oType).Page;

            if (System.Web.HttpContext.Current.Request.ApplicationPath != "/")
            {
                if (!objPage.ClientScript.IsClientScriptBlockRegistered(objPage.GetType(), "common"))
                    objPage.ClientScript.RegisterClientScriptBlock(objPage.GetType(), "common", "<script type='text/javascript' src='" + System.Web.HttpContext.Current.Request.ApplicationPath + "/common__ajax.ashx'></script>");
                objPage.ClientScript.RegisterClientScriptBlock(objType, path, "<script type='text/javascript' src='" + System.Web.HttpContext.Current.Request.ApplicationPath + "/" + path + ".ashx'></script>");
            }
            else
            {
                if (!objPage.ClientScript.IsClientScriptBlockRegistered(objPage.GetType(), "common"))
                    objPage.ClientScript.RegisterClientScriptBlock(objPage.GetType(), "common", "<script type='text/javascript' src='/common__ajax.ashx'></script>");
                objPage.ClientScript.RegisterClientScriptBlock(objType, path, "<script type='text/javascript' src='" + path + ".ashx'></script>");
            }

            configAtClientSide = configureAtClientSide;

        }

    }
}
