﻿using System;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using VideoGameHash.Controllers;
using VideoGameHash.Decorators;
using VideoGameHash.Handlers;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            // Register db container
            container.Register(() => new VGHDatabaseContainer(), Lifestyle.Scoped);

            // Register Repositories
            container.Register<IUserRepository, UserRepository>(Lifestyle.Scoped);
            container.Register<IInfoRepository, InfoRepository>(Lifestyle.Scoped);
            container.Register<IErrorRepository, ErrorRepository>(Lifestyle.Scoped);

            // Register command handlers
            // Go look in all assemblies and register all implementations
            // of ICommandHandler<T> by their closed interface:
            container.Register(typeof(ICommandHandler<>),
                AppDomain.CurrentDomain.GetAssemblies(), Lifestyle.Scoped);

            // Decorate each returned ICommandHandler<T> object with
            // a TransactionCommandHandlerDecorator<T>.
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(TransactionCommandHandlerDecorator<>), Lifestyle.Scoped);

            // Register query handlers
            container.Register<IQueryProcessor, QueryProcessor>(Lifestyle.Scoped);
            container.Register(typeof(IQueryHandler<,>), new[] {typeof(IQueryHandler<,>).Assembly}, Lifestyle.Scoped);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            #if DEBUG
            container.Verify();
            #endif
            
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var httpContext = Context;
            var currentController = " ";
            var currentAction = " ";
            var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
 
            if (currentRouteData != null)
            {
                if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                {
                    currentController = currentRouteData.Values["controller"].ToString();
                }
 
                if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                {
                    currentAction = currentRouteData.Values["action"].ToString();
                }
            }
 
            var ex = Server.GetLastError();

            var container = new Container();
            var repository = container.GetInstance<ErrorRepository>();

            repository.AddError($"{ex.Message} - {ex.StackTrace}");

            var controller = new ErrorController();
            var routeData = new RouteData();
            const string action = "Index";
 
            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException exception ? exception.GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;
     
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = action;
 
            controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }
    }
}