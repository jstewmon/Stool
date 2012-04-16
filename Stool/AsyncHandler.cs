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

        private Action<HttpContext, Exception> _errorHandler;
        public Action<HttpContext, Exception> ExceptionHandler
        {
            get { return _errorHandler ?? HandleError; }
            set { _errorHandler = value; }
        }

        public AsyncHandler OnException(Action<HttpContext, Exception> exceptionHandler)
        {
            ExceptionHandler = exceptionHandler;
            return this;
        }

        private void HandleError(HttpContext context, Exception exception)
        {
            context.Response.Clear();
            context.Response.StatusCode = 500;
            context.Response.Write(exception);
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
            return Task.Factory.StartNew(() => _processor(context))
                .ContinueWith(task => ExceptionHandler(context, task.Exception), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(task => cb(task), TaskContinuationOptions.NotOnFaulted);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            if (result == null)
                return;
            ((Task)result).Dispose();
        }
    }
}