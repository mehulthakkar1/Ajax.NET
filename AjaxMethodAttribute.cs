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

        public AjaxMethodAttribute(string methodName, bool isCallBackRequired, bool isErrorRequired, string LoadingMessage = "", bool isAsync = true)
        {
            MethodName = methodName;
            IsCallBackRequired = isCallBackRequired;
            IsErrorRequired = isErrorRequired;
            this.LoadingMessage = LoadingMessage;
            IsAsync = isAsync;
        }

        public AjaxMethodAttribute(string methodName, string CallBackMethodName, string ErrorMethodName, string LoadingMessage, bool isAsync = true)
        {
            MethodName = methodName;
            if ((CallBackMethodName != null))
            {
                this.CallBackMethodName = CallBackMethodName;
            }

            if ((ErrorMethodName != null))
            {
                this.ErrorMethodName = ErrorMethodName;
            }
            this.LoadingMessage = LoadingMessage;
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
