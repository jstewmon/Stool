using System;
using System.Web;
using System.Web.Routing;

namespace Stool
{
    public class RouteHandler : IRouteHandler
    {
        private readonly IHttpHandler _handler;

        public RouteHandler(IHttpHandler handler)
        {
            _handler = handler;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return _handler;
        }
    }
}