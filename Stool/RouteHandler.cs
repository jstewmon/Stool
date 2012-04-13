using System;
using System.Web;
using System.Web.Routing;

namespace Stool
{
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
}