﻿using System;
using System.Web;
using Stool;

namespace Stool.Example
{
    public class MyApp : StoolApp
    {
        public MyApp()
        {
            CascadeLayouts = true;
            TemplateDirectory = "~/templates";
            Get("", Render("home.vm", GetHomeData));
            Get("plain", Render("plain.vm"));
            On(new[] { "GET", "POST" }, "foo/bar", FooBar);
            Get("home/null", Render<Customer>("home.vm", () => null));
            Get("sub/home", Render<Customer>("sub/home.vm", () => null));
            Get("customer", Send(GetHomeData));
            Get("customer/{id}", ctx => ctx.Send(GetCustomer(int.Parse(ctx.Request.RequestContext.RouteData.Values["id"].ToString()))));
            Get("error/default", ctx => { throw new NotImplementedException(); });
            Get("error/custom/{*err}", ctx => { throw new NotImplementedException(); })
                .Use((ctx, cts) => ctx.Items.Add("foo", "bar"))
                .Use((ctx, cts) =>
                         {
                             var err = ctx.Request.RequestContext.RouteData.Values["err"];
                             if(err == null)
                             {
                                 ctx.Response.Clear();
                                 ctx.Response.StatusCode = 200;
                                 ctx.Response.Write(ctx.Items["foo"]);
                                 cts.Cancel();
                             }
                             else if(err.ToString() == "bar")
                             {
                                 throw new InvalidOperationException();
                             }
                         })
                .OnException((ctx, ex) =>
                                 {
                                     ctx.Response.Clear();
                                     ctx.Response.StatusCode = 500;
                                     ctx.Response.Write("Oh NO!  An error occurred!");
                                 });
        }

        public class Customer
        {
            public string name { get; set; }
            public decimal salary { get; set; }
        }

        public Customer GetCustomer(int id)
        {
            return new Customer
                       {
                           name = "id " + id,
                           salary = new Random().Next(50, 100)
                       };
        }

        public Customer GetHomeData()
        {
            return new Customer
                       {
                           name = "Stew",
                           salary = 500
                       };
        }

        public void FooBar(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World!");
            context.Response.End();
        }
    }
}