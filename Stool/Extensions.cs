using System.Web;
using Newtonsoft.Json;
using log4net;

namespace Stool
{
    public static class Extensions
    {
        private static ILog _log = LogManager.GetLogger(typeof (Extensions));

        public static void Send<T>(this HttpContext context, T data)
        {
            context.Send(data, 200);
        }
        public static void Send<T>(this HttpContext context, T data, int code)
        {
            _log.Debug("Sending response with status " + code);

            bool jsonp = false;
            string callback = string.Empty;
            if(context.Items.Contains("allow-jsonp") && (bool)context.Items["allow-jsonp"])
            {
                callback = context.Request.QueryString["callback"];
                jsonp = !string.IsNullOrEmpty(callback);
            }

            context.Response.StatusCode = code;
            context.Response.ContentType = jsonp ? "text/javascript" : "application/json";
            if(jsonp)
            {
                context.Response.Write(callback + "(");
            }

            var serializer = new JsonSerializer();
            serializer.Serialize(context.Response.Output, data);

            if (jsonp)
            {
                context.Response.Write(")");
            }

            _log.Debug("Sent response with status " + code);
        }
    }
}
