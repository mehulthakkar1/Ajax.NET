using System;
using System.Linq;
using System.Reflection;
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
        private static readonly string tab = "\t";
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

        public static string Generate(Type classType)
        {
            var isController = classType.IsSubclassOf(typeof(Controller));
            
            var assemblyName = classType.Assembly.GetName().Name.ToLower();
            var methods = classType.GetMethods().Where(method =>
                method.IsPublic && !method.IsSpecialName);
            
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
            sb.AppendLine("function " + className.ToUpper() + "() {");
            sb.Append(tab);
            sb.AppendLine("var ajaxMethod = new AjaxMethod();");

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
            sb.Append(tab);
            sb.AppendLine("ajaxMethod.initialize(" + strURL + ");");

            foreach (var methodinfo in methods)
            {
                var ma = (AjaxMethodAttribute[])methodinfo.GetCustomAttributes(typeof(AjaxMethodAttribute), true);
                if (ma.Length == 0)
                {
                    continue;
                }
                var methodName = methodinfo.Name;
                var callbackMethodName = "CallBack_" + methodinfo.Name; 
                var errorMethodName = "Error_" + methodinfo.Name; 
                string loadingMsg = null;
                var isAsync = false;
                var pi = methodinfo.GetParameters();
                var httpVerb = GetHttpVerb(methodinfo);

                if (ma[0].MethodName != null)
                {
                    methodName = ma[0].MethodName;
                }

                if (ma[0].IsCallBackRequired)
                {
                    if (ma[0].CallBackMethodName != null)
                    {
                        callbackMethodName = ma[0].CallBackMethodName;
                    }
                }

                if (ma[0].IsErrorRequired)
                {
                    if (ma[0].ErrorMethodName != null)
                    {
                        errorMethodName = ma[0].ErrorMethodName;
                    }
                }

                loadingMsg = ma[0].LoadingMessage;
                isAsync = ma[0].IsAsync;

                sb.Append(tab);
                sb.Append("this." + methodName + " = function(");
                var i = 0;
                foreach (var par in pi)
                {
                    sb.Append(par.Name);
                    if (i < pi.Length - 1)
                    {
                        sb.Append(", ");
                    }
                    i = i + 1;
                }

                sb.AppendLine(") {");
                i = 0;
                sb.Append(tab + tab);
                if (!ma[0].ReturnPromise)
                {
                    //at the moment for simple api controller, later will take care for attribute base routing
                    sb.Append("return ajaxMethod.invokeAjax('" + (methodinfo.Name) + "',{");
                }
                else
                {
                    //at the moment for simple api controller, later will take care for attribute base routing
                    sb.AppendLine("return new Promise(function(resolve, reject) {");
                    sb.Append(tab + tab + tab);
                    sb.Append("ajaxMethod.invokeAjax('" + (methodinfo.Name) + "', {");
                }
                foreach (var par in pi)
                {
                    sb.Append("'" + par.Name + "':" + par.Name);
                    if (i < pi.Length - 1)
                    {
                        sb.Append(", ");
                    }
                    i = i + 1;
                }
                sb.AppendLine("},");
                sb.Append(tab + tab + tab + tab);
                sb.Append("{");
                if (isAsync && !ma[0].ReturnPromise)
                {
                    sb.Append("'CallBack':'" + callbackMethodName + "', 'Error':'" + errorMethodName + "',");
                }

                if (loadingMsg != null && loadingMsg.Trim() != string.Empty)
                {
                    sb.Append("'LoadingMsg':'" + loadingMsg + "', ");
                }

                sb.Append("'IsAsync':" + isAsync.ToString().ToLower());
                sb.Append(", 'ControllerType':" + (isController ? "'Mvc'" : "''").ToString());
                sb.Append(", 'httpVerb':'" + httpVerb + "'");
                if (ma?.Count() > 0 && !ma[0].ReturnPromise)
                {
                    sb.AppendLine("});");//close invokeajax
                }
                else
                {
                    sb.Append(", 'onSuccess': resolve, 'onError': reject }");
                    sb.Append(tab + tab + tab);
                    sb.AppendLine(")"); //close promise
                    sb.Append(tab + tab);
                    sb.AppendLine("});"); //close promise
                }
                sb.Append(tab);
                sb.AppendLine("};");            
            }
            sb.Append(tab);
            sb.AppendLine("this.Abort = function() { ajaxMethod.Abort(); };");
            sb.AppendLine("};var " + className + " = new " + className.ToUpper() + "();");

            return sb.ToString();
        }

        public static string GenerateForApi(Type classType)
        {
            var strURL = "'/api/" + classType.Name.Replace("Controller", "") + "'";
            
            var assemblyName = classType.Assembly.GetName().Name.ToLower();
            var methods = classType.GetMethods().Where(method =>
                method.IsPublic && !method.IsSpecialName && method.Module.Name.ToLower().StartsWith(assemblyName))
                .OrderBy(m => m.Name).ThenBy(m => m.GetParameters().Count());
            var sb = new StringBuilder();
            sb.AppendLine("function " + classType.Name.ToUpper() + "() {");
            sb.Append(tab);
            sb.AppendLine("var ajaxMethod = new AjaxMethod();");
            sb.Append(tab);
            sb.AppendLine("ajaxMethod.initialize(" + strURL + ");");
            var methodNames = new string[methods.Count()];
            var mCount = 0;
            foreach (var methodinfo in methods)
            {
                var methodName = methodinfo.Name;
                var pi = methodinfo.GetParameters();
                var httpVerb = GetHttpVerb(methodinfo);
                if(methodNames.Any(m => m == methodName))
                {
                    var paramCount = pi.Count();
                    if(paramCount > 1)
                    {
                        methodName += "ByParam";
                    }
                    else if(paramCount == 1)
                    {
                        methodName += "By" + pi[0].Name;
                    }
                }
                methodNames[mCount++] = methodName;
                sb.Append(tab);
                sb.Append("this." + methodName + " = function(");

                var i = 0;
                foreach (var par in pi)
                {
                    sb.Append(par.Name);
                    if (i < pi.Length - 1)
                    {
                        sb.Append(", ");
                    }
                    i = i + 1;
                }

                sb.AppendLine(") {");
                sb.Append(tab + tab);
                //at the moment for simple api controller, later will take care for attribute base routing
                sb.AppendLine("return new Promise(function(resolve, reject) {");
                sb.Append(tab + tab + tab);
                sb.Append("ajaxMethod.invokeWebApi('',{");
                sb.Append("'ControllerType':'WebApi'");
                sb.Append(", 'httpVerb':'" + httpVerb + "'");
                sb.Append(", 'onSuccess': resolve, 'onError': reject");
                sb.AppendLine("},");
                
                var apiParams = new System.Collections.Generic.List<string>();
                var methodParams = new System.Collections.Generic.List<string>();
                foreach (var par in pi)
                {
                    var ba = par.CustomAttributes.Any(ca => ca.AttributeType.Name == "FromBodyAttribute");
                    if (!ba)
                    {
                        apiParams.Add("'" + par.Name + "':" + par.Name);
                    }
                    else
                    {
                        methodParams.Add("'" + par.Name + "':" + par.Name);
                    }
                }
                sb.Append(tab + tab + tab + tab);
                sb.Append("{");
                sb.Append(string.Join(", ", apiParams));
                sb.AppendLine("},");
                sb.Append(tab + tab + tab + tab);
                sb.Append("{");
                sb.Append(string.Join(", ", methodParams));
                sb.AppendLine("}");
                sb.Append(tab + tab + tab);
                sb.AppendLine(")");
                sb.Append(tab + tab);
                sb.AppendLine("});"); //close promise
                sb.Append(tab);
                sb.AppendLine("};");
            }
            sb.Append(tab);
            sb.AppendLine("this.Abort = function() { ajaxMethod.Abort(); };");
            sb.AppendLine("}; var " + classType.Name + " = new " + classType.Name.ToUpper() + "();");

            return sb.ToString();
        }

        private static string GetHttpVerb(MethodInfo methodinfo)
        {
            if ((methodinfo.CustomAttributes.Count() > 0 && methodinfo.GetCustomAttributes(typeof(HttpGetAttribute), true) != null) || methodinfo.Name.StartsWith("Get"))
            {
                return "GET";
            }
            else if ((methodinfo.CustomAttributes.Count() > 0 && methodinfo.GetCustomAttributes(typeof(HttpPutAttribute), true) != null) || methodinfo.Name.StartsWith("Put"))
            {
                return "PUT";
            }
            else if ((methodinfo.CustomAttributes.Count() > 0 && methodinfo.GetCustomAttributes(typeof(HttpDeleteAttribute), true) != null) || methodinfo.Name.StartsWith("Delete"))
            {
                return "DELETE";
            }

            return "POST";
        }
    }
}
