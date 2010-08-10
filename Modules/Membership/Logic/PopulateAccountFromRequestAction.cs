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
	public class PopulateAccountFromRequestAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public PopulateAccountFromRequestAction()
		{
			this.AccountItemNameIn = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;

			this.AccountItemNameOut = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;
		}


		public string AccountItemNameIn { get; set; }

		public string AccountItemNameOut { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				if (context.Request.Items[this.AccountItemNameIn] == null ||
				    !(context.Request.Items[this.AccountItemNameIn] is SearchResult<Account>) ||
				    context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items.Length != 1) {
					
					throw new MissingFieldException("Could not retrieve an account or the more than one account was returned.");
				}

				Account account = context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items[0];

				/* Hmm whats better... one line or 3...
					context.Request.Items[this.AccountsItemNameIn] = new SearchResult<Account>( new[] { Deserialize.Populate(context.Request, accounts.Items[0]) });
					*/

				Account toReturn = Deserialize.Populate(context.Request, account);

				SearchResult<Account> result = new SearchResult<Account>(new Account[] {toReturn});

				context.Request.Items[this.AccountItemNameOut] = result;

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