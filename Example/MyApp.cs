using System;
using System.Web;

namespace Stool.Example
{
    public class MyApp : StoolApp
    {
        public MyApp()
        {
            CascadeLayouts = true;
            Get("", Render("home.vm", GetHomeData));
            On(new[] { "GET", "POST" }, "foo/bar", FooBar);
            Get("home/null", Render<Customer>("home.vm", () => null));
            Get("sub/home", Render<Customer>("sub/home.vm", () => null));
        }

        public class Customer
        {
            public string name { get; set; }
            public decimal salary { get; set; }
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