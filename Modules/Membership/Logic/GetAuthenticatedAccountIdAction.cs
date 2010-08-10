using System;
using System.Web;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Support;
using Triton.Membership.Support.Request;

namespace Triton.Membership.Logic
{
	public class GetAuthenticatedAccountIdAction : IAction
	{
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

		public string AuthenticatedAccountIdParamNameOut { get; set; }

		public GetAuthenticatedAccountIdAction()
		{
			this.AuthenticatedAccountIdParamNameOut = ParameterNames.Account.AUTHENTICATED_ID;
		}

		private readonly ILog logger = LogManager.GetCurrentClassLogger();

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {

				context.Request[this.AuthenticatedAccountIdParamNameOut] = ((Account)HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT]).Id.ToString();

				retEvent = Events.Ok;
			} catch (Exception ex) {
				this.logger.Error("Error occured in Execute.", ex);
			}

			return retEvent;
		}

		#endregion
	}
}