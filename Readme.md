This readme is a work in progress.

##Stool##
Stool is a micro framework for .net, written in C#. It aims to make creating RESTful applications easier by allowing developers to define routes and functions to handle those routes. This idea is not novel, and there are many similar frameworks for other languages.

Stool is not a standalone web server - it is meant to be embedded in web applications and deployed to a web server, typically IIS. Stool implements as little custom functionality as possible. Routing is supplied by System.Web.Routing, and request handling is provided by implementing a custom System.Web.IHttpAsyncHandler.

Although Stool is not supported by unit tests, an example application is available to demonstrate how most features work.  I may include some unit tests in the future if issues are reported that prove unit tests are valuable for this project.

###Logging###
Stool uses log4net, so if you want logging for debugging purposes, you can [configure](http://logging.apache.org/log4net/release/manual/configuration.html) log4net as you wish. Many people configure log4net using xml, but fail to actally configure log4net before loggings calls start being sent to log4net, which means you get no log output, which can be misleading. I recommend using [WebActivator](http://nuget.org/packages/WebActivator) or the [PreApplicationStartMethod](http://msdn.microsoft.com/en-us/library/system.web.preapplicationstartmethodattribute.preapplicationstartmethodattribute.aspx) assembly attributes to call `XmlConfigurator.Configure()` to ensure that log4net is configured before calls are issued to it.

###Initialization###
Routes can be configured at any time, but you probably want them to get defined when you application starts, so that they're actually defined when your application starts receiving requests.  In the example application, routes are setup in a constructor, which I chose arbitrarily. Wherever you choose to define your routes, I recommend using one of the assembly attributes mentioned in the logging section to make sure your code is executed before requests are sent to the application.

You do need to define your routes in a class that derives from StoolApp because Get, Post, Use and On are all instance methods. The only real reason for them to be instance methods is so that middleware can be applied to all the routes in a given app without being applied to routes from another app. For example, if some of your routes require authentication, you may want to put those routes in an app that uses middleware to ensure a request is authenticated before it is processed.

###Examples####
These snippets are taken directly from the Example project. The lines are assumed to be placed in a method of a class which inherits from StoolApp.

Return data as json
```c#
Get("customer", Send(new { Name = "Bob", Title = "Developer"}));
```