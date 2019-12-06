using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace Ajax.NET
{
    public class AjaxJsController : Controller
    {
        public ActionResult Index()
        {
            var aAss = Assembly.GetExecutingAssembly();
            var aAssName = aAss.FullName.Split(',')[0];
            var aStream = aAss.GetManifestResourceStream(aAssName + ".Ajax.js");
            return File(aStream, "text/javascript");
        }

        public ActionResult ScriptFor(string c)
        {
            var classType = Type.GetType(c);
            if(classType == null)
            {
                throw new Exception("No assembly found");
            }
            var scripts = Scripts.Generate(classType);
            return File(Encoding.ASCII.GetBytes(scripts) , "text/javascript");
        }
    }
}
