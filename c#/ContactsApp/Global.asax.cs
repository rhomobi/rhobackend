using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections;

using RhoconnectNET;
using RhoconnectNET.Controllers;

namespace ContactsApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Contact", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "DeailJson", // Route name
                "{controller}/{id}/{action}", // URL with parameters
                new { controller = "Contact", id = UrlParameter.Optional, action = "Detail" } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            init_rhoconnect();
        }

        // implement init_rhoconnect() method to establish
        // communication link between `Rhoconnect` server
        // and ASP.NET MVC application
        private void init_rhoconnect()
        {
            // this call allows parsing JSON structures into Objects
            ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());

            // this call establishes communication between Rhoconnect and ASP.NET application
            // as a last parameter we supply authentication routine that will called 
            // by rhoconnect server to authenticate users.
            RhoconnectNET.Client.set_app_endpoint("http://localhost:9292", //"my_rhoconnect_server_url", ///application/
                                                 "http://localhost:8081",// "my_mvc_app_root_url", ///Contact
                                                 "rhoconnect_api_token", // "rhoconnect_api_token",
                                                  rhoconnect_authenticate);
        }

        private bool rhoconnect_authenticate(ref String username, String password, Hashtable auth_attrs)
        {
            // uncomment the following line, if you want to replace the default partitioning to 'app'
            // username = "app";
            // perform your authentication here
            return true;
        }
    }
}