using System;
using System.Threading.Tasks;
using System.Web;
using log4net;

namespace Stool
{
    public class AsyncHandler : IHttpAsyncHandler
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (AsyncHandler));

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

        private Action<HttpContext, Action> _middleWare;
        public AsyncHandler Use(Action<HttpContext, Action> middleWare)
        {
            var current = (_middleWare ?? ((ctx, a) => a()));
            _middleWare = (ctx, n) => current(ctx, () => middleWare(ctx, n));
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
            _log.Debug("Finshed processing request!");
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return Task.Factory.StartNew(() =>
                                             {
                                                 try
                                                 {
                                                     (_middleWare ?? ((ctx, a) => a()))(context, () => _processor(context));
                                                     _log.Debug("Finshed processing request!");
                                                 }
                                                 catch (Exception ex)
                                                 {
                                                     ExceptionHandler(context, ex);
                                                 }
                                             }).ContinueWith(t => cb(t));
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            if (result == null)
                return;
            ((Task)result).Dispose();
        }
    }
}