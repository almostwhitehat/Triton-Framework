


namespace web.security.membership.action {

    using System;
    using System.Collections.ObjectModel;
    using web.controller;
    using web.model;
    using web.security.membership.model;
    using web.security.membership.model.dao;
    using web.support.logging;
    using web.util.action;

    public class GetUserAttributeAction : web.controller.action.FormatableAction
    {
	    private const	string	DEFAULT_ITEM_NAME		= "attributes";
        
	    private	string		connectionType		= null;
	    private	string		requestItemName		= null;
        private string      attributeTypeName   = null;

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
            get
            {
                return this.requestItemName ?? DEFAULT_ITEM_NAME;
            }
            set
            {
                this.requestItemName = value;
            }
        }
        public string AttributeTypeName {
            get { return attributeTypeName; }
            set { attributeTypeName = value; }
        }

	    #region Action Members

	    public override string Execute(
		    TransitionContext context)
	    {
		    string retEvent	= ActionUtil.EVENT_ERROR;
            ReadOnlyCollection<MembershipAttribute> result = null;
		    try {
    // TODO: should not be hard-coded to "users" -- has to match RequestItemName on GetUser action
			    SearchResult<Account> acctResults = (SearchResult<Account>)context.Request.Items["users"];

    // TODO: what if > 1 account in list??
			    if ((acctResults == null) || (acctResults.Items.Count == 0)) {
				    throw new ApplicationException("No account found in Request.Items.");
			    }

			    Account account = acctResults.Items[0];

			    if (account == null) {
				    throw new ApplicationException("No account found in SearchResult.");
			    }

					    //  get the attribute type (name) and value from the request
			    string attrType = AttributeTypeName ?? context.Request[MembershipConstants.PARAM_ATTRIBUTE_TYPE];

			    //  determine if the attribute specified in the request already exists in the account
			    //int index = account.IndexOfAttribute(attrType, attrValue);

				//  if the attribute alread exists, delete it, else add it
                if (account.GetAttribute(attrType) != null)
                {
                    result = account.GetAttribute(attrType);
			    } else {
                }

                context.Request.Items[RequestItemName] = result;
                switch (result.Count)
                {
                    case 0:
                        retEvent = ActionUtil.EVENT_FOUND_ZERO;
                        break;
                    case 1:
                        retEvent = ActionUtil.EVENT_FOUND_ONE;
                        break;
                    default:
                        retEvent = ActionUtil.EVENT_FOUND_MULTIPLE;
                        break;
                }
			    //retEvent = ActionUtil.EVENT_PASS;
		    } catch (Exception e) {
			    Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("SaveUserAttributeAction.Execute: ", e);
		    }

		    if (this.Formatter != null) {
                context.Request.Items[this.requestItemName] = this.Formatter.Format(result);
		    }

		    return retEvent;
	    }

	    #endregion
    }
}
