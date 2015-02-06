using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SiGyl.HubChain
{
	public interface IDataProvider<T>
	{
		IObservable<Item<T>> GetObservable(Item<T> item, Action<IDisposable> onDispose);
		Task<IEnumerable<Item<T>>> Subscribe(IEnumerable<Item<T>> groups);

        IObservable<IEnumerable<Item<T>>> Get(IEnumerable<Item<T>> items);
        IObservable<IEnumerable<Item<T>>> Put(IEnumerable<Item<T>> items);

	}
}