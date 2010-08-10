using System;
using System.Web;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Logic.Support;
using Triton.Membership.Support;

namespace Triton.Membership.Logic
{
	public class UnauthenticateAccountAction : IAction
	{
		#region IAction Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				//  make sure we got exactly 1 matching account
				if (HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT] != null) {
					// TODO: this should not access HttpContext directly!
					HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT] = null;

					// expire the cookie if it exists
					if (context.Request.GetCookie(MembershipConstants.COOKIE_USER_ACCOUNT).Value != "") {
						MvcCookie cookie = context.Request.GetCookie(MembershipConstants.COOKIE_USER_ACCOUNT);
						cookie.Expires = DateTime.Now.AddDays(-1);
						context.Request.SetResponseCookie(cookie);
					}

					retEvent = Events.Ok;
				}
			} catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("There was an error while unauthenticating an account."), ex);
			}

			return retEvent;
		}

		#endregion

		#region Nested type: Events

		public class Events
		{
			public static string Ok
			{
				get { return EventNames.OK; }
			}

			public static string Error
			{
				get { return EventNames.ERROR; }
			}
		}

		#endregion
	}
}