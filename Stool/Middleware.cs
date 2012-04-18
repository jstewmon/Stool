using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stool
{
    public static class Middleware
    {
        /// <summary>
        /// Loads body into a <see cref="JObject"/> and adds the result into <paramref name="ctx.Items"/> with the key "body"
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="next"></param>
        public static void ParseBody(HttpContext ctx, Action next)
        {
            if (ctx.Request.HttpMethod != "POST")
            {
                next();
                return;
            }

            using (var reader = new StreamReader(ctx.Request.InputStream))
            {
                ctx.Items.Add("body", JObject.Load(new JsonTextReader(reader)));
            }
            next();
        }
    }
}
