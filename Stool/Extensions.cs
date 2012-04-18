using System.Web;
using Newtonsoft.Json;

namespace Stool
{
    public static class Extensions
    {
        public static void Send<T>(this HttpContext context, T data)
        {
            context.Send(data, 200);
        }
        public static void Send<T>(this HttpContext context, T data, int code)
        {
            context.Response.StatusCode = code;
            context.Response.ContentType = "application/json";
            var serializer = new JsonSerializer();
            serializer.Serialize(context.Response.Output, data);
        }
    }
}
