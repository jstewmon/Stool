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
            context.Response.StatusCode = code;
            context.Response.ContentType = "application/json";
            var serializer = new JsonSerializer();
            serializer.Serialize(context.Response.Output, data);
            _log.Debug("Sent response with status " + code);
        }
    }
}
