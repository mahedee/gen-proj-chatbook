﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FacebookClone
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Default", "", new {controller = "Account", action = "Index"});
            routes.MapRoute("CreateAccount", "Account/CreateAccount", new {controller = "Account", action = "CreateAccount"});
            routes.MapRoute("Account", "{username}", new {controller = "Account", action = "Username"});
            routes.MapRoute("Logout", "Account/Logout", new {controller = "Account", action = "Logout"});
            routes.MapRoute("LoginPartial", "Account/LoginPartial", new {controller = "Account", action = "LoginPartial"});
            routes.MapRoute("Login", "Account/Login", new {controller = "Account", action = "Login"});
            routes.MapRoute("Profile", "Profile/{action}/{id}", new {controller = "Profile", action = "Index", id = UrlParameter.Optional});
            routes.MapRoute("FriendList", "Friendlist/{username}",
                new {controller = "Account", action = "ShowFriends"});
            //routes.MapRoute("Profile", "Profile/LiveSearch", new {controller = "Profile", action = "LiveSearch"});


            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
