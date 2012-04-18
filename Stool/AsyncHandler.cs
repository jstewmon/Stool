using System;
using System.Collections.Generic;
using System.Threading;
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

        /// <summary>
        /// Provide a function to be run as middleware during the processing of a request.
        /// Middleware are run in the order in which they were added.
        /// Calling cancel on the <see cref="CancellationTokenSource"/> will hault the processing of additional middleware, the request handler and the exception handler.
        /// </summary>
        /// <param name="middleWare"></param>
        /// <returns></returns>
        public AsyncHandler Use(Action<HttpContext, CancellationTokenSource> middleWare)
        {
            _middleWare.Add(middleWare);
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

        private readonly List<Action<HttpContext, CancellationTokenSource>> _middleWare = new List<Action<HttpContext, CancellationTokenSource>>(); 

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            Task task = null;
            var cts = new CancellationTokenSource();
            foreach(var mw in _middleWare)
            {
                var currentMw = mw;
                var current = mw;
                if (task == null)
                {
                    task = Task.Factory.StartNew(() => current(context, cts), cts.Token);
                }
                else
                {
                    task = task.ContinueWith(t => currentMw(context, cts), cts.Token,
                                          TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
                }
                task.ContinueWith(t => ExceptionHandler(context, t.Exception), cts.Token, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
            }
            
            task = (task == null)
                       ? Task.Factory.StartNew(() => _processor(context))
                       : task.ContinueWith(t => _processor(context), cts.Token,
                                          TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            
            task.ContinueWith(t => ExceptionHandler(context, t.Exception), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(t => cb(t), TaskContinuationOptions.NotOnFaulted);
            return task;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            if (result == null)
                return;
            ((Task)result).Dispose();
        }
    }
}