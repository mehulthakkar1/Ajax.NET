if (!window.XMLHttpRequest) {
    window.XMLHttpRequest = function () {
        var xmlHttp = null;
        var clsids = ["Msxml2.XMLHTTP.4.0", "MSXML2.XMLHTTP", "Microsoft.XMLHTTP"];
        for (var i = 0; i < clsids.length && xmlHttp == null; i++) {
            try {
                xmlHttp = new ActiveXObject(clsids[i]);
            }
            catch (e) {
            }
        }
        return xmlHttp;
    }
}

function AjaxMethod() {
    var newdiv = null;
    function ShowLoading(bool, msg) {
        if (msg == "" || msg == null) msg = "Loading....";
        if (bool == true && newdiv == null) {
            newdiv = document.createElement('div');
            newdiv.setAttribute('id', 'divShowLoading');
            newdiv.style.width = "100%";
            newdiv.style.height = 20;
            newdiv.style.left = 0;
            newdiv.style.top = 0;
            newdiv.style.position = "absolute";
            newdiv.style.overflow = "auto";
            newdiv.style.fontFamily = "verdana";
            newdiv.style.fontSize = 10;
            newdiv.style.color = "#ffdd11";
            newdiv.innerHTML = "<table style='background:#ff0000; font-size:11; color:#ffffff' height='100% width='100%' border='0' cellpadding='0' align='right'><tr><td align='right'>" + msg + "</td></tr></table>"
            document.body.appendChild(newdiv);
        }
        else if (bool == true && newdiv != null) {
            newdiv.style.visibility = "visible";
        }
        else if (bool == false && newdiv != null) {
            newdiv.style.visibility = "hidden";
        }
    };

    this.Abort = function () {
        this.xmlHttp.abort();
    };

    this.initialize = function (url) {
        this.xmlHttp = new XMLHttpRequest();
        this.CallBack = null;
        this.Url = url;
        this.Error = null;
        this.Result = null;
        this.async = true;
    };

    this.invokeService = function (args, command) {
        this.CallBack = command["onSuccess"];
        this.Error = command["onError"];
        var _this = this;

        this.xmlHttp.onreadystatechange = function () { _this.stateChange() };
        this.xmlHttp.open("POST", this.Url, this.async);
        this.xmlHttp.setRequestHeader("Content-Type", "application/json; charset=UTF-8");
        try { this.xmlHttp.send(args); }
        catch (e) { }
    };

    this.invokeAjax = function (methodName, args, command) {
        this.async = command["IsAsync"];
        if (this.async == undefined) {
            this.async = true;
        }
        this.CallBack = command["CallBack"];
        this.Error = command["Error"];
        var _this = this;

        if (command["LoadingMsg"] != '' && command["LoadingMsg"] != undefined) {
            try {
                if (typeof (eval(command["LoadingMsg"])) == "function")
                    eval(command["LoadingMsg"])();
            }
            catch (e) { ShowLoading(true, command["LoadingMsg"]); }
        }
        this.xmlHttp.onreadystatechange = function () { _this.stateChange(command["onSuccess"], command["onError"]) };
        var buildUrl = function () {
            var url = _this.Url + (command["ControllerType"] == "Mvc" ? "/" + methodName : "");
            if (methodName != "" && command["ControllerType"] != "") {
                url += "/";
            }
            url += (command["ControllerType"] != "Mvc" ? "?d=" + Date.parse(new Date()) : "");
            return url;
        };
        var url = buildUrl();
        this.xmlHttp.open(command["httpVerb"] || "POST", url, this.async);
        this.xmlHttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

        if (methodName != "" && !command["ControllerType"]) {
            this.xmlHttp.setRequestHeader("AJAX-METHOD", methodName);
        }

        var sendStr = "";
        var i = 0;
        var argsLen = 0;
        for (prop in args) argsLen++;
        for (prop in args) {
            var o = args[prop];
            if (i > 0 && i < argsLen)
                sendStr += "&";
            sendStr += prop + "=" + serialize(o);
            i++;
        }
        try
        {
            this.xmlHttp.send(sendStr);
        }
        catch (e) {
            console.log(e);
        }
        if (!this.async)
            return this.Result;
    };

    this.invokeWebApi = function (methodName, command, urlArgs, bodyArgs) {    
         var _this = this;

        this.xmlHttp.onreadystatechange = function () { _this.stateChange(command["onSuccess"], command["onError"]) };
        var buildUrl = function () {
            var url = _this.Url + "/" + methodName;
            if (methodName != "") {
                url += "/";
            }
            for (prop in urlArgs) {
                url += urlArgs[prop] + "/";
            }
            return url;
        };
        var url = buildUrl();
        this.xmlHttp.open(command["httpVerb"], url, true);
        this.xmlHttp.setRequestHeader("Content-Type", "application/json; charset=UTF-8");
     
        try {
            this.xmlHttp.send(JSON.stringify(bodyArgs));
        }
        catch (e) {
            console.log(e);
        }
    };

    function serialize(obj) {

        var returnVal;
        if (obj != undefined) {
            switch (obj.constructor) {
                case Array:
                    var vArr = "[";
                    for (var i = 0; i < obj.length; i++) {
                        if (i > 0) vArr += ",";
                        vArr += serialize(obj[i]);
                    }
                    vArr += "]"
                    return vArr;
                case String:
                    returnVal = escape("'" + obj + "'");
                    return returnVal;
                case Number:
                    returnVal = isFinite(obj) ? obj.toString() : null;
                    return returnVal;
                case Date:
                    returnVal = "#" + obj + "#";
                    return returnVal;
                default:
                    if (typeof obj == "object") {
                        var vobj = [];
                        for (attr in obj) {
                            if (typeof obj[attr] != "function") {
                                vobj.push('"' + attr + '":' + serialize(obj[attr]));
                            }
                        }
                        if (vobj.length > 0)
                            return "{" + vobj.join(",") + "}";
                        else
                            return "{}";
                    }
                    else {
                        return obj.toString();
                    }
            }
        }
        return null;
    };
    this.stateChange = function (onSuccess, onError) {
        
        try {

            if (this.xmlHttp.readyState != 4) {
                return;
            }
            if (this.xmlHttp.status == 200) {
                this.executeMethod(onSuccess || this.CallBack);
            } else {
                this.executeMethod(onError || this.Error);
            }
        }
        catch (e) {
        }
        ShowLoading(false);
    };
    this.executeMethod = function (methodname) {
        if (methodname != '') {
            var o = null;

            try {
                this.Result = eval("o=" + this.xmlHttp.responseText + ";");
            } catch (e) {
                this.Result = eval("o='" + this.xmlHttp.responseText + "';");
            }
            this.xmlHttp.abort();
            if (this.async) {
                if (methodname.constructor === String) {
                    eval(methodname + ("(o);"));
                } else {
                    methodname(this.Result);
                }
            }
        }

    };
}
var SmartAjax = {
    CallService : function (url, args, command) {
        var ajax = new AjaxMethod();
        ajax.initialize(url);
        ajax.invokeService(args, command);
    }
};