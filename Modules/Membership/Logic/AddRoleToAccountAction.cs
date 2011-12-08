using System;
using System.Collections.Generic;
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
	public class AddRoleToAccountAction : IAction
	{
		public AddRoleToAccountAction()
		{
			this.AccountItemNameIn = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;

			this.RoleItemNameIn = ItemNames.Role.DEFAULT_SEARCH_RESULT_ROLES;

			this.AccountItemNameOut = ItemNames.Account.DEFAULT_SEARCH_RESULT_ACCOUNTS;
		}
        
		public string AccountItemNameIn { get; set; }

		public string RoleItemNameIn { get; set; }

		public string AccountItemNameOut { get; set; }

		#region IAction Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				if (context.Request.Items[this.AccountItemNameIn] == null ||
				    !(((context.Request.Items[this.AccountItemNameIn] is SearchResult<Account> ||
                     context.Request.Items[this.AccountItemNameIn] is Account))))   {
					
					throw new MissingFieldException("Could not find the account item in the request items.");
				}

				if (context.Request.Items[this.RoleItemNameIn] == null ||
				    !(context.Request.Items[this.RoleItemNameIn] is SearchResult<Role>) ||
				    context.Request.GetItem<SearchResult<Role>>(this.RoleItemNameIn).Items.Length != 1) {
					
					throw new MissingFieldException("Could not find the role item in the request items.");
				}
			    Account account;
                if (context.Request.Items[this.AccountItemNameIn] is SearchResult<Account>)
                {
                     account = context.Request.GetItem<SearchResult<Account>>(this.AccountItemNameIn).Items[0];
                }else
                {
                     account = context.Request.GetItem<Account>(this.AccountItemNameIn);
                }
			    Role role = context.Request.GetItem<SearchResult<Role>>(this.RoleItemNameIn).Items[0];

                if (account.Roles == null)
                {
                    account.Roles = new List<Role>();
                }
				if (!account.IsMemberOf(role.Code)) {
                    
					account.Roles.Add(role);
				}

				context.Request.Items[this.AccountItemNameOut] = new SearchResult<Account>(new Account[] {account});

				retEvent = Events.Ok;

			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error("Error occured while saving role to account.", e);
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