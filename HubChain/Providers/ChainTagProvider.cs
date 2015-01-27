using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SiGyl.HubChain
{
	public class ChainTagProvider<T> : ITagProvider<T>
	{
		public static Func< HubConnection> HubConnection;
		static IHubProxy Proxy;
		static IDisposable PermanentSubscription;
		static IObservable<Item<T>> Connector = Observable.Create<Item<T>>(async o =>
		{

			var started = false;
			Func<Task> start = null;
			HubConnection hubConnection = HubConnection();
			Proxy = hubConnection.CreateHubProxy("NewHub");
			Proxy.On("dummy", () => { });
			Proxy.On<Item<T>>("Update", i =>
			{
				o.OnNext(i);
			}
			);
			hubConnection.StateChanged += (st) =>
			{
				lock (hubConnection)
				{
					if (st.NewState == ConnectionState.Disconnected)
					{
						if (started)
						{
							started = false;
							var t = Task.Run(async () => await Task.Delay(1000).ContinueWith(async tt => await start()));
						}
					}
					if (st.NewState == ConnectionState.Connected)
					{
						started = true;
						var t = Task.Run(async () => await Task.Delay(1000).ContinueWith(async ttt => await Proxy.Invoke<IEnumerable<int>>("Join", Tags<int>.observables.Keys)));
					}
				}

			};

			start =
				async () =>
				{
					int delay = 1000;
					while (!started)
					{
						try
						{
							System.Diagnostics.Debug.WriteLine("Starting hub connection");
							await hubConnection.Start();
							started = true;
						}
						catch
						{
							System.Diagnostics.Debug.WriteLine("Hub conection start failed");
							hubConnection.Stop();
						}

						if (!started)
						{
							if (delay < 5000)
								delay += 1000;
							await Task.Delay(5000);
						}
					}
				};

			await start();
			return () => { };
		}).Publish().RefCount();

		static Lazy<bool> LazyInit = new Lazy<bool>(() =>
		{
			
				if (PermanentSubscription == null)
					PermanentSubscription = Connector.Subscribe();

			return true;

		});


		public IObservable<Item<T>> GetObservable(Item<T> item, Action<IDisposable> onDispose)
		{
			var vv = LazyInit.Value;

			return Observable.Create<Item<T>>(o =>
			{
				//o.OnNext(-4);
				var subscription = Connector.Where(x => x.Id == item.Id).Subscribe(x => o.OnNext(x));

				return () =>
				{
					o.OnCompleted();
					var t = Task.Run(async () =>
						await Proxy.Invoke<IEnumerable<T>>("Leave", new List<Item<T>> { item }));
					onDispose(subscription);

				};
			}
				).Publish().RefCount();
		}



		public async Task<IEnumerable<Item<T>>> Subscribe(IEnumerable<Item<T>> items)
		{
			return await Proxy.Invoke<IEnumerable<Item<T>>>("SubscribeItem", items);
		}
	}


}