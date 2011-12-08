using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Membership.Model;
using Triton.Membership.Model.Dao;
using Triton.Membership.Model.Dao.Support;
using Triton.Membership.Support.Request;
using Triton.Model;
using Triton.Model.Dao;

namespace Triton.Membership.Logic
{
	public class GetAccountAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public string AccountItemNameOut { get; set; }


		public GetAccountAction()
		{
			this.AccountItemNameOut = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;
		}


		#region IAction Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				IAccountDao dao = DaoFactory.GetDao<IAccountDao>();

				AccountFilter filter = dao.GetFilter();

				filter.Fill(context.Request);

				SearchResult<Account> accounts = dao.Find(filter);

				context.Request.Items[this.AccountItemNameOut] = accounts;

				retEvent = EventUtilities.GetSearchResultEventName(accounts.Items.Length);
			} catch (Exception ex) {
				this.logger.Error("Error occured in Execute.", ex);
			}

			return retEvent;
		}

		#endregion


		#region Nested type: Events

		public class Events
		{
			public static string Zero
			{
				get { return EventNames.ZERO; }
			}

			public static string One
			{
				get { return EventNames.ONE; }
			}

			public static string Multiple
			{
				get { return EventNames.MULTIPLE; }
			}

			public static string Error
			{
				get { return EventNames.ERROR; }
			}
		}

		#endregion
	}
}