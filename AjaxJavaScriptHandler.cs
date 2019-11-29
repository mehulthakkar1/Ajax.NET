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
            var mi = ClassType.GetMethods();
            var lastMod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            
            var path = ClassType.FullName + "," + ClassType.Assembly.FullName.Substring(0, ClassType.Assembly.FullName.IndexOf(",")) + "__ajax";
            var className = ClassType.BaseType.Name;
            var sb = new StringBuilder();
            sb.Append("function " + className.ToUpper() + "(){");
            sb.Append("var ajaxMethod = new AjaxMethod();");

            string strURL = null;

            if (HttpContext.Current.Request.ApplicationPath != "/")
            {
                strURL = "'" + HttpContext.Current.Request.ApplicationPath + "/" + path + Utility.HandlerExtention + "'";
            }
            else
            {
                strURL = "'" + path + Utility.HandlerExtention + "'";
            }

            sb.Append("ajaxMethod.initialize(" + strURL + ");");


            foreach (MethodInfo methodinfo in mi)
            {

                if (methodinfo.IsPublic)
                {
                    var ma = (AjaxMethodAttribute[])methodinfo.GetCustomAttributes(typeof(AjaxMethodAttribute), true);
                    string methodName = null;
                    string callbackMethodName = null;
                    string errorMethodName = null;
                    string loadingMsg = null;
                    bool isAsync = false;


                    if (ma.Length > 0)
                    {
                        var pi = methodinfo.GetParameters();
                        
                        if (ma[0].MethodName == null)
                        {
                            methodName = methodinfo.Name;
                        }
                        else
                        {
                            methodName = ma[0].MethodName;
                        }

                        if (ma[0].IsCallBackRequired)
                        {
                            if (ma[0].CallBackMethodName == null)
                            {
                                callbackMethodName = "CallBack_" + methodinfo.Name;
                            }
                            else
                            {
                                callbackMethodName = ma[0].CallBackMethodName;
                            }
                        }

                        if (ma[0].IsErrorRequired)
                        {
                            if (ma[0].ErrorMethodName == null)
                            {
                                errorMethodName = "Error_" + methodinfo.Name;
                            }
                            else
                            {
                                errorMethodName = ma[0].ErrorMethodName;
                            }
                        }

                        loadingMsg = ma[0].LoadingMessage;
                        isAsync = ma[0].IsAsync;

                        sb.Append("this." + methodName + " = function(");
                        var i = 0;
                        foreach (var par in pi)
                        {
                            sb.Append(par.Name);
                            if (i < pi.Length - 1)
                            {
                                sb.Append(",");
                            }
                            i = i + 1;
                        }

                        sb.Append("){");
                        i = 0;
                        sb.Append("return " + "ajaxMethod.invokeAjax('" + methodinfo.Name + "',{");
                        foreach (var par in pi)
                        {
                            sb.Append("'" + par.Name + "':" + par.Name);
                            if (i < pi.Length - 1)
                            {
                                sb.Append(",");
                            }
                            i = i + 1;
                        }
                        sb.Append("},");
                        sb.Append("{");
                        if (isAsync)
                            sb.Append("'CallBack':'" + callbackMethodName + "','Error':'" + errorMethodName + "',");
                        if (loadingMsg != null && loadingMsg.Trim() != string.Empty)
                            sb.Append("'LoadingMsg':'" + loadingMsg + "',");
                        sb.Append("'IsAsync':" + isAsync.ToString().ToLower() + "});");
                        sb.Append("};");
                    }

                }
            }

            sb.Append("this.Abort = function() { ajaxMethod.Abort(); };");           
            sb.Append("};var " + className + " = new " + className.ToUpper() + "();");

            context.Response.AddHeader("Content-Type", "applicatoin/x-javascript");
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Cache.SetLastModified(lastMod);
            context.Response.Write(sb.ToString());

        }
    }
}
