using System;
using System.Collections.ObjectModel;
using web.controller;
using web.model;
using web.security.membership.model;
using web.security.membership.model.dao;
using web.support.logging;

namespace web.security.membership.action {

    /// <summary>
    /// 
    /// UserRequestItemName must be set to the RequestItemName for a previous GetUser Action
    /// </summary>

    public class SaveUserAttributeAction : web.controller.action.FormatableAction
    {
	    private const	string	EVENT_SUCCESS			= "success";
	    private const	string	EVENT_FAILURE			= "failure";
	    private const	string	DEFAULT_ITEM_NAME		= "saved";

        private string      userRequestItemName = "users";
	    private	string		connectionType		= null;
	    private	string		requestItemName		= null;
        private bool        optionAllowInserts  = true;
	    public string ConnectionType
	    {
		    get {
			    return this.connectionType;
		    }
		    set {
			    this.connectionType = value;
		    }
	    }

        
	    /// <summary>
	    /// Gets or sets the name of the results of the action used to identify this result in
	    /// the Request.Items collection.
	    /// </summary>
	    public string RequestItemName
	    {
		    get {
			    return this.requestItemName ?? DEFAULT_ITEM_NAME;
		    }
		    set {
			    this.requestItemName = value;
		    }
	    }

        /// <summary>
        /// Gets or sets the option to allow inserts of user attributes
        /// if false only removals will occur.
        /// </summary>
        public string AllowInserts
        {
            get
            {
                return this.optionAllowInserts.ToString();
            }
            set
            {
                this.optionAllowInserts = (value.ToLower() == "true");
            }
        }

	    #region Action Members

	    public override string Execute(
		    TransitionContext context)
	    {
		    string retEvent	= EVENT_FAILURE;

		    try {
    // TODO: should not be hard-coded to "users" -- has to match RequestItemName on GetUser action
			    SearchResult<Account> acctResults = (SearchResult<Account>)context.Request.Items[UserRequestItemName];

    // TODO: what if > 1 account in list??
			    if ((acctResults == null) || (acctResults.Items.Count == 0)) {
				    throw new ApplicationException("No account found in Request.Items.");
			    }

			    Account account = acctResults.Items[0];

			    if (account == null) {
				    throw new ApplicationException("No account found in SearchResult.");
			    }

					    //  get the attribute type (name) and value from the request
			    string[] attrType = context.Request[MembershipConstants.PARAM_ATTRIBUTE_TYPE].Split(',');
			    string[] attrValue = context.Request[MembershipConstants.PARAM_ATTRIBUTE_VALUE].Split(',');

                if ( attrType.Length == attrValue.Length ) {
                    for ( int i = 0; i < attrType.Length; i++ ) {
					    //  determine if the attribute specified in the request already exists in the account
                        //int index = account.IndexOfAttribute(attrType[i], attrValue[i]);
                        
                        bool allowMult = true;
                        
                        try
                        {
                            AccountAttributeType attributeType = SingletonBase<AccountAttributeType>.GetInstance()[attrType[i]];
                            allowMult = attributeType.AllowMultiples;
                        }
                        catch (Exception exc) {
                            Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("SaveUserAttributeAction.Execute: ", exc);
                        }

                        // TODO: this should be obtained from the DaoFactory
					    // get the DAO
                        AccountDAO dao = AccountDAOFactory.GetAccountDAO(this.ConnectionType);

			            if (dao == null) {
				            throw new ApplicationException(string.Format("Unable to obtain DAO. [ConnectionType = '{0}']", ConnectionType));
			            }

                        if (allowMult)
                        {
                            int index = account.IndexOfAttribute(attrType[i], attrValue[i]);
                            //  if the attribute alread exists, delete it, else add it
                            if (index >= 0)
                            {
                                dao.DeleteAttribute(account.Id, attrType[i], attrValue[i]);
                            }
                            else
                            {
                                if (optionAllowInserts)
                                    dao.InsertAttribute(account.Id, attrType[i], attrValue[i]);
                            }
                        }
                        else {
                            int index = account.GetAttribute(attrType[i]).Count;
                            if (index >= 0)
                            {
                                if (optionAllowInserts)
                                {
                                    dao.DeleteMultipleAttribute(account.Id, attrType[i]);
                                    dao.InsertAttribute(account.Id, attrType[i], attrValue[i]);
                                }
                            }
                            else
                            {
                                if (optionAllowInserts)
                                    dao.InsertAttribute(account.Id, attrType[i], attrValue[i]);
                            }
                        
                        }

                    }
                }
    			

			    retEvent = EVENT_SUCCESS;
		    } catch (Exception e) {
			    Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("SaveUserAttributeAction.Execute: ", e);
		    }

				    //  TODO: GET THIS OUT OF HERE!!
				    //  This is REALLY ugly -- used to get the last event fired back to the client
				    //  for ajax requests
		    if (this.Formatter != null) {
			    context.Request.Items[this.requestItemName] = this.Formatter.Format(retEvent);
		    }

		    return retEvent;
	    }

	    #endregion
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
    }
}
