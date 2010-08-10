using System;
using System.Web;
using Common.Logging;
using Triton.NHibernate.Model;

namespace Triton.NHibernate.Web.Support
{
	public class NHibernateHttpModule : IHttpModule
	{
		#region IHttpModule Members

		public void Dispose() {}


		public void Init(HttpApplication context)
		{
			context.PostRequestHandlerExecute += this.RequestEnd;
		}

		#endregion

		private void RequestEnd(
			object sender,
			EventArgs e)
		{
			NHibernateSessionProvider.Instance.DestroySession();
		}
	}
}