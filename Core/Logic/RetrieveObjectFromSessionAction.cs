using System;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Logic.Support;
using Triton.Support.Request;
using Triton.Utilities.Reflection;
using Triton.CodeContracts;

namespace Triton.Logic {

#region History

// History:

#endregion

/// <summary>
/// Action to retrieve an object from session.
/// </summary>
/// <remarks>
/// <para>Returned events:</para>
/// <c>ok</c> - successfully got the object from session.<br/>
/// <c>novalue</c> - no object was found in session for the given name.<br/>
/// <c>error</c> - an error occurred while attempting to get the object from session.<br/>
/// </remarks>
///	<author>Scott Dyke</author>
///	<created>3/11/10</created>
public class RetrieveObjectFromSessionAction : IAction
{


	/// <summary>
	/// Gets or sets the name in Request.Items to place the the object retrieved.
	/// </summary>
	public string ObjectItemNameOut
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the object in session to retrieve.
	/// </summary>
	public string SessionName
	{
		get;
		set;
	}


	#region Action Members

	public string Execute(
		TransitionContext context)
	{
		string retEvent = Events.Error;
		MvcRequest request = context.Request;

		try {
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(ObjectItemNameOut),
					"No item name given in the ObjectItemNameOut attribute.");
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(SessionName),
					"No session name to retrieve the object from given in the SessionName attribute.");
			
			if(System.Web.HttpContext.Current.Session[SessionName] != null) {
				
				if (System.Web.HttpContext.Current.Session[SessionName] is string && string.IsNullOrEmpty(System.Web.HttpContext.Current.Session[SessionName].ToString())) {
					retEvent = Events.NoValue;
				
				} else {
					request.Items[ObjectItemNameOut] = System.Web.HttpContext.Current.Session[SessionName];

					retEvent = Events.Ok;
				}
				
			} else {
				retEvent = Events.NoValue;
			}

			
		} catch (Exception e) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error(error => error("Error getting object property value."), e);
		}

		return retEvent;
	}

	#endregion


	#region Nested type: Events

	public class Events
	{
		public static string Ok
		{
			get {
				return EventNames.OK;
			}
		}

		public static string NoValue
		{
			get {
				return EventNames.NO_VALUE;
			}
		}

		public static string Error
		{
			get {
				return EventNames.ERROR;
			}
		}
	}

	#endregion
}
}
