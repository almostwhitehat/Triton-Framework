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
	public class SaveRoleAction : IAction
	{
		private readonly ILog logger = LogManager.GetCurrentClassLogger();


		public SaveRoleAction()
		{
			this.RoleItemNameIn = ItemNames.Role.DEFAULT_SEARCH_RESULT_ROLES;
		}


		public string RoleItemNameIn { get; set; }

		#region IAction Members

		public string Execute(TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				if (context.Request.Items[this.RoleItemNameIn] == null ||
				    !(context.Request.Items[this.RoleItemNameIn] is SearchResult<Role>) ||
				    context.Request.GetItem<SearchResult<Role>>(this.RoleItemNameIn).Items.Length == 0) {
					throw new MissingFieldException("Could not retrieve an roles to save.");
				}

				SearchResult<Role> roles = context.Request.GetItem<SearchResult<Role>>(this.RoleItemNameIn);


				IRoleDao dao = DaoFactory.GetDao<IRoleDao>();

				dao.Save(roles.Items);

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