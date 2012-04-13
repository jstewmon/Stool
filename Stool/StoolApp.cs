using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Commons.Collections;
using NVelocity;
using NVelocity.App;
using NVelocity.Context;

namespace Stool
{
    //public class HelloHandler : AbstractAsyncHandler
    //{
    //    protected override Task ProcessRequestAsync(HttpContext context)
    //    {
    //        context.Response.ContentType = "text/plain";
    //        return context.Response.Output.WriteAsync("Hello World!");
    //    }
    //}

    

    public class RouteHandler : IRouteHandler
    {
        private readonly Action<HttpContext> _handler;

        public RouteHandler(Action<HttpContext> handler)
        {
            _handler = handler;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new AsyncHandler(_handler);
        }
    }

    public class AsyncHandler : IHttpAsyncHandler
    {
        private readonly Action<HttpContext> _processor;
        public AsyncHandler(Action<HttpContext> processor)
        {
            _processor = processor;
        }

        private Task ProcessRequestAsync(HttpContext context, AsyncCallback cb)
        {
            return Task.Factory.StartNew(() => _processor(context)).ContinueWith(task => cb(task));
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException("I want async!");
            _processor(context);
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return ProcessRequestAsync(context, cb);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            if (result == null)
                return;
            ((Task)result).Dispose();
        }
    }

    public class MyApp : StoolApp
    {
        public MyApp()
        {
            Get("foo/bar", FooBar);
            Get("home", Render("home.vm", GetHomeData));
            Get("sub/home", Render("sub/home.vm", GetHomeData));
        }

        public int GetHomeData()
        {
            return 5;
        }

        public void FooBar(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World!");
            context.Response.End();
        }
    }

    public class StoolApp
    {
        public static Action<HttpContext> Render<T>(string templatePath, Func<T> dataLoader)
        {
            return ctx =>
                       {

                           var velocity = new VelocityEngine();
                           var props = new ExtendedProperties();
                           props.AddProperty("file.resource.loader.path", ctx.Server.MapPath("~/templates"));
                           velocity.Init(props);
                           var template = velocity.GetTemplate(templatePath);

                           var context = new VelocityContext();
                           context.Put("data", dataLoader());
                           template.Merge(context, ctx.Response.Output);
                       }; 
        }

        public static void Get(string path, Action<HttpContext> handler)
        {
            On(new []{"GET"}, path, handler);
        }

        public static void Post(string path, Action<HttpContext> handler)
        {
            On(new[] { "GET" }, path, handler);
        }

        public static void On(IEnumerable<string> httpMethods, string path, Action<HttpContext> handler)
        {
            RouteTable.Routes.Add(
                new Route(path,
                          new RouteValueDictionary
                              {
                                  {"httpMethod", new HttpMethodConstraint(httpMethods.ToArray())}
                              },
                          new RouteHandler(handler)));
        }
    }
}