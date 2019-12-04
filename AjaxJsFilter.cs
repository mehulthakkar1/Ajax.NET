using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Ajax.NET
{
    public class AjaxJsFileAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            response.Filter = new StripEnclosingScriptTagsFilter(response.Filter);
            response.ContentType = "text/javascript";
        }

        private class StripEnclosingScriptTagsFilter : MemoryStream
        {
            private static readonly Regex LeadingOpeningScriptTag;
            private static readonly Regex TrailingClosingScriptTag;

            private readonly StringBuilder _output;
            private readonly Stream _responseStream;

            static StripEnclosingScriptTagsFilter()
            {
                LeadingOpeningScriptTag = new Regex(@"^\s*<script[^>]*>", RegexOptions.Compiled);
                TrailingClosingScriptTag = new Regex(@"</script>\s*$", RegexOptions.Compiled);
            }

            public StripEnclosingScriptTagsFilter(Stream responseStream)
            {
                _responseStream = responseStream;
                _output = new StringBuilder();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                string response = GetStringResponse(buffer, offset, count);
                _output.Append(response);
            }

            public override void Flush()
            {
                string response = _output.ToString();

                if (LeadingOpeningScriptTag.IsMatch(response) && TrailingClosingScriptTag.IsMatch(response))
                {
                    response = LeadingOpeningScriptTag.Replace(response, string.Empty);
                    response = TrailingClosingScriptTag.Replace(response, string.Empty);
                }

                WriteStringResponse(response);
                _output.Clear();
            }

            private static string GetStringResponse(byte[] buffer, int offset, int count)
            {
                byte[] responseData = new byte[count];
                Buffer.BlockCopy(buffer, offset, responseData, 0, count);

                return Encoding.Default.GetString(responseData);
            }

            private void WriteStringResponse(string response)
            {
                byte[] outdata = Encoding.Default.GetBytes(response);
                _responseStream.Write(outdata, 0, outdata.GetLength(0));
            }
        }
    }
}
