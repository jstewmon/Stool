using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Commons.Collections;
using NVelocity;
using NVelocity.App;

namespace Stool
{
    public abstract class StoolApp
    {
        private string _dataKey = "data";
        public string DataKey
        {
            get { return _dataKey; }
            set { _dataKey = value; }
        }

        private string _templateDirectory = "~/templates";
        public string TemplateDirectory
        {
            get { return _templateDirectory; }
            set { _templateDirectory = value; }
        }

        private bool _useLayouts = true;
        public bool UseLayouts
        {
            get { return _useLayouts; }
            set { _useLayouts = value; }
        }

        public bool CascadeLayouts { get; set; }

        private string _layoutName = "layout.vm";
        public string LayoutName
        {
            get { return _layoutName; }
            set { _layoutName = value; }
        }

        public Action<HttpContext> Render<T>(string templatePath, Func<T> dataLoader)
        {
            return ctx =>
                       {
                           var velocity = new VelocityEngine();
                           var props = new ExtendedProperties();
                           props.AddProperty("file.resource.loader.path", ctx.Server.MapPath(TemplateDirectory));
                           velocity.Init(props);
                           var template = velocity.GetTemplate(templatePath);
                           var context = new VelocityContext();
                           context.Put(DataKey, dataLoader());
                           string layoutPath;
                           if (UseLayouts && !string.IsNullOrEmpty(layoutPath = FindLayout(velocity, templatePath, LayoutName)))
                           {
                               var layout = velocity.GetTemplate(layoutPath);
                               using (var writer = new StringWriter())
                               {
                                   template.Merge(context, writer);
                                   context.Put("childContent", writer.ToString());
                                   layout.Merge(context, ctx.Response.Output);
                               }
                           }
                           else template.Merge(context, ctx.Response.Output);
                       };
        }

        private string FindLayout(VelocityEngine velocity, string templatePath, string layoutName)
        {
            var layoutPath = JoinPaths(templatePath, layoutName);
            if (velocity.TemplateExists(layoutPath))
            {
                return layoutPath;
            }
            var parts = templatePath.Split('/');
            if (CascadeLayouts && parts.Length > 1)
            {
                var upOne = string.Join("/", parts.TakeWhile((s, i) => i < parts.Length - 1));
                return FindLayout(velocity, upOne, layoutName);
            }
            return null;
        }

        private static string JoinPaths(string p1, string p2)
        {
            if (!p1.StartsWith("/") && !p1.StartsWith("~"))
            {
                p1 = "~/" + p1;
            }
            return VirtualPathUtility.Combine(p1, p2).TrimStart('~', '/');
        }


        //private void Default(Action<HttpContext> render)
        //{
        //    RouteTable.Routes.Add(
        //        new Route("Default", new RouteHandler(render)));
        //}

        public static void Get(string path, Action<HttpContext> handler)
        {
            On(new[] { "GET" }, path, handler);
        }

        public static void Post(string path, Action<HttpContext> handler)
        {
            On(new[] { "POST" }, path, handler);
        }

        public static void On(IEnumerable<string> httpMethods, string path, Action<HttpContext> handler)
        {
            RouteTable.Routes.Add(
                new Route(path, new RouteHandler(handler))
                    {
                        Constraints = new RouteValueDictionary
                                          {
                                              {"httpMethod", new HttpMethodConstraint(httpMethods.ToArray())}
                                          }
                    });
        }
    }
}