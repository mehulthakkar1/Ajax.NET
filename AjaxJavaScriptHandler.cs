using System;
using System.Reflection;
using System.Text;
using System.Web;

namespace Ajax.NET
{
    public class AjaxJavaScriptHandler : IHttpHandler
    {
        public Type ClassType { get; }

        public AjaxJavaScriptHandler(Type classType)
        {
            ClassType = classType;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var lastMod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            var scripts = Scripts.Generate(ClassType);
            context.Response.AddHeader("Content-Type", "applicatoin/x-javascript");
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Cache.SetLastModified(lastMod);
            context.Response.Write(scripts);

        }
    }
}
