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


/// <summary>
/// Gets a property of an object in Request.Items and sets a request parameter
/// to the property value.
/// </summary>
/// <remarks>
/// <para>Returned events:</para>
/// <c>ok</c> - the property value was successfully retrieved and stored in a parameter.<br/>
/// <c>novalue</c> - there was no value for the property.<br/>
/// <c>error</c> - an error occurred while attempting to get the item's property.<br/>
/// </remarks>
///	<author>Scott Dyke</author>
///	<created>6/30/10</created>
public class GetObjectPropertyAsParameterAction : IAction
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
	/// Gets or sets the name of the request parameter to store the
	/// property value in.
	/// </summary>
	public string ParameterNameOut
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
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(ParameterNameOut),
					"No parameter name given in the ParameterNameOut attribute.");

			object obj = request.GetRequestItem<object>(ObjectItemNameIn, true);
			if (obj.GetType().Name.StartsWith("SearchResult")) {
				Array list = (Array)ReflectionUtilities.GetPropertyValue(obj, "Items");
				obj = list.GetValue(0);
			}

					//  loop thru the path to the lowest level property
			string[] path = ObjectPropertyNameIn.Split('.');
			for (int k = 0; k < path.Length && obj != null; k++) {
				obj = ReflectionUtilities.GetPropertyValue(obj, path[k]);
			}
			object propVal = obj;

			if (propVal != null) {
				request[ParameterNameOut] = propVal.ToString();
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
