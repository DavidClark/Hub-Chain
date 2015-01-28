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
		IHubProxy Proxy;
		IDisposable PermanentSubscription;
		IObservable<Item<T>> Connector;
		public ChainTagProvider()
		{
			Connector = Observable.Create<Item<T>>(async o =>
			{

				var started = false;
				Func<Task> start = null;
				Proxy = HubConnection.CreateHubProxy(HubName);
				Proxy.On("dummy", () => { });
				Proxy.On<Item<T>>("Update", i =>
				{
					o.OnNext(i);
				}
				);
				HubConnection.StateChanged += (st) =>
				{
					lock (HubConnection)
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
								await HubConnection.Start();
								System.Diagnostics.Debug.WriteLine("Started hub connection");
								started = true;
							}
							catch
							{
								System.Diagnostics.Debug.WriteLine("Hub conection start failed");
								HubConnection.Stop();
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
			Initialiser = new Lazy<bool>(() =>
			{
				if (PermanentSubscription == null)
					PermanentSubscription = Connector.Subscribe();
				return true;
			});
		}
		Lazy<bool> Initialiser;
		public string HubName { get; set; }
		public string JoinMethodName { get; set; }
		public string LeaveMethodName { get; set; }
		public HubConnection HubConnection { get; set; }
		public IObservable<Item<T>> GetObservable(Item<T> item, Action<IDisposable> onDispose)
		{


			return Observable.Create<Item<T>>(o =>
			{
				if (Initialiser.Value)
				{
					//o.OnNext(-4);
					var subscription = Connector.Where(x =>
					{
						return
							x.Id == item.Id;
					}).Subscribe(x => o.OnNext(x));


					return () =>
					{
						o.OnCompleted();
						var t = Task.Run(async () =>
							await Proxy.Invoke<IEnumerable<T>>("UnsubscribeItem", new List<Item<T>> { item }));
						onDispose(subscription);

					};
				}
				return () => { };
			}
				).Publish().RefCount();
		}



		public async Task<IEnumerable<Item<T>>> Subscribe(IEnumerable<Item<T>> items)
		{
			return await Proxy.Invoke<IEnumerable<Item<T>>>("SubscribeItem", items);
		}
	}


}