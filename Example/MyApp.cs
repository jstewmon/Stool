using System;
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