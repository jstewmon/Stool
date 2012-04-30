using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Stool;

namespace Stool.Example
{
    public class MyApp : StoolApp
    {
        public MyApp()
        {
            Use(Middleware.RouteDataContextItems);
            CascadeLayouts = true;
            TemplateDirectory = "~/templates";
            Get("", Render("home.vm", GetHomeData));
            Get("plain", Render("plain.vm"));
            On(new[] { "GET", "POST" }, "foo/bar", FooBar);
            Get("home/null", Render<Customer>("home.vm", () => null));
            Get("sub/home", Render<Customer>("sub/home.vm", () => null));
            Get("customer", Send(GetHomeData));
            Get("customer/{id}", ctx => ctx.Send(GetCustomer(int.Parse(ctx.Request.RequestContext.RouteData.Values["id"].ToString()))));
            Get("error/json", ctx => ctx.Send(new {message = "OH NO!!!!"}, 500));
            Get("error/json/{code}", ctx => ctx.Send(new {message = "OH NO!!!!"}, int.Parse(ctx.Request.RequestContext.RouteData.Values["code"].ToString())));
            Get("error/default", ctx => { throw new NotImplementedException(); });
            Get("error/custom/{*err}", ctx => { throw new NotImplementedException(); })
                .Use((ctx, next) =>
                         {
                             ctx.Items.Add("foo", "bar");
                             next();
                         })
                .Use((ctx, next) =>
                         {
                             var err = ctx.Request.RequestContext.RouteData.Values["err"];
                             if(err == null)
                             {
                                 ctx.Response.Clear();
                                 ctx.Response.StatusCode = 200;
                                 ctx.Response.Write(ctx.Items["foo"]);
                                 return;
                             }
                             if(err.ToString() == "bar")
                             {
                                 throw new InvalidOperationException();
                             }
                             next();
                         })
                .OnException((ctx, ex) =>
                                 {
                                     ctx.Response.Clear();
                                     ctx.Response.StatusCode = 500;
                                     ctx.Response.Write("Oh NO!  An error occurred!" + ex);
                                 });
            //Get("customers/{howmany}").Process((ctx, next) => ctx.Send(GetCustomers(Convert.ToInt32(ctx.Items["howmany"]))));
            Get("customers/{howmany}/{pagesize}/{page}")
                .RouteDefault("pagesize", 10)
                .RouteDefault("page", 1)
                .Process((ctx, next) =>
                             {
                                 ctx.Items.Add("data", GetCustomers(Convert.ToInt32(ctx.Items["howmany"])));
                                 next();
                             })
                .After(Middleware.PageData<Customer>);
            Post("json/post")
                .Use(Middleware.BodyToExpando)
                .Process((ctx, next) =>
                             {
                                 dynamic input = ctx.Items["body"] as ExpandoObject;
                                 if(input == null) throw new Exception("the body was not an ExpandoObject");
                                 ctx.Send((object)input);
                             });
        }

        public class Customer
        {
            public string name { get; set; }
            public decimal salary { get; set; }
        }

        private readonly Random rand = new Random();
        public Customer GetCustomer(int id)
        {
            return new Customer
                       {
                           name = "id " + id,
                           salary = rand.Next(50, 100)
                       };
        }

        public IEnumerable<Customer> GetCustomers(int howMany)
        {
            var customers = new List<Customer>(howMany);
            for(var i = 0; i < howMany; i++)
            {
                customers.Add(GetCustomer(i));
            }
            return customers;
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