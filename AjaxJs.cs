using System;
using System.IO;
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
            var assemblyNameArray = c.Split('.');
            var classType = Type.GetType(c + ", " + assemblyNameArray[0]);
            if(classType == null)
            {
                var assemblyName = new StringBuilder();
                assemblyName.Append(assemblyNameArray[0]);
                for (var i = 1; i < assemblyNameArray.Length; i++)
                {
                    assemblyName.Append("." + assemblyNameArray[i]);
                    classType = Type.GetType(c + ", " + assemblyName.ToString());

                    if(classType != null)
                    {
                        break;
                    }
                }
            }

            if(classType == null)
            {
                throw new Exception("No assembly found");
            }
            var scripts = Scripts.Generate(classType);
            return File(Encoding.ASCII.GetBytes(scripts) , "text/javascript");
        }
    }
}
