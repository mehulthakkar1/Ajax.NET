using System;

namespace Ajax.NET
{
    public class AjaxMethodAttribute : Attribute
    {
        public bool IsAsync { get; set; } = true;
        
        public bool IsCallBackRequired { get; set; } = true;

        public bool IsErrorRequired { get; set; } = true;

        public string LoadingMessage { get; set; }

        public string MethodName { get; set; }

        public string ErrorMethodName { get; set; }

        public string CallBackMethodName { get; set; }

        public AjaxMethodAttribute(string methodName, bool isCallBackRequired, bool isErrorRequired, string loadingMessage = "", bool isAsync = true)
        {
            MethodName = methodName;
            IsCallBackRequired = isCallBackRequired;
            IsErrorRequired = isErrorRequired;
            LoadingMessage = loadingMessage;
            IsAsync = isAsync;
        }

        public AjaxMethodAttribute(string methodName, string callBackMethodName, string errorMethodName, string loadingMessage, bool isAsync = true)
        {
            MethodName = methodName;
            if ((callBackMethodName != null))
            {
                CallBackMethodName = callBackMethodName;
            }

            if ((errorMethodName != null))
            {
                ErrorMethodName = errorMethodName;
            }
            LoadingMessage = loadingMessage;
            IsAsync = isAsync;
        }

        public AjaxMethodAttribute(string methodName, bool isAsync = true)
        {
            MethodName = methodName;
            IsAsync = isAsync;
        }

        public AjaxMethodAttribute(bool isAsync = true)
        {
            IsAsync = isAsync;
        }
    }
}
