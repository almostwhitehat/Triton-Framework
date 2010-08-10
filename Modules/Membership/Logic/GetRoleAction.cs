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
	public class GetRoleAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public GetRoleAction()
		{
			this.RoleItemNameOut = ItemNames.Role.DEFAULT_SEARCH_RESULT_ROLES;
			this.All = false;
		}


		public string RoleItemNameOut { get; set; }

		public bool All { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				IRoleDao dao = DaoFactory.GetDao<IRoleDao>();

				RoleFilter filter = dao.GetFilter();

				//if all is not set, filter results
				if (!this.All) {
					filter.Fill(context.Request);
				}

				SearchResult<Role> result = dao.Find(filter);

				context.Request.Items[this.RoleItemNameOut] = result;

				retEvent = EventUtilities.GetSearchResultEventName(result.Items.Length);
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