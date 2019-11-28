using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Ajax
{
    class AjaxEmbededJavaScriptHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            System.Reflection.Assembly aAss = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream aStream = null;
            string aAssName = null;

            aAssName = aAss.FullName.Split(',')[0];
            aStream = aAss.GetManifestResourceStream(aAssName + ".Ajax.js");
            System.IO.StreamReader aStreamReader = new System.IO.StreamReader(aStream);

            DateTime lastMod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            context.Response.AddHeader("Content-Type", "applicatoin/x-javascript");
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Cache.SetLastModified(lastMod);
            context.Response.Write(aStreamReader.ReadToEnd());
        }
    }
}
