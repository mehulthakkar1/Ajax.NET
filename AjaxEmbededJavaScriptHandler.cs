using System;
using System.IO;
using System.Reflection;
using System.Web;

namespace Ajax.NET
{
    internal class AjaxEmbededJavaScriptHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var aAss = Assembly.GetExecutingAssembly();
            var aAssName = aAss.FullName.Split(',')[0];
            var aStream = aAss.GetManifestResourceStream(aAssName + ".Ajax.js");
            using (var aStreamReader = new StreamReader(aStream))
            {
                var lastMod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                context.Response.AddHeader("Content-Type", "application/x-javascript");
                context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                context.Response.Cache.SetLastModified(lastMod);
                context.Response.Write(aStreamReader.ReadToEnd());
            }
        }
    }
}
