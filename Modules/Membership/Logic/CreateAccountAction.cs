using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Model.Dao;
using Triton.Membership.Support.Request;
using Triton.Model;
using Triton.Model.Dao;

namespace Triton.Membership.Logic
{
	public class CreateAccountAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public CreateAccountAction()
		{
			this.AccountsItemNameOut = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;
		}


		public string AccountsItemNameOut
		{
			get;
			set;
		}


		/// <summary>
		/// Overloaded AccountsItemNameOut so that the state's attribute can match the action "Account".
		/// </summary>
		public string AccountItemNameOut
		{
			get {
				return AccountsItemNameOut;
			}
			set {
				AccountsItemNameOut = value;
			}
		}

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				/* Hmm whats better... one line or 3...
				context.Request.Items[this.AccountsItemNameOut] = new SearchResult<Account>( new[] { Deserialize.CreateAccount(context.Request) });
				*/

				Account toReturn = Deserialize.CreateAccount(context.Request);

				toReturn.Status = DaoFactory.GetDao<IAccountStatusDao>().Get("active");

				SearchResult<Account> result = new SearchResult<Account>(new Account[] {toReturn});

				context.Request.Items[this.AccountsItemNameOut] = result;

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