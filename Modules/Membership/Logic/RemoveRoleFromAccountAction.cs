using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Support.Request;
using Triton.Model;

namespace Triton.Membership.Logic
{
	public class RemoveRoleFromAccountAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public RemoveRoleFromAccountAction()
		{
			this.AccountItemNameIn = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;

			this.RoleItemNameIn = ItemNames.Role.DEFAULT_SEARCH_RESULT_ROLES;

			this.AccountItemNameOut = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;
		}


		public string AccountItemNameIn { get; set; }

		public string RoleItemNameIn { get; set; }

		public string AccountItemNameOut { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				if (context.Request.Items[this.AccountItemNameIn] == null ||
				    !(context.Request.Items[this.AccountItemNameIn] is SearchResult<Account>) ||
				    context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items.Length != 1) {
					throw new MissingFieldException("Could not find the account item in the request items.");
				}

				if (context.Request.Items[this.RoleItemNameIn] == null ||
				    !(context.Request.Items[this.RoleItemNameIn] is SearchResult<Role>) ||
				    context.Request.GetItem<SearchResult<Role>>(this.RoleItemNameIn).Items.Length != 1) {
					throw new MissingFieldException("Could not find the role item in the request items.");
				}

				Account account = context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items[0];

				Role role = context.Request.GetItem<SearchResult<Role>>(this.RoleItemNameIn).Items[0];


				if (account.IsMemberOf(role.Code)) {
					account.Roles.Remove(role);
				}

				context.Request.Items[this.AccountItemNameOut] = new SearchResult<Account>(new Account[] {account});

				retEvent = Events.Ok;
			} catch (Exception ex) {
				this.logger.Error("Error occured in Execute.", ex);
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