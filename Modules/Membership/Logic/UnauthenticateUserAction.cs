using System;
using web.controller;
using web.model;
using web.security.membership;
using web.security.membership.model;
using web.security.membership.model.dao;
using web.util.crypto;
using web.support.logging;
using web.error;

namespace web.security.membership.action {


public class UnauthenticateUserAction : web.controller.action.Action
{
	private const	string	EVENT_OK				= "ok";
	private const	string	EVENT_INVALID			= "error";

	private			string	connectionType			= null;
	private			string	errorId					= null;

	public string ConnectionType
	{
		get {
			return this.connectionType;
		}
		set {
			this.connectionType = value;
		}
	}


	public string ErrorId
	{
		get {
			return this.errorId;
		}
		set {
			this.errorId = value;
		}
	}

	#region Action Members

	public string Execute(
		TransitionContext context)
	{
		string retEvent = EVENT_INVALID;

		try {

					//  make sure we got exactly 1 matching account
			if (System.Web.HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT] != null )
            {
                Account acct = (Account)System.Web.HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT];
                // update logout time
                


                // TODO: this should not access HttpContext directly!
				System.Web.HttpContext.Current.Session[MembershipConstants.SESSION_USER_ACCOUNT] = null;
                System.Web.HttpContext.Current.User = null;

                retEvent = EVENT_OK;
			}

		} catch (Exception e) {

                ErrorList errors = new ErrorList();
                ///TODO : fix this :
                //ErrorList errors = new ErrorList(FPErrorDictionary.GetDictionary(context.Site));
                //	create the error
				//Error error = FPErrorDictionary.GetDictionary(context.Site).getError(long.Parse(this.errorId));
                Error error = new Error();
                error.Id = long.Parse(this.errorId);
                error.Message = e.Message;
                error.Type = Error.ErrorType.FATAL;
                error.FormField = "";
				errors.Add(error);

				//  add the errors to the request to propagate back to the page
				context.Request.Items["errors"] = errors;
		}

		return retEvent;
	}

	#endregion

    #region Action Members

    public string RequestItemName
    {
        get
        {
            throw new Exception("The method or operation is not implemented.");
        }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    #endregion
}
}
