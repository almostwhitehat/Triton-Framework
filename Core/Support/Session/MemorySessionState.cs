using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Triton.Support.Session
{
	public class MemorySessionState : ISessionState 
	{
		private static readonly IDictionary<string, object> session = new Dictionary<string, object>(); 
		
		private static readonly object syncRoot = new object();

		#region ICollection Members

		public void CopyTo(Array array, int index)
		{
			//session.CopyTo(array, index);
		}

		public int Count
		{
			get { return session.Count; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return syncRoot; }
		}

		#endregion

		#region IEnumerable Members

		public System.Collections.IEnumerator GetEnumerator()
		{
			return session.GetEnumerator();
		}

		#endregion

		public object this[string key]
		{
			get
			{
				if (session.ContainsKey(key))
					return session[key];
				else

					return null;

			}
			set { session[key] = value; }
		}
	}
}
