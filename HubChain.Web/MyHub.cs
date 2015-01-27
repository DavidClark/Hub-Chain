using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Hubs;
using SiGyl.HubChain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace newhubs.code
{

	[HubName("NewHub")]
	public class MyHub : Hub
	{
		public MyHub()
		{

		}


		public async Task<IEnumerable<Item<int>>> SubscribeItem(IEnumerable<Item<int>> items)
		{
			var ret = await Tags<int>.Join(items, Context.ConnectionId, async (group) => await Groups.Add(Context.ConnectionId, group));
			return ret;
		}



		public async Task UnsubscribeItem(IEnumerable<Item<int>> items)
		{
			await Tags<int>.Leave(items, Context.ConnectionId, async (group) => await Groups.Remove(Context.ConnectionId, group));
		}


		public override Task OnDisconnected(bool stopCalled)
		{
			Tags<int>.OnDisconnected(stopCalled, Context.ConnectionId);
			return base.OnDisconnected(stopCalled);
		}


	}
}