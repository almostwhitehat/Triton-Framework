using System;
using System.Configuration;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using web.controller;
using web.controller.request;
using web.controller.util;
using web.model;
using web.security.membership;
using web.security.membership.model;
using web.security.membership.model.dao;
using web.util.reflection;
using web.util.crypto;
using web.support.logging;

namespace web.security.membership.action
{


    public class UpdateUserStatusAction : web.controller.action.Action
    {
        private const string EVENT_SUCCESS = "success";
        private const string EVENT_ERROR = "error";

        
        private string requestItemName = "";
        private string userRequestItemName = "users";
        private string connectionType = null;
        /// <summary>
        /// Request Item Name for the user accounts
        /// </summary>
        public string UserRequestItemName
        {
            get
            {
                return userRequestItemName;
            }
            set
            {
                userRequestItemName = value;
            }
        }
        /// <summary>
        /// Property used to assign the db connection type
        /// </summary>
        public string ConnectionType
        {
            get
            {
                return this.connectionType;
            }
            set
            {
                this.connectionType = value;
            }
        }
        #region Action Members


        /// <summary>
        /// Primary Action Method
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string Execute(
            TransitionContext context)
        {
           string retEvent = EVENT_ERROR;
        
            try 
            {
                SearchResult<Account> srAccount = (SearchResult<Account>)context.Request.Items[userRequestItemName];

                Account account = (srAccount != null) ? srAccount.Items[srAccount.Items.Count - 1] : null;

                AccountStatus status = SingletonBase<AccountStatus>.GetInstance()[context.Request["status"]];

                account.Status = status;

                AccountDAO dao = AccountDAOFactory.GetAccountDAO(this.ConnectionType);

                dao.UpdateAccountStatus(account.Id, status);

                retEvent = EVENT_SUCCESS;
            }
            catch (Exception e) { }

            return retEvent;
        }

        #endregion

        #region Action Members

        public string RequestItemName
        {
            get
            {
                return requestItemName;
            }
            set
            {
                requestItemName = value;
            }
        }

        #endregion
    }
}
