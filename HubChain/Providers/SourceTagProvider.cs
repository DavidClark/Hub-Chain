using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SiGyl.HubChain
{
	public class SourceTagProvider : IDataProvider<int>
	{
		static Random _Random = new Random();
		public IObservable<Item<int>> GetObservable(Item<int> item, Action<IDisposable> onDispose)
		{
			return Observable.Create<Item<int>>(
					o =>
					{
						var subscription = Observable.Zip<int>(Observable.Interval(TimeSpan.FromMilliseconds(100 + _Random.Next(200))).Select(i => 1), Observable.Range(0, 70000)).Select((t, v) => v).Concat<int>(Observable.Throw<int>(new Exception("OMFG!")))
						.Subscribe((x) => o.OnNext(new Item<int> { Id = item.Id, Index = Indexer.Index, Value = x }), (ex) => o.OnError(ex), () => o.OnCompleted());
						return () =>
						{
							onDispose(subscription);

						};

					}
				).Publish().RefCount();
		}



		public Task<IEnumerable<Item<int>>> Subscribe(IEnumerable<Item<int>> groups)
		{
			return Task.FromResult((IEnumerable<Item<int>>)new List<Item<int>>());
		}
	}

}