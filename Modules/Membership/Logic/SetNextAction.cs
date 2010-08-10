using System;
using web.controller;
using web.support.logging;
using web.security.membership.model;
using web.error;

namespace web.security.membership.action {


/// <remarks>
/// Returned events:<br/>
/// <b>error</b> - the refering Url was not located.<br/>
/// <b>ok</b> - the current request was redirected to the origional request.<br/>
/// </remarks>
public class SetNextActionAction : web.controller.action.FormatableAction
{
    private const string EVENT_ERROR            = "error";
    private const string EVENT_OK               = "ok";

    private string requestItemName              = "refererUrl";


	/// <summary>
	/// Gets or sets the name of the results of the action used to identify this result in
	/// the Request.Items collection.
	/// </summary>
	public string RequestItemName
	{
		get {
			return this.requestItemName;
		}
		set {
			this.requestItemName = value;
		}
	}


	#region Action Members

	public override string Execute(
		TransitionContext context)
	{
		string retEvent = EVENT_ERROR;
        
		try {
            if ( context.Request[RequestItemName] != "" ){
                // redirect the currrent request
                // this isa hack but it is required.
                context.Request.Redirect(context.Request[RequestItemName]);
            }
            else {
                web.error.ErrorList eList = new web.error.ErrorList();
                eList.Add(new Error(1,"none","unable to redirect to origional request",Error.ErrorType.WARNING));
                retEvent = EVENT_ERROR;
            }
		} catch (Exception e) {
			Logger.GetLogger(MembershipConstants.ACTION_LOGGER).Error("IsLoggedInAction", e);
		}

		return retEvent;
	}

	#endregion
}
}
