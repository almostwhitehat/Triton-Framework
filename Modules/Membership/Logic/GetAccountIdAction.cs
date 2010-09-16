using System;
using System.Text;
using Common.Logging;
using Triton.CodeContracts;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Support.Request;
using Triton.Model;

namespace Triton.Membership.Logic
{

	/// <summary>
	/// Builds a string concatenated list of account id's from the account in the request.
	/// </summary>
	public class GetAccountIdAction : IAction
	{
		/// <summary>
		/// The account item name to retrieve from the request.
		/// </summary>
		public string AccountItemNameIn { get; set; }

		/// <summary>
		/// String parameter name to place the account id in the request.
		/// </summary>
		public string AccountIdParamNameOut { get; set; }

		/// <summary>
		/// Default constructor.
		/// </summary>
		public GetAccountIdAction()
		{
			this.AccountItemNameIn = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;

			this.AccountIdParamNameOut = ParameterNames.Account.Field.ID;
		}

		private readonly ILog logger = LogManager.GetCurrentClassLogger();

		#region IAction Members

		public string Execute(Triton.Controller.TransitionContext context)
		{
			string retEvent = Events.Error;

			try {

				ActionContract.Requires<NullReferenceException>(
				context.Request.Items[this.AccountItemNameIn] != null,
				"Could not retrieve the account from the request to save.");

				ActionContract.Requires<TypeMismatchException>(
					context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> || context.Request.Items[this.AccountItemNameIn] is Account,
					"The account item was not of type SearchResult<Account> or Account.");

				ActionContract.Requires<ApplicationException>(
					context.Request.Items[this.AccountItemNameIn] is Account || (context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> && context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items.Length > 0),
					"The account items collection did not contain any items.");

				StringBuilder builder = new StringBuilder("");

				foreach(Account account in context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items) {
					builder = builder.Append(account.Id + ",");
				}

				if(builder.Length > 1) {
					builder = builder.Remove(builder.Length-1, 1);
				}

				context.Request[this.AccountIdParamNameOut] = builder.ToString();

				retEvent = Events.Ok;

			} catch (Exception ex) {

				this.logger.Error("Error occured in Execute.", ex);

			}

			return retEvent;
		}

		#endregion
	}

	#region Nested type : Events

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
