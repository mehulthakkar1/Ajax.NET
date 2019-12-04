using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Ajax.NET
{
    public class Extention
    {
        public static string OfHandler = ".ashx";
    }
    public class Scripts
    {
        public static void Add(object oType)
        {
            var objType = oType.GetType();
            string path = objType.FullName + "," + objType.Assembly.FullName.Substring(0, objType.Assembly.FullName.IndexOf(",")) + "__ajax";
            var objPage = ((Control)oType).Page;

            if (System.Web.HttpContext.Current.Request.ApplicationPath != "/")
            {
                if (!objPage.ClientScript.IsClientScriptBlockRegistered(objPage.GetType(), "common"))
                    objPage.ClientScript.RegisterClientScriptBlock(objPage.GetType(), "common", "<script type='text/javascript' src='" + System.Web.HttpContext.Current.Request.ApplicationPath + "/common__ajax.ashx'></script>");
                objPage.ClientScript.RegisterClientScriptBlock(objType, path, "<script type='text/javascript' src='" + System.Web.HttpContext.Current.Request.ApplicationPath + "/" + path + ".ashx'></script>");
            }
            else
            {
                if (!objPage.ClientScript.IsClientScriptBlockRegistered(objPage.GetType(), "common"))
                    objPage.ClientScript.RegisterClientScriptBlock(objPage.GetType(), "common", "<script type='text/javascript' src='/common__ajax.ashx'></script>");
                objPage.ClientScript.RegisterClientScriptBlock(objType, path, "<script type='text/javascript' src='" + path + ".ashx'></script>");
            }

        }

        internal static string Generate(Type classType)
        {
            var isController = classType.IsSubclassOf(typeof(Controller));
            var mi = classType.GetMethods();
            
            var className = "";
            if (isController)
            {
                className = classType.Name;
            }
            else
            {
                className = classType.BaseType.Name;
            }
            var sb = new StringBuilder();
            sb.Append("function " + className.ToUpper() + "(){");
            sb.Append("var ajaxMethod = new AjaxMethod();");

            string strURL = null;

            if (isController)
            {
                if (HttpContext.Current.Request.ApplicationPath != "/")
                {
                    strURL = "'/" + HttpContext.Current.Request.ApplicationPath + "/" + className.Replace("Controller", "") + "'";
                }
                else
                {
                    strURL = "'/" + className.Replace("Controller", "") + "'";
                }
            }
            else
            {
                var path = classType.FullName + "," + classType.Assembly.FullName.Substring(0, classType.Assembly.FullName.IndexOf(",")) + "__ajax";
                if (HttpContext.Current.Request.ApplicationPath != "/")
                {
                    strURL = "'" + HttpContext.Current.Request.ApplicationPath + "/" + path + Extention.OfHandler + "'";
                }
                else
                {
                    strURL = "'" + path + Extention.OfHandler + "'";
                }
            }
            sb.Append("ajaxMethod.initialize(" + strURL + ");");

            foreach (var methodinfo in mi)
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

                        var httpVerb = "POST";
                        if (methodinfo.GetCustomAttributes(typeof(HttpGetAttribute), true) != null)
                        {
                            httpVerb = "GET";
                        }
                        else if(methodinfo.GetCustomAttributes(typeof(HttpPutAttribute), true) != null) {
                            httpVerb = "PUT";
                        }
                        else if (methodinfo.GetCustomAttributes(typeof(HttpDeleteAttribute), true) != null)
                        {
                            httpVerb = "DELETE";
                        }

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
                        if (!ma[0].ReturnPromise)
                        {
                            sb.Append("return ajaxMethod.invokeAjax('" + methodinfo.Name + "',{");
                        }
                        else
                        {
                            sb.Append("return new Promise(function(resolve, reject) { ajaxMethod.invokeAjax('" + methodinfo.Name + "',{");
                        }
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
                        if (isAsync && !ma[0].ReturnPromise)
                            sb.Append("'CallBack':'" + callbackMethodName + "','Error':'" + errorMethodName + "',");
                        if (loadingMsg != null && loadingMsg.Trim() != string.Empty)
                            sb.Append("'LoadingMsg':'" + loadingMsg + "',");
                        sb.Append("'IsAsync':" + isAsync.ToString().ToLower());
                        sb.Append(", 'isController':" + isController.ToString().ToLower());
                        sb.Append(", 'httpVerb':'" + httpVerb + "'");
                        if (!ma[0].ReturnPromise)
                        {
                            sb.Append("});");//close invokeajax
                        }
                        else
                        {
                            sb.Append(", 'onSuccess': resolve, 'onError': reject");
                            sb.Append("})});"); //close promise
                        }

                        sb.Append("};");
                    }

                }
            }

            sb.Append("this.Abort = function() { ajaxMethod.Abort(); };");
            sb.Append("};var " + className + " = new " + className.ToUpper() + "();");

            return sb.ToString();
        }

    }
}
