using System;
using System.IO;
using System.Web;

namespace Ajax.NET
{
    internal class AjaxHandlerFactory : IHttpHandlerFactory
    {
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            var fName = Path.GetFileNameWithoutExtension(context.Request.Path);
            var methodName = string.Empty;
            if (fName.Substring(fName.Length - 6) == "__ajax" & (requestType.ToUpper() == "POST" | requestType.ToUpper() == "GET"))
            {
                try
                {
                    fName = fName.Substring(0, fName.Length - 6);
                    methodName = context.Request.Headers["AJAX-METHOD"];
                }
                catch (Exception ex)
                {
                    methodName = string.Empty;
                }
                var classType = Type.GetType(fName);
                if (methodName != null)
                {
                    AjaxMethodHandler objMethodHandler = new AjaxMethodHandler(classType, methodName);
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
                        classType = Type.GetType(fName);
                        if ((classType != null))
                        {
                            AjaxJavaScriptHandler objJsHandler = new AjaxJavaScriptHandler(classType);
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
