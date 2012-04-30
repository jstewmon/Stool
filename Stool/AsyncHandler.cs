using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using log4net;

namespace Stool
{
    public class AsyncHandler : IHttpAsyncHandler
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (AsyncHandler));

        /// <summary>
        /// The route for which the handler was created.
        /// </summary>
        public Route Route { get; internal set; }

        public AsyncHandler RouteDefault(string key, object value)
        {
            if(Route.Defaults == null)
                Route.Defaults = new RouteValueDictionary();
            Route.Defaults.Add(key, value);
            return this;
        }

        private Action<HttpContext, Action> _process;
        /// <summary>
        /// Creates a handler that supports afteware by calling the action passed to the handler
        /// </summary>
        /// <param name="process"></param>
        public AsyncHandler(Action<HttpContext, Action> process)
        {
            _process = process;
        }

        /// <summary>
        /// Creates a handler that will run the process without supporting afterware
        /// </summary>
        /// <param name="processor"></param>
        public AsyncHandler(Action<HttpContext> processor)
        {
            _process = (ctx, a) => processor(ctx);
        }

        /// <summary>
        /// Requires <see cref="Process"/> to be called to specify the method to handle the request.
        /// </summary>
        public AsyncHandler(){}

        public AsyncHandler OnException(Action<HttpContext, Exception> exceptionHandler)
        {
            ExceptionHandler = exceptionHandler;
            return this;
        }
        /// <summary>
        /// Called before any <see cref="Use"/>
        /// </summary>
        /// <param name="beforeall"></param>
        /// <returns></returns>
        public AsyncHandler BeforeAll(Action<HttpContext, Action> beforeall)
        {
            var current = _beforeAll;
            _beforeAll = (ctx, n) => current(ctx, () => beforeall(ctx, n));
            return this;
        }
        private Action<HttpContext, Action> _beforeAll = (ctx, a) => a();

        /// <summary>
        /// Called after any <see cref="BeforeAll"/>, before any <see cref="Process"/>
        /// </summary>
        /// <param name="use"></param>
        /// <returns></returns>
        public AsyncHandler Use(Action<HttpContext, Action> use)
        {
            var current = _use;
            _use = (ctx, n) => current(ctx, () => use(ctx, n));
            return this;
        }
        private Action<HttpContext, Action> _use = (ctx, a) => a();

        /// <summary>
        /// Called after any <see cref="Before"/>, before any <see cref="After"/>
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public AsyncHandler Process(Action<HttpContext, Action> process)
        {
            if (_process != null)
            {
                _log.Warn("Process called after the process method was already specified");
            }
            _process = process;
            return this;
        }

        /// <summary>
        /// Called after any <see cref="Use"/>, before <see cref="Process"/>
        /// </summary>
        /// <param name="before"></param>
        /// <returns></returns>
        public AsyncHandler Before(Action<HttpContext, Action> before)
        {
            var current = _before;
            _before = (ctx, n) => current(ctx, () => before(ctx, n));
            return this;
        }
        private Action<HttpContext, Action> _before = (ctx, a) => a();

        /// <summary>
        /// Called after <see cref="Process"/>, before any <see cref="AfterAll"/>
        /// </summary>
        /// <param name="afterware"></param>
        /// <returns></returns>
        public AsyncHandler After(Action<HttpContext, Action> afterware)
        {
            var current = _after;
            _after = (ctx, n) => current(ctx, () => afterware(ctx, n));
            return this;
        }
        private Action<HttpContext, Action> _after = (ctx, a) => a();

        /// <summary>
        /// Called after any <see cref="After"/>
        /// </summary>
        /// <param name="afterall"></param>
        /// <returns></returns>
        public AsyncHandler AfterAll(Action<HttpContext, Action> afterall)
        {
            var current = _afterAll;
            _afterAll = (ctx, n) => current(ctx, () => afterall(ctx, n));
            return this;
        }
        private Action<HttpContext, Action> _afterAll = (ctx, a) => a();

        private Action<HttpContext, Exception> _errorHandler;
        /// <summary>
        /// Called if an unhandled exception is encountered while handling the request
        /// </summary>
        public Action<HttpContext, Exception> ExceptionHandler
        {
            get { return _errorHandler ?? HandleError; }
            set { _errorHandler = value; }
        }

        private void HandleError(HttpContext context, Exception exception)
        {
            context.Response.Clear();
            context.Response.StatusCode = 500;
            context.Response.Write(exception);
        }

        public void ProcessRequest(HttpContext context)
        {
            _log.Debug("Enter ProcessRequest " + context.Request.RawUrl);
            Execute(context);
            _log.Debug("Exit ProcessRequest " + context.Request.RawUrl);
        }

        public bool IsReusable
        {
            get { return true; }
        }

        private void Execute(HttpContext context)
        {
            _log.Debug("Enter Execute " + context.Request.RawUrl);
            _beforeAll(context,
                       () => _use(context,
                       () => _before(context,
                       () => _process(context,
                       () => _after(context,
                       () => _afterAll(context, () => { }))))));
            _log.Debug("Exit Execute " + context.Request.RawUrl);
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            _log.Debug("Enter BeginProcessRequest " + context.Request.RawUrl);
            var task = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            Execute(context);
                        }
                        catch (Exception ex)
                        {
                            _log.Warn("Caught exception while processing request:");
                            _log.Warn(ex);
                            try
                            {
                                ExceptionHandler(context, ex);
                            }
                            catch(Exception ehex)
                            {
                                _log.Warn("Caught exception while calling ExceptionHandler:");
                                _log.Warn(ehex);
                                try
                                {
                                    HandleError(context, ex); 
                                }
                                catch(Exception heex)
                                {
                                    _log.Error("Caught exception while callign HandleError:");
                                    _log.Error(heex);
                                    throw;
                                }
                            }
                        }
                    }).ContinueWith(t => cb(t));
            _log.Debug("Exit BeginProcessRequest " + context.Request.RawUrl);
            return task;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            _log.Debug("Enter EndProcessRequest");
            if (result != null)
                ((Task)result).Dispose();
            _log.Debug("Exit EndProcessRequest");
        }
    }
}