using System;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Model;
using Triton.Model.Dao;
using Triton.Logic.Support;
using Triton.Support.Request;
using Triton.Utilities.Reflection;
using Triton.CodeContracts;

namespace Triton.Logic {

#region History

// History:
//    12/3/13	SD	Added support for ObjectPropertyNameIn to be a path to a nested object property.

#endregion

/// <summary>
/// Gets a property of an object in Request.Items and stores the property
/// value back to Request.Items.
/// </summary>
/// <remarks>
/// <para>Returned events:</para>
/// <c>ok</c> - the property value was successfully retrieved and stored in Request.Items.<br/>
/// <c>novalue</c> - there was no value for the property.<br/>
/// <c>error</c> - an error occurred while attempting to get the item's property.<br/>
/// </remarks>
///	<author>Scott Dyke</author>
///	<created>3/16/10</created>
public class GetObjectPropertyAsItemAction : IAction
{


	/// <summary>
	/// Gets or sets the name of the item in Request.Items to get the
	/// property from.
	/// </summary>
	public string ObjectItemNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the property to get.
	/// </summary>
	public string ObjectPropertyNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name in Request.Items into which the
	/// property value is stored.
	/// </summary>
	public string ItemNameOut
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
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(ObjectItemNameIn),
					"No item name given in the ObjectItemNameIn attribute.");
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(ObjectPropertyNameIn),
					"No object property name given in the ObjectPropertyNameIn attribute.");
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(ItemNameOut),
					"No item name given in the ItemNameOut attribute.");

			object obj = request.GetRequestItem<object>(ObjectItemNameIn, true);
			if (obj.GetType().Name.StartsWith("SearchResult")) {
				Array list = (Array)ReflectionUtilities.GetPropertyValue(obj, "Items");
				obj = list.GetValue(0);
			}

			string[] path = ObjectPropertyNameIn.Split('.');
			for (int k = 0; k < path.Length && obj != null; k++) {
				obj = ReflectionUtilities.GetPropertyValue(obj, path[k]);
			}
			object propVal = obj;

			if (propVal != null) {
				request.Items[ItemNameOut] = propVal;
				retEvent = Events.Ok;
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
			get
			{
				return EventNames.ERROR;
			}
		}
	}

	#endregion
}
}
