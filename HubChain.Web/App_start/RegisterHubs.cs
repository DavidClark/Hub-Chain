

using System.Web.Routing;

using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;
//using Microsoft.Owin.Cors;
using System.Web.Configuration;
using newhubs.App_start;
using Microsoft.Owin.Cors;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using SiGyl.HubChain;
using newhubs.code;
[assembly: OwinStartup(typeof(Startup))]

namespace newhubs.App_start
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{

			if (ConfigurationManager.AppSettings["chainHub"] != null)
			{
				ChainTagProvider<int>.HubConnection = () => new HubConnection(ConfigurationManager.AppSettings["chainHub"]);
				Tags<int>.TagProvider = new ChainTagProvider<int>();
			}
			else
				Tags<int>.TagProvider = new SourceTagProvider();

			if (ConfigurationManager.AppSettings["redis"] == null || ConfigurationManager.AppSettings["chainHub"] == null)
			{
				Tags<int>.Updater = (group, item) =>
					Task.Run(async () =>
						await GlobalHost.ConnectionManager.GetHubContext<MyHub>().Clients.Group(group).Update(item)
					);
			}
			app.Map("/signalr", map =>
			{
				var redis  = WebConfigurationManager.AppSettings["redis"];
				if(redis!=null)
					GlobalHost.DependencyResolver.UseRedis(redis.Split(':')[0], int.Parse(redis.Split(':')[1]), "", "BatchScada");
				

			   // GlobalHost.DependencyResolver.UseRedis(redis.Split(':')[0], int.Parse(redis.Split(':')[1]), "", "BatchScada");
				// Setup the CORS middleware to run before SignalR.
				// By default this will allow all origins. You can 
				// configure the set of origins and/or http verbs by
				// providing a cors options with a different policy.
				map.UseCors(CorsOptions.AllowAll);
				var hubConfiguration = new HubConfiguration
				{
					// You can enable JSONP by uncommenting line below.
					// JSONP requests are insecure but some older browsers (and some
					// versions of IE) require JSONP to work cross domain
					// EnableJSONP = true
				};
				// Run the SignalR pipeline. We're not using MapSignalR
				// since this branch already runs under the "/signalr"
				// path.
				map.RunSignalR(hubConfiguration);
});

		   
	//		app.MapSignalR();

			// Register the default hubs route: ~/signalr/hubs
		   // RouteTable.Routes.MapHubs();
		}
	}
}
