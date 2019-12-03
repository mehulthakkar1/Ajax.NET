using System.Web.UI;

namespace Ajax.NET
{
    public class Extention
    {
        public static string OfHandler = ".ashx";
    }
    public class Generate
    {
        public static void Scripts(object oType)
        {
            var objType = oType.GetType();
            string path = objType.FullName + "," + objType.Assembly.FullName.Substring(0, objType.Assembly.FullName.IndexOf(",")) + "__ajax";
            var objPage = ((Control)oType).Page;

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

        }

    }
}
