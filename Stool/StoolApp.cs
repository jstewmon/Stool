﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Commons.Collections;
using NVelocity;
using NVelocity.App;
using log4net;

namespace Stool
{
    public abstract class StoolApp
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StoolApp));

        /// <summary>
        ///  When using <see cref="Render"/>, this will be the name of the token through which data can be accessed in the template.
        /// </summary>
        public string DataKey
        {
            get { return _dataKey; }
            set { _dataKey = value; }
        }
        private string _dataKey = "data";

        /// <summary>
        /// Virtual path to your templates
        /// </summary>
        /// <remarks>Defaults to "~/templates"</remarks>
        public string TemplateDirectory
        {
            get { return _templateDirectory; }
            set { _templateDirectory = value; }
        }
        private string _templateDirectory = "~/templates";

        /// <summary>
        /// Determines whether the a layout will be searched for when rendering a template.
        /// </summary>
        public bool UseLayouts
        {
            get { return _useLayouts; }
            set { _useLayouts = value; }
        }
        private bool _useLayouts = true;

        /// <summary>
        /// Whether or not parent directories should be searched for a layout if one is not found in the same directory as the template
        /// </summary>
        public bool CascadeLayouts { get; set; }

        /// <summary>
        /// The name of layout files
        /// </summary>
        /// <remarks>Defaults to "layout.vm"</remarks>
        public string LayoutName
        {
            get { return _layoutName; }
            set { _layoutName = value; }
        }
        private string _layoutName = "layout.vm";

        public Action<HttpContext> Render(string templatePath)
        {
            Log.Debug("Begin Render " + templatePath);
            var handler =  Render<object>(templatePath, () => null);
            Log.Debug("End Render " + templatePath);
            return handler;
        } 

        public Action<HttpContext> Render<T>(string templatePath, Func<T> dataLoader)
        {
            return ctx =>
                       {
                           Log.DebugFormat("Begin Render templatePath: {0}, dataLoader: {1}", templatePath, dataLoader);
                           
                           var velocity = new VelocityEngine();
                           var props = new ExtendedProperties();
                           props.AddProperty("file.resource.loader.path", ctx.Server.MapPath(TemplateDirectory));
                           
                           Log.DebugFormat("Initializing velocity with file.resource.loader.path: {0}", props["file.resource.loader.path"]);
                           velocity.Init(props);
                           
                           Log.Debug("Getting template: " + templatePath);
                           var template = velocity.GetTemplate(templatePath);
                           
                           var context = new VelocityContext();
                           context.Put(DataKey, dataLoader());
                           
                           string layoutPath;
                           if (UseLayouts && !string.IsNullOrEmpty(layoutPath = FindLayout(velocity, templatePath, LayoutName)))
                           {
                               Log.DebugFormat("Using layout {0}", layoutPath);
                               var layout = velocity.GetTemplate(layoutPath);
                               using (var writer = new StringWriter())
                               {
                                   template.Merge(context, writer);
                                   context.Put("childContent", writer.ToString());

                                   Log.Debug("Merging template with layout");
                                   layout.Merge(context, ctx.Response.Output);
                               }
                           }
                           else
                           {
                               Log.Debug("Merging without layout");
                               template.Merge(context, ctx.Response.Output);
                           }
                           Log.DebugFormat("End Render templatePath: {0}, dataLoader: {1}", templatePath, dataLoader);
                       };

        }

        private string FindLayout(VelocityEngine velocity, string templatePath, string layoutName)
        {
            Log.DebugFormat("Begin FindLayout templatePath: {0}, layoutName: {1}", templatePath, layoutName);
            var layoutPath = JoinPaths(templatePath, layoutName);
            if (velocity.TemplateExists(layoutPath))
            {
                Log.DebugFormat("Found layoutPath: {0}", layoutPath);
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

        public static Action<HttpContext> Send<T>(Func<T> dataLoader)
        {
            return ctx => ctx.Send(dataLoader());
        }

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