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
using Triton.Support.Session;

namespace Triton.Logic {

#region History

// History:

#endregion

/// <summary>
/// Action to store an object to session.
/// </summary>
/// <remarks>
/// <para>Returned events:</para>
/// <c>ok</c> - successfully stored the object in session.<br/>
/// <c>novalue</c> - no object was found in Request.Items for the given name.<br/>
/// <c>error</c> - an error occurred while attempting to store the object to session.<br/>
/// </remarks>
///	<author>Scott Dyke</author>
///	<created>3/11/10</created>
public class StoreObjectToSessionAction : IAction
{

	public StoreObjectToSessionAction()
	{
		ExtractFromSearchResult = false;
	}


	/// <summary>
	/// Gets or sets the name of the item in Request.Items containing the object
	/// to place into session.  If wrapped in a SearchResult, the action unwraps the object.
	/// </summary>
	public string ObjectItemNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name under which the object is stored in session.
	/// </summary>
	public string SessionName
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the flag to indicate if the object stored in session
	/// should be extracted from a SearchResult if embedded in such, or
	/// store "as is" from Items (keeping the SearchResult if present).
	/// Default is <c>false</c>, keeping the SearchResult if present.
	/// </summary>
	public bool ExtractFromSearchResult
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
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(ObjectItemNameIn), "No item name given in the ObjectItemNameIn attribute.");
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(SessionName), "No name to store the object in given in the SessionName attribute.");

			object obj = null;
			if (ExtractFromSearchResult) {
				obj = request.GetRequestItem<object>(ObjectItemNameIn, true);
				if (obj.GetType().Name.StartsWith("SearchResult")) {
					Array list = (Array)ReflectionUtilities.GetPropertyValue(obj, "Items");
					obj = list.GetValue(0);
				}
			} else {
				obj = request.Items[ObjectItemNameIn];
			}

			if (obj != null) {
				SessionStateProvider.GetSessionState()[SessionName] = obj;
				retEvent = Events.Ok;
			} else {
				retEvent = Events.NoValue;
			}
		} catch (Exception e) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error(error => error("Error storing object to session."), e);
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
