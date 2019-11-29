using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;

namespace Ajax.NET
{
    public class AjaxMethodHandler : IHttpHandler, IRequiresSessionState
    {
        public Type ClassType { get; }

        public string MethodName { get; set; }

        public AjaxMethodHandler(Type classType, string methodName)
        {
            ClassType = classType;
            MethodName = methodName;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            object res = null;
            var i = 0;
            var methodinfo = ClassType.GetMethod(MethodName);
            var parameters = methodinfo.GetParameters();
            var args = new object[parameters.Length];

            foreach (var param in parameters)
            {
                args[i++] = param.DefaultValue;
            }

            i = 0;

            if (context.Request.RequestType.ToUpper() == "POST")
            {
                foreach (ParameterInfo param in parameters)
                {
                    if ((context.Request.Form[param.Name] != null))
                    {
                        args[i] = Deserialize(context.Request.Form[param.Name], param.ParameterType);
                    }
                    i = i + 1;
                }
            }
            else if (context.Request.RequestType.ToUpper() == "GET")
            {
                foreach (ParameterInfo param in parameters)
                {
                    if ((context.Request.QueryString[param.Name] != null))
                    {
                        args[i] = Deserialize(context.Request.QueryString[param.Name], param.ParameterType);
                    }
                    i = i + 1;
                }
            }

            if (methodinfo.IsStatic)
            {
                res = ClassType.InvokeMember(MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase, null, null, args);
            }
            else
            {
                object o = Activator.CreateInstance(ClassType);

                try
                {
                    res = methodinfo.Invoke(o, args);
                }
                catch (Exception ex)
                {
                }
            }

            context.Response.Expires = 0;
            context.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            context.Response.AddHeader("Content-Type", "text/plain");
            context.Response.ContentType = "text/plain";
            try
            {
                context.Response.Write(Serialize(res).ToString());
            }
            catch (Exception ex)
            {
                CallBackError newErr = new CallBackError();
                newErr.ErrMsg = ex.Message.Replace("'", "\\'");
                context.Response.Status = "500 OK";
                context.Response.Write(Serialize(newErr).ToString());
            }
            context.Response.End();
        }

        public object Deserialize(object obj, Type paramType = null)
        {
            var objType = obj.GetType();

            var rObj = new Regex("^(?<Value>(True|False))$", RegexOptions.IgnoreCase);
            var rMatch = rObj.Match(obj.ToString());

            if (rMatch.Success)
            {
                return Convert.ToBoolean(rMatch.Groups["Value"].Value);
            }
            else
            {
                rObj = new Regex("^('(?<Value>[a-zA-Z0-9\\-.,!@#$%^&*()_+;:\"'|~`<>?/{}\\s]{1,20000000})'|(?<Value>[0-9.]{1,20000000}))$");
                rMatch = rObj.Match(obj.ToString());
                if (rMatch.Success)
                {
                    if (object.ReferenceEquals(paramType, typeof(string)))
                    {
                        return Convert.ToString(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(Guid)))
                    {
                        return new Guid(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(int)) || object.ReferenceEquals(paramType, typeof(Nullable<int>)))
                    {
                        return Convert.ToInt32(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(short)) || object.ReferenceEquals(paramType, typeof(Nullable<short>)))
                    {
                        return Convert.ToInt16(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(long)) || object.ReferenceEquals(paramType, typeof(Nullable<long>)))
                    {
                        return Convert.ToInt64(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(ushort)) || object.ReferenceEquals(paramType, typeof(Nullable<ushort>)))
                    {
                        return Convert.ToUInt16(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(uint)) || object.ReferenceEquals(paramType, typeof(Nullable<uint>)))
                    {
                        return Convert.ToUInt32(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(ulong)) || object.ReferenceEquals(paramType, typeof(Nullable<ulong>)))
                    {
                        return Convert.ToUInt64(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(double)) || object.ReferenceEquals(paramType, typeof(Nullable<double>)))
                    {
                        return Convert.ToDouble(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(decimal)) || object.ReferenceEquals(paramType, typeof(Nullable<decimal>)))
                    {
                        return Convert.ToDecimal(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(DateTime)))
                    {
                        return Convert.ToDateTime(rMatch.Groups["Value"].Value);
                    }
                    else if (object.ReferenceEquals(paramType, typeof(bool)) || object.ReferenceEquals(paramType, typeof(Nullable<bool>)))
                    {
                        return Convert.ToBoolean(rMatch.Groups["Value"].Value);
                    }
                    else
                    {
                        return rMatch.Groups["Value"].Value;
                    }
                }
                else
                {
                    rObj = new Regex("^\\[(?<Value>[a-zA-Z0-9\\-.,!@#$%^&*()_+;:\",'|~`<>?/{}\\s]*)\\]$");
                    rMatch = rObj.Match(obj.ToString());
                    
                    if (rMatch.Success)
                    {
                        string[] arr = rMatch.Groups["Value"].Value.Split(',');
                        Array returnArr = null;
                        if (arr.Length > 0)
                        {
                            var sArr = new ArrayList();
                            foreach (string d in arr)
                            {
                                sArr.Add(Deserialize(d, d.GetType()));
                            }
                            returnArr = sArr.ToArray(paramType.GetElementType());

                        }
                        return returnArr;
                    }
                    else
                    {
                        rObj = new Regex("^\\{(?<Value>[\"][a-zA-Z0-9]*[\"][:][']?[a-zA-Z0-9\\-.,!@#$%^&*()_+;:\",|~`<>?/{}\\s]*[']?[,]?)+\\}$");
                        rMatch = rObj.Match(obj.ToString());
                        if (rMatch.Success)
                        {
                            object o = Activator.CreateInstance(paramType);
                            rObj = new Regex("([\"][a-zA-Z0-9]+[\"][:]['][a-zA-Z0-9\\-.,!@#$%^&*()_+;:\",|~`<>?/{}\\s]+[']|[\"][a-zA-Z0-9]+[\"][:][0-9.]+)");
                            var rMatches = rObj.Matches(obj.ToString());
                            foreach (Match rMatch_loopVariable in rMatches)
                            {
                                rMatch = rMatch_loopVariable;
                                rObj = new Regex("[\"](?<Prop>[a-zA-Z0-9]+)[\"][:][']?(?<Value>[a-zA-Z0-9\\-.,!@#$%^&*()_+;:\",|~`<>?/{}\\s]+)[']?");
                                var rMatchProp = rObj.Match(rMatch.Value);
                                try
                                {
                                    if (rMatchProp.Success)
                                    {
                                        PropertyInfo propInfo = paramType.GetProperty(rMatchProp.Groups["Prop"].Value);
                                        propInfo.SetValue(o, Deserialize("'" + rMatchProp.Groups["Value"].Value + "'", propInfo.PropertyType), null);
                                    }

                                }
                                catch (Exception ex)
                                {
                                }


                            }
                            return o;
                        }
                    }

                }

            }

            return obj;

        }

        public string Serialize(object obj)
        {
            var sb = new StringBuilder();

            if (obj == null)
            {
                return "null";
            }
            var objType = obj.GetType();

            if (object.ReferenceEquals(objType, typeof(string)))
            {
                sb.Append("'" + obj.ToString() + "'");
            }
            else if (object.ReferenceEquals(objType, typeof(int)) | object.ReferenceEquals(objType, typeof(decimal)) | object.ReferenceEquals(objType, typeof(short)) | object.ReferenceEquals(objType, typeof(long)) | object.ReferenceEquals(objType, typeof(uint)) | object.ReferenceEquals(objType, typeof(ulong)) | object.ReferenceEquals(objType, typeof(ushort)) | object.ReferenceEquals(objType, typeof(double)))
            {
                sb.Append(obj.ToString());
            }
            else if (object.ReferenceEquals(objType, typeof(bool)))
            {
                sb.Append(obj.ToString().ToLower());
            }
            else if (object.ReferenceEquals(objType, typeof(DateTime)))
            {
                sb.Append("new Date('" + Convert.ToDateTime(obj.ToString()).ToString("MM/dd/yyyy hh:mm:ss") + "')");
            }
            else if (object.ReferenceEquals(objType, typeof(Hashtable)))
            {
                sb.Append("{");
                foreach (DictionaryEntry o in (Hashtable)obj)
                {
                    if (sb.ToString() != "{")
                    {
                        sb.Append(",");
                    }
                    sb.Append("'" + o.Key + "':" + Serialize(o.Value));
                }
                sb.Append("}");
            }
            else if (objType.IsArray)
            {
                sb.Append("[");
                if (objType.Name == "DateTime[]")
                {
                    DateTime[] objs = (DateTime[])obj;
                    foreach (object o in objs)
                    {
                        if (sb.ToString() != "[")
                            sb.Append(",");
                        if ((o != null))
                        {
                            sb.Append(Serialize(o));
                        }
                        else
                        {
                            sb.Append("''");
                        }
                    }
                }
                else
                {
                    
                    IEnumerable objs = obj as IEnumerable;
                    foreach (object o in objs)
                    {
                        if (sb.ToString() != "[")
                            sb.Append(",");
                        if ((o != null))
                        {
                            sb.Append(Serialize(o));
                        }
                        else
                        {
                            sb.Append("''");
                        }
                    }
                }
                sb.Append("]");
            }
            else if (objType.IsAssignableFrom(typeof(DataSet)))
            {
                DataSet ds = (DataSet)obj;
                sb.Append("{");
                sb.Append("Type:'" + objType.Name + "',DataSetName:'" + ds.DataSetName + "',TableCount:" + ds.Tables.Count + ",");
                sb.Append("Tables:[");
                for (int i = 0; i <= ds.Tables.Count - 1; i++)
                {
                    if (i > 0)
                        sb.Append(",");
                    sb.Append(Serialize(ds.Tables[i]));
                }
                sb.Append("]");
                sb.Append("}");

            }
            else if (objType.IsAssignableFrom(typeof(DataTable)))
            {
                DataTable dt = (DataTable)obj;
                sb.Append("{");
                sb.Append("Type:'" + objType.Name + "',TableName:'" + dt.TableName + "',RowCount:" + dt.Rows.Count + ",");
                sb.Append("Rows:[");
                for (int i = 0; i <= dt.Rows.Count - 1; i++)
                {
                    if (i > 0)
                        sb.Append(",");
                    sb.Append(Serialize(dt.Rows[i]));
                }
                sb.Append("]");
                sb.Append("}");

            }
            else if (objType.IsAssignableFrom(typeof(DataRow)))
            {
                DataRow dr = (DataRow)obj;
                sb.Append("{");
                sb.Append("Type:'" + objType.Name + "',");
                sb.Append("Columns:[");
                for (int i = 0; i <= dr.ItemArray.Length - 1; i++)
                {
                    if (i > 0)
                        sb.Append(",");
                    sb.Append("{");
                    sb.Append("Name:'" + dr.Table.Columns[i].ColumnName + "',Value:" + (dr[i] == null ? "null" : Serialize(dr[i])));
                    sb.Append("}");
                }
                sb.Append("]");
                sb.Append("}");

            }
            else if (typeof(IEnumerable).IsAssignableFrom(objType) || typeof(IEnumerable<>).IsAssignableFrom(objType))
            {
                bool useCurly=false;
                
                
                foreach (object item in (IEnumerable)obj)
                {
                    if (sb.ToString() != string.Empty && (sb.ToString() != "[" || sb.ToString() != "{"))
                    {
                        sb.Append(",");
                    }
                    else if(sb.ToString() == string.Empty)
                    {
                        if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                            useCurly = true;
                        if (useCurly)
                            sb.Append("{");
                        else
                            sb.Append("[");
                    }
                    sb.Append(Serialize(item));
                }
                if(useCurly)
                    sb.Append("}");
                else
                    sb.Append("]");
            }
            else if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var key = objType.GetProperty("Key");
                var value = objType.GetProperty("Value");
                object keyObj = key.GetValue(obj, null);
                object valueObj = value.GetValue(obj, null);
                sb.Append(keyObj.ToString() + ":" + Serialize(valueObj));
            }
            else if (objType.IsClass)
            {
                var i = 0;
                var fields = objType.GetFields(BindingFlags.Public);
                var Props = objType.GetProperties();

                sb.Append("{");
                sb.Append("type:'" + objType.Name + "'");
                if (fields.Length > 0 | Props.Length > 0)
                {
                    sb.Append(",");
                }
                foreach (var field in fields)
                {
                    if (i > 0)
                        sb.Append(",");
                    sb.Append(field.Name + ":" + Serialize(field.GetValue(obj)));
                    i = i + 1;
                }

                int j = 0;
                foreach (var prop in Props)
                {
                    var mi = prop.GetGetMethod();
                    if (i > 0)
                    {
                        sb.Append(",");
                        i = 0;
                    }

                    if (j > 0)
                        sb.Append(",");

                    sb.Append(prop.Name + ":" + Serialize(mi.Invoke(obj, null)));
                    j = j + 1;
                }
                sb.Append("}");
            }
            return sb.ToString();

        }
    }
}

class CallBackError
{
    public int ErrNum { get; set; }
    public string ErrMsg { get; set; }
}

