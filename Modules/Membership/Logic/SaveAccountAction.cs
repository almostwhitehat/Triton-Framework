using System;
using Common.Logging;
using Triton.CodeContracts;
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
	public class SaveAccountAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public SaveAccountAction()
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

				ActionContract.Requires<NullReferenceException>(context.Request.Items[this.AccountItemNameIn] != null, "Could not retrieve the account from the request to save.");

				ActionContract.Requires<TypeMismatchException>(context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> || context.Request.Items[this.AccountItemNameIn] is Account, "The account item was not of type SearchResult<Account> or Account.");

				ActionContract.Requires<ApplicationException>(context.Request.Items[this.AccountItemNameIn] is Account || (context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> && context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items.Length > 0), "The account items collection did not contain any items.");
            
				IAccountDao dao = DaoFactory.GetDao<IAccountDao>();

				if (context.Request.Items[this.AccountItemNameIn] is SearchResult<Account>) {
					SearchResult<Account> accounts = context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn);

					foreach (Account account in accounts.Items) {
						account.ModifiedDate = DateTime.Now;
					}

					dao.Save(accounts.Items);

					context.Request.Items[this.AccountItemNameOut] = accounts;
				} else {
					Account account = context.Request.GetItem<Account>(this.AccountItemNameIn);
					account.ModifiedDate = DateTime.Now;
					dao.Save(account);

					context.Request.Items[this.AccountItemNameOut] = account;
				}

				retEvent = Events.Ok;
			} catch (Exception ex) {
				this.logger.Error(
					errorMessage => errorMessage("There was an error while saving an account."), ex);
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