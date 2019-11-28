using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ajax
{
    public class AjaxMethodAttribute : Attribute
    {
        private string _MethodName;
        private string _CallBackMethodName;
        private string _ErrorMethodName;
        private string _LoadingMsg;
        private bool _IsCallBackRequired = true;
        private bool _IsErrorRequired = true;

        private bool _IsAsync = true;
        public bool IsAsync
        {
            get { return _IsAsync; }
            set { _IsAsync = value; }
        }


        public bool IsCallBackRequired
        {
            get { return _IsCallBackRequired; }
            set { _IsCallBackRequired = value; }
        }

        public bool IsErrorRequired
        {
            get { return _IsErrorRequired; }
            set { _IsErrorRequired = value; }
        }

        public string LoadingMessage
        {
            get { return _LoadingMsg; }
            set { _LoadingMsg = value; }
        }

        public string MethodName
        {
            get { return _MethodName; }
            set { _MethodName = value; }
        }

        public string ErrorMethodName
        {
            get { return _ErrorMethodName; }
            set { _ErrorMethodName = value; }
        }

        public string CallBackMethodName
        {
            get { return _CallBackMethodName; }
            set { _CallBackMethodName = value; }
        }

        public AjaxMethodAttribute(string methodName, bool isCallBackRequired, bool isErrorRequired, string LoadingMessage = "", bool isAsync = true)
        {
            _MethodName = methodName;
            _IsCallBackRequired = isCallBackRequired;
            _IsErrorRequired = isErrorRequired;
            _LoadingMsg = LoadingMessage;
            _IsAsync = isAsync;
        }

        public AjaxMethodAttribute(string methodName, string CallBackMethodName, string ErrorMethodName, string LoadingMessage, bool isAsync = true)
        {
            _MethodName = methodName;
            if ((CallBackMethodName != null))
            {
                _CallBackMethodName = CallBackMethodName;
            }

            if ((ErrorMethodName != null))
            {
                _ErrorMethodName = ErrorMethodName;
            }
            _LoadingMsg = LoadingMessage;
            _IsAsync = isAsync;
        }

        public AjaxMethodAttribute(string methodName, bool isAsync = true)
        {
            _MethodName = methodName;
            _IsAsync = isAsync;
        }

        public AjaxMethodAttribute(bool isAsync = true)
        {
            _IsAsync = isAsync;
        }
    }
}
