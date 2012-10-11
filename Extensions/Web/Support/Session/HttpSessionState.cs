using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Triton.Support.Session;

namespace Triton.Web.Support.Session
{
	public class HttpSessionState : ISessionState
	{
		private static System.Web.SessionState.HttpSessionState sessionState = HttpContext.Current.Session;

		#region ICollection Members

		public void CopyTo(Array array, int index)
		{
			HttpContext.Current.Session.CopyTo(array, index);
		}

		public int Count
		{
			get { return HttpContext.Current.Session.Count; }
		}

		public bool IsSynchronized
		{
			get { return HttpContext.Current.Session.IsSynchronized; }
		}

		public object SyncRoot
		{
			get { return HttpContext.Current.Session.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		public System.Collections.IEnumerator GetEnumerator()
		{
			return HttpContext.Current.Session.GetEnumerator();
		}

		#endregion

		#region ISessionState Members

		public object this[string key]
		{
			get { return HttpContext.Current.Session[key]; }
			set { HttpContext.Current.Session[key] = value; }
		}

		#endregion
	}
}
