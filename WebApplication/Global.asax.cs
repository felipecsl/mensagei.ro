using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication {
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication {
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("Content/{*pathInfo}");
			routes.IgnoreRoute("Scripts/{*pathInfo}");

			routes.MapRoute(
			  "HostStart",
			  "Room.aspx/HostStart/{id}/{roomName}",
			  new { controller = "Room", action = "HostStart", id = "", email = "" }
			);
			
			routes.MapRoute(
			  "EventsRoute",
			  "Room.aspx/Events/{id}/{email}",
			  new { controller = "Room", action = "Events", id = "", email = "" }
			);

			routes.MapRoute(
			  "Default",
			  "{controller}.aspx/{action}/{id}",
			  new { controller = "Room", action = "Index", id = UrlParameter.Optional }
			);
		}

		protected void Application_Start() {
			RegisterRoutes(RouteTable.Routes);
		}
	}
}