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
	public class PopulateRoleFromRequestAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public PopulateRoleFromRequestAction()
		{
			this.RoleItemNameIn = ItemNames.Role.DEFAULT_SEARCH_RESULT_ROLES;

			this.RoleItemNameOut = ItemNames.Role.DEFAULT_SEARCH_RESULT_ROLES;
		}


		public string RoleItemNameIn { get; set; }

		public string RoleItemNameOut { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				if (context.Request.Items[this.RoleItemNameIn] == null ||
				    !(context.Request.Items[this.RoleItemNameIn] is SearchResult<Role>) ||
				    context.Request.GetItem<SearchResult<Role>>(this.RoleItemNameIn).Items.Length != 1) {
					throw new MissingFieldException("Could not retrieve an role or the more than one role was returned.");
				}

				Role role = context.Request.GetItem<SearchResult<Role>>(this.RoleItemNameIn).Items[0];

				Role toReturn = Deserialize.Populate(context.Request, role);

				SearchResult<Role> result = new SearchResult<Role>(new Role[] {toReturn});

				context.Request.Items[this.RoleItemNameOut] = result;

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