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

    /// <summary>
    /// 
    /// </summary>
    public class DeleteMemberContactAction : web.controller.action.Action
    {
        private const string EVENT_SUCCESS = "success";
        private const string EVENT_ERROR = "error";
        private string requestUserName = "users";
        private string requestItemName = "deletemembercontact";
        /// <summary>
        /// 
        /// </summary>
        public string UserRequestItemName
        {
            get { return requestUserName; }
            set { requestUserName = value; }
        }

        #region Action Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string Execute(
            TransitionContext context)
        {
            string retEvent = EVENT_ERROR;

            try
            {

                SearchResult<Account> acctResults = (SearchResult<Account>)context.Request.Items[requestUserName];

                int contactId = int.Parse(context.Request["contactvalues"]);

                if ((acctResults == null) || (acctResults.Items.Count == 0))
                {
                    throw new ApplicationException("No account found in Request.Items.");
                }

                Account account = acctResults.Items[0];

                if (account == null)
                {
                    throw new ApplicationException("No account found in SearchResult.");
                }

                try
                {

                    AccountDAO dao = AccountDAOFactory.GetAccountDAO();
                    dao.DeleteContact(account.Id, contactId);

                    retEvent = EVENT_SUCCESS;

                }
                catch (Exception) { }


            }
            catch (Exception e)
            {
                Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("DeleteMemberContactAction:Execute", e);
            }


            return retEvent;
        }

        /// <summary>
        /// 
        /// </summary>
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
