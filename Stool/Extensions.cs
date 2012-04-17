
using System;
using System.Collections;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Stool
{
    public static class Extensions
    {
        /// <summary>
        /// Serializes the <paramref name="data"/> using the <see cref="JsonSerializer"/> and writes the result to the <see cref="HttpResponse.Output"/> of <paramref name="context"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="data"></param>
        /// <remarks>
        /// If <paramref name="data"/> is null, either an empty <see cref="T:byte[]"/> will be serialized for enumerables or a new <see cref="object"/> will be serialized for other types.
        /// This is done to ensure the response is JSON compliant.
        /// </remarks>
        public static void Send<T>(this HttpContext context, T data) where T : class
        {
            var serializer = new JsonSerializer();
            context.Response.ContentType = "application/json";
            serializer.Serialize(
                context.Response.Output, data
                ?? (typeof(T).GetInterfaces().Contains(typeof(IEnumerable)) ? new byte[0] : new object()));
        }
    }
}
