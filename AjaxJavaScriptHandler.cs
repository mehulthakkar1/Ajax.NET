using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Ajax
{
    public class AjaxJavaScriptHandler : IHttpHandler
    {

        private Type _ClassType;
        public Type ClassType
        {
            get { return _ClassType; }
        }

        public AjaxJavaScriptHandler(Type classType)
        {
            _ClassType = classType;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            System.Reflection.MethodInfo[] mi = ClassType.GetMethods();
            DateTime lastMod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int i = 0;
            string path = ClassType.FullName + "," + ClassType.Assembly.FullName.Substring(0, ClassType.Assembly.FullName.IndexOf(",")) + "__ajax";
            string className = ClassType.BaseType.Name;
            context.Response.AddHeader("Content-Type", "applicatoin/x-javascript");
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Cache.SetLastModified(lastMod);
            sb.Append("function " + className.ToUpper() + "(){");
            sb.Append("var ajaxMethod = new AjaxMethod();");

            string strURL = null;

            if (System.Web.HttpContext.Current.Request.ApplicationPath != "/")
            {
                strURL = "'" + System.Web.HttpContext.Current.Request.ApplicationPath + "/" + path + Ajax.Utility.HandlerExtention + "'";
            }
            else
            {
                strURL = "'" + path + Ajax.Utility.HandlerExtention + "'";
            }

            sb.Append("ajaxMethod.initialize(" + strURL + ");");


            foreach (System.Reflection.MethodInfo methodinfo in mi)
            {

                if (methodinfo.IsPublic)
                {
                    AjaxMethodAttribute[] ma = (AjaxMethodAttribute[])methodinfo.GetCustomAttributes(typeof(AjaxMethodAttribute), true);
                    string methodName = null;
                    string callbackMethodName = null;
                    string errorMethodName = null;
                    string loadingMsg = null;
                    bool isAsync = false;


                    if (ma.Length > 0)
                    {
                        System.Reflection.ParameterInfo[] pi = methodinfo.GetParameters();


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
                        i = 0;
                        foreach (System.Reflection.ParameterInfo par in pi)
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
                        foreach (System.Reflection.ParameterInfo par in pi)
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
            context.Response.Write(sb.ToString());

        }
    }
}
