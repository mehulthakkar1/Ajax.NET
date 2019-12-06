using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Ajax.NET
{
    public static class HtmlHelperExtension
    {
        public static IHtmlString AjaxScriptsFor<T>(this HtmlHelper htmlHelper, T oType)
        {
            var objType = oType.GetType();
            string path = objType.FullName + "," + objType.Assembly.FullName.Substring(0, objType.Assembly.FullName.IndexOf(","));
            var sbScript = new StringBuilder();
            if (System.Web.HttpContext.Current.Request.ApplicationPath != "/")
            {
                sbScript.AppendLine("<script type='text/javascript' src='" + System.Web.HttpContext.Current.Request.ApplicationPath + "/AjaxJs/'></script>");
                sbScript.AppendLine("<script type='text/javascript' src='" + System.Web.HttpContext.Current.Request.ApplicationPath + "/AjaxJs/ScriptFor?c=" + path + "'></script>");
                return new HtmlString(sbScript.ToString());
            }
            else
            {
                sbScript.AppendLine("<script type='text/javascript' src='/AjaxJs/'></script>");
                sbScript.AppendLine("<script type='text/javascript' src='/AjaxJs/ScriptFor?c=" + path + "'></script>");
                return new HtmlString(sbScript.ToString());
            }
        }
    }
}
