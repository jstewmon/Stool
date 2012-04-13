using System;
using System.Threading.Tasks;
using System.Web;

namespace Stool
{
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
}