using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SiGyl.HubChain
{
	public class Item<T>
	{
		public string Id { get; set; }
		public long Index { get; set; }
		public T Value { get; set; }
		public string Exception { get; set; }
	}

	public static class Indexer
	{
		static long _index = 0;
		static object locker = new object();
		static public long Index
		{
			get
			{
				lock (locker)
				{
					return ++_index;
				}
			}
		}
	}

}