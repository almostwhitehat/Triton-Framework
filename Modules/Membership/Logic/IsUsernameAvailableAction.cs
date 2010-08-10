using System;
using System.Linq;
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
	public class IsUsernameAvailableAction : IAction
	{
		public IsUsernameAvailableAction()
		{
			this.AccountItemNameIn = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;
		}


		public string AccountItemNameIn { get; set; }

		#region IAction Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				if (context.Request.Items[this.AccountItemNameIn] != null &&
				    context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> &&
				    context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items.Length == 1) {
					Account account = context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items[0];

					Username name = account.Usernames.FirstOrDefault(x => x.Value == context.Request[ParameterNames.Account.USERNAME]);
					
					retEvent = name != null && account.Id.HasValue && account.Id.Value != Guid.Empty ? Events.Yes : this.CheckDao(context);
				} else {
					
					retEvent = this.CheckDao(context);
				}
			} catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Error("Error occured in Execute.", ex);
			}

			return retEvent;
		}


		private string CheckDao(TransitionContext context)
		{
			IAccountDao dao = DaoFactory.GetDao<IAccountDao>();

			AccountFilter filter = dao.GetFilter();

			filter.Usernames = new[] {context.Request[ParameterNames.Account.USERNAME]};

			SearchResult<Account> acc = dao.Find(filter);

			string retEvent = acc.Items.Length > 0 ? Events.No : Events.Yes;

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

			public static string Error
			{
				get { return EventNames.ERROR; }
			}
		}

		#endregion
	}
}