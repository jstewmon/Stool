using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

namespace Stool
{
    public static class Middleware
    {
        private static ILog _log = LogManager.GetLogger(typeof(Middleware));

        /// <summary>
        /// Loads body into a <see cref="JObject"/> and adds the result into <paramref name="ctx.Items"/> with the key "body"
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="next"></param>
        public static void ParseBody(HttpContext ctx, Action next)
        {
            _log.Debug("Begin ParseBody");
            if (ctx.Request.HttpMethod != "POST")
            {
                _log.DebugFormat("httpMethod is {0}, calling next()", ctx.Request.HttpMethod);
                next();
                _log.Debug("End ParseBody");
                return;
            }

            using (var reader = new StreamReader(ctx.Request.InputStream))
            {
                try
                {
                    ctx.Items.Add("body", JObject.Load(new JsonTextReader(reader)));
                    _log.Debug("ParseBody successfully parsed the request body");
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    ctx.Request.InputStream.Seek(0, SeekOrigin.Begin);
                    using (var r = new StreamReader(ctx.Request.InputStream))
                        _log.Error(r.ReadToEnd());
                    throw;
                }
            }
            _log.Debug("ParseBody calling next()");
            next();
            _log.Debug("End ParseBody");
        }
    }
}
