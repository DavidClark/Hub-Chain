
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


namespace SiGyl.HubChain
{
	public static class DataSource<T>
	{
		public static IDataProvider<T> DataProvider = null;
		public static Action<string, Item<T>> Updater = null;
		public static ConcurrentDictionary<string, IObservable<Item<T>>> observables = new ConcurrentDictionary<string, IObservable<Item<T>>>();
		static Random _Random = new Random();
		static ConcurrentDictionary<string, IObservable<Item<T>>> _tags = new ConcurrentDictionary<string, IObservable<Item<T>>>();
		public static IObservable<Item<T>> GetObservable(Item<T> item)
		{
			var toRet = DataSource<T>.observables.GetOrAdd(item.Id, (g) =>
			{
				return create(item)
				.Do((d) =>
				{
					values.AddOrUpdate(item.Id, d, (gg, od) => d);
					if (DataSource<T>.Updater != null)
						DataSource<T>.Updater(item.Id, d);
				})
				.Publish().RefCount();
			}
			);
			Item<T> existing = null;
			if (values.TryGetValue(item.Id, out existing))
				return Observable.Return(existing).Concat(toRet);
			else
				return toRet;
		}

		static ConcurrentDictionary<string, Dictionary<string, IDisposable>> subscriptions = new ConcurrentDictionary<string, Dictionary<string, IDisposable>>();

		static ConcurrentDictionary<string, Item<T>> values = new ConcurrentDictionary<string, Item<T>>();
	
		static IObservable<Item<T>> create(Item<T> item)
		{
			return Observable.Create<Item<T>>(o =>
			{

				var tt = Observable.Return(new Item<T> { Id = item.Id, Index = -998 })
					.Concat(
						_tags.GetOrAdd(item.Id, DataProvider.GetObservable(
							item, (d) =>
							{
								IObservable<Item<T>> removed;
								if (_tags.TryRemove(item.Id, out removed))
									d.Dispose();

							}
						)
					)
				)
				.Catch((Exception ex) => Observable.Return(new Item<T> { Id = item.Id, Index = Indexer.Index, Exception= ex.Message }))
				.Subscribe((l) =>
				{
					o.OnNext(l);
				});




				return Disposable.Create(() =>
				{
					IObservable<Item<T>> oo;
					if (DataSource<T>.observables.TryRemove(item.Id, out oo))
						tt.Dispose();

				});
			});


		}
		public async static Task<IEnumerable<Item<T>>> Join(IEnumerable<Item<T>> items, string connectionId, Func<string, Task> joiner)
		{
			//return new List<int>();
			//Connector.Subscribe();
			//List<string> addedItems = new List<string>();

			var toRet = await Task.WhenAll(items.Select(async item =>
			{
				await joiner(item.Id);
				IObservable<Item<T>> obs = null;

				obs = GetObservable(item);

				var subscription = subscriptions.GetOrAdd(connectionId, (g) => new Dictionary<string, IDisposable>());
				IDisposable sub;
				if (!subscription.TryGetValue(item.Id, out sub))
					subscription[item.Id] = obs.Subscribe();
				return obs;
			}));

			try
			{
				foreach (var item in await DataSource<T>.DataProvider.Subscribe(items))
				{
					values.AddOrUpdate(item.Id, item, (gg, od) => item);
				}
			}
			catch { }

			return items.Select(g => values[g.Id]);
		}



		public async static Task Leave(IEnumerable<Item<T>> items, string connectionId, Func<string, Task> leaver)
		{
			await Task.WhenAll(items.Select(async item =>
			{
				await leaver(item.Id);
				Dictionary<string, IDisposable> subscription = null;
				if (subscriptions.TryGetValue(connectionId, out subscription))
				{
					if (subscription.ContainsKey(item.Id))
					{
						subscription[item.Id].Dispose();
						subscription.Remove(item.Id);

					}
					//int i;
					//values.TryRemove(group, out i);
				}
			})
			);

		}


		public static void OnDisconnected(bool stopCalled, string connectionId)
		{
			Dictionary<string, IDisposable> subscription = null;
			if (subscriptions.TryRemove(connectionId, out subscription))
			{
				foreach (var v in subscription.Values)
					v.Dispose();
			}

		}
	}

}