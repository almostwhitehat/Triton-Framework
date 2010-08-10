using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Model.Dao;
using Triton.Membership.Support;
using Triton.Model.Dao;
using Triton.Utilities;

namespace Triton.Membership.Logic
{
	/// <remarks>
	/// Returned events:<br/>
	/// <b>yes</b> - the account/user with the given username is logged in.<br/>
	/// <b>no</b> - the account/user with the given username is not logged in.<br/>
	/// </remarks>
	public class IsMemberOfAction : IAction
	{

		/// <summary>
		/// Gets or sets the role names that are to be used to test against.
		/// </summary>
		public string Roles { get; set; }

		#region Action Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.No;

			try {
				if(string.IsNullOrEmpty(this.Roles)) {
					throw new NullReferenceException("Roles action variable was not set.");
				}

				//  try to get the account ID from the session
				Account acct = System.Web.HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT] as Account;

				IAccountDao dao = DaoFactory.GetDao<IAccountDao>();

				acct = dao.Get(acct.Id.Value);

				//  if we got the account from the session, set the return event accordingly
				if (acct != null) {
					// get the list of role to check for
					string[] roles = this.Roles.ToStringArray();
					foreach ( string role in roles ) {
                        
						if ( acct.IsMemberOf(role) ) {
							retEvent = Events.Yes;
						}
					}
				}
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(error => error("Error occured."), e);
			}

			return retEvent;
		}

		#endregion

		#region Nested type: Events

		public class Events
		{
			public static string Yes
			{
				get { return EventNames.YES; }
			}

			public static string No
			{
				get { return EventNames.NO; }
			}
		}

		#endregion
	}
}