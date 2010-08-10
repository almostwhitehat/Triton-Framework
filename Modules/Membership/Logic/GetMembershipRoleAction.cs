namespace web.security.membership.action
{

    using System;
    using System.Collections.Generic;
    using web.controller;
    using web.model;
    using web.security.membership.model;
    using web.security.membership.model.dao;
    using web.support.logging;

    /// <summary>
    /// Get the membership roles defined in the system.
    /// </summary>
    /// <remarks>
    /// 
    /// Returned events:<br/>
    /// <b>zero</b> - no matching accounts found<br/>
    /// <b>one</b> - one matching account found<br/>
    /// <b>multiple</b> - more than one matching accounts found<br/>
    /// <b>error</b> - an error occurred while searching for acounts<br/>
    /// </remarks>
    public class GetMembershipRoleAction : web.controller.action.Action
    {
        protected const string EVENT_FOUND_0 = "zero";
        protected const string EVENT_FOUND_1 = "one";
        protected const string EVENT_FOUND_MULTIPLE = "multiple";
        protected const string EVENT_ERROR = "error";
        protected const string DEFAULT_ITEM_NAME = "roles";

        private string requestItemName = null;


        /// <summary>
        /// Gets or sets the name of the results of the action used to identify this result in
        /// the Request.Items collection.
        /// </summary>
        public string RequestItemName
        {
            get
            {
                return (this.requestItemName == null) ? DEFAULT_ITEM_NAME : this.requestItemName;
            }
            set
            {
                this.requestItemName = value;
            }
        }


        #region Action Members

        public string Execute(
            TransitionContext context)
        {
            string retEvent = EVENT_ERROR;

            try
            {
                // TODO: this should come from DAOFactory!!
                //            AccountDAO dao = AccountDAOFactory.GetAccountDAO();


                //  get the filter from the DAO
                RoleFilter filter = new RoleFilter();

                //  populate the filter from the request
                //filter.Fill(context.Request);

                //  get the account for the given username
                Dictionary<string, Role> result = RoleManager.GetInstance().GetRoles(filter);

                context.Request.Items[RequestItemName] = result;

                //  determine which event to return based on the number of
                //  accounts found
                switch (result.Count)
                {
                    case 0:
                        retEvent = EVENT_FOUND_0;
                        break;
                    case 1:
                        retEvent = EVENT_FOUND_1;
                        break;
                    default:
                        retEvent = EVENT_FOUND_MULTIPLE;
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("GetUserAction", e);
            }

            return retEvent;
        }

        #endregion
    }
}
