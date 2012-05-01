using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
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
        /// <remarks>
        /// The body must contain valid JSON.
        /// </remarks>
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

        public static void BodyToExpando(HttpContext ctx, Action next)
        {
            _log.Debug("Begin BodyToExpando " + ctx.Request.RawUrl);
            if (ctx.Request.HttpMethod != "POST")
            {
                _log.DebugFormat("httpMethod is {0}, calling next()", ctx.Request.HttpMethod);
                next();
                _log.Debug("End BodyToExpando " + ctx.Request.RawUrl);
                return;
            }

            using (var reader = new StreamReader(ctx.Request.InputStream))
            {
                try
                {
                    var serializer = new JsonSerializer();
                    var body = serializer.Deserialize<ExpandoObject>(new JsonTextReader(reader));
                    ctx.Items.Add("body", body);
                    _log.Debug("BodyToExpando successfully parsed the request body");
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
            _log.Debug("BodyToExpando calling next()");
            next();
            _log.Debug("End BodyToExpando " + ctx.Request.RawUrl);
        }

        /// <summary>
        /// Iterates <paramref name="context.Request.RequestContext.RouteData.Values"/> and adds the key value pairs to <paramref name="context.Items"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        public static void RouteDataContextItems(HttpContext context, Action next)
        {
            foreach (var rv in context.Request.RequestContext.RouteData.Values)
            {
                context.Items.Add(rv.Key, rv.Value);
            }
            next();
        }

        /// <summary>
        /// Checks <paramref name="context.Items['data']"/> for an instance of <see cref="IEnumerable{T}"/>.
        /// If found, <paramref name="context.Items['pagesize']"/> and <paramref name="context.Items['page']"/> will be used to return paged data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <remarks>
        /// pagesize must be an integer greater than zero.
        /// If page is negative, reverse paging will be performed.  For example, a page value of -1 returns the last page.
        /// </remarks>
        /// <exception cref="Exception">Thrown if pagesize is null or less than zero</exception>
        public static void PageData<T>(HttpContext context, Action next)
        {
            var data = context.Items["data"] as IEnumerable<T>;
            if (data == null)
            {
                next();
                return;
            }
            var datasize = data.Count();
            var pagesize = Convert.ToInt32(context.Items["pagesize"]);
            if(pagesize == 0) throw new InvalidOperationException("context.Items[\"pagesize\"] must be an integer greater than zero");
            var page = Convert.ToInt32(context.Items["page"]);
            if (page < 0) page = datasize/pagesize + page + 1;
            var pagecount = datasize/pagesize;
            if (datasize % pagesize > 0)
                pagecount++;
            context.Send(new
                             {
                                 datasize,
                                 page,
                                 pagesize,
                                 pagecount,
                                 data = data.Skip((page - 1) * pagesize).Take(pagesize)
                             });
        }
    }
}
