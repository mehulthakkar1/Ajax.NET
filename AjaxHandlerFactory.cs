using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace Ajax
{
    class AjaxHandlerFactory : IHttpHandlerFactory
    {
        

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            string fName = Path.GetFileNameWithoutExtension(context.Request.Path);
            Type ClassType = null;
            string MethodName = string.Empty;
            if (fName.Substring(fName.Length - 6) == "__ajax" & (requestType.ToUpper() == "POST" | requestType.ToUpper() == "GET"))
            {
                try
                {
                    fName = fName.Substring(0, fName.Length - 6);
                    MethodName = context.Request.Headers["AJAX-METHOD"];
                }
                catch (Exception ex)
                {
                    MethodName = string.Empty;
                }
                ClassType = Type.GetType(fName);
                if (MethodName != null)
                {
                    AjaxMethodHandler objMethodHandler = new AjaxMethodHandler(ClassType, MethodName);
                    return (IHttpHandler)objMethodHandler;
                }
                else
                {
                    if (fName == "common")
                    {
                        AjaxEmbededJavaScriptHandler objemJsHandler = new AjaxEmbededJavaScriptHandler();
                        return objemJsHandler;
                    }
                    else
                    {
                        ClassType = Type.GetType(fName);
                        if ((ClassType != null))
                        {
                            AjaxJavaScriptHandler objJsHandler = new AjaxJavaScriptHandler(ClassType);
                            return objJsHandler;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            
        }
    }
}
