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

#endregion

/// <summary>
/// Action to compare the value of a request item to a given value.
/// </summary>
/// <remarks>
/// <para>Returned events:</para>
/// <c>equal</c> - the object/property's value is equal to the given value.<br/>
/// <c>lessthan</c> - the object/property's value is less than the given value.<br/>
/// <c>greaterthan</c> - the object/property's value is greater than the given value.<br/>
/// <c>novalue</c> - a property value in the object's property path was null.<br/>
/// <c>error</c> - an error occurred while attempting to compare object's value.<br/>
/// </remarks>
///	<author>Scott Dyke</author>
///	<created>7/12/10</created>
public class CompareAction : IAction
{

	/// <summary>
	/// Default constructor to set the property defaults.
	/// </summary>
	public CompareAction()
	{
		Unwrap = true;
	}


	/// <summary>
	/// Gets or sets the name of the item in Request.Items containing the object
	/// to use to compare against the given value. The object itself is used for
	/// the comparison if no <c>ObjectPropertyNameIn</c> is provided, or a property
	/// (or nested property) of the object if <c>ObjectPropertyNameIn</c> is provided.
	/// </summary>
	public string ObjectItemNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the property of the object identified by the
	/// <c>ObjectItemNameIn</c> attribute to be compared. The property may be of a
	/// nested object where <c>ObjectPropertyNameIn</c> would be the path of the
	/// desired property, for example, PropertyA.PropertyB.PropertyC. If
	/// <c>ObjectPropertyNameIn</c> is not provided, the object identified by
	/// <c>ObjectItemNameIn</c> is used directly.
	/// </summary>
	public string ObjectPropertyNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the value to compare the object/property value against.
	/// This may be the name of a request parameter specified by placing the
	/// parameter name within [ ], for example, <c>[paramName]</c>.
	/// </summary>
	public string CompareValueIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the flag to indicate whether or not to "unwrap" an object
	/// from a <c>SearchResult</c> if encapsulated within one.
	/// </summary>
	public bool Unwrap
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
//			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(ObjectPropertyNameIn),
//					"No object property name given in the ObjectPropertyNameIn attribute.");
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(CompareValueIn),
					"No value to compare to given in the CompareValueIn attribute.");

//			object obj = request.GetRequestItem<object>(ObjectItemNameIn, true);
			object obj = request.Items[ObjectItemNameIn];
			if (obj == null) {
				retEvent = Events.NoValue;
			} else {
				if (Unwrap && obj.GetType().Name.StartsWith("SearchResult")) {
					Array list = (Array)ReflectionUtilities.GetPropertyValue(obj, "Items");
					obj = list.GetValue(0);
				}

				if (!string.IsNullOrEmpty(ObjectPropertyNameIn)) {
					//  loop thru the path of the ObjectPropertyNameIn to the lowest level property value
					string[] path = ObjectPropertyNameIn.Split('.');
					for (int k = 0; k < path.Length && obj != null; k++) {
						obj = ReflectionUtilities.GetPropertyValue(obj, path[k]);
					}
				}

						//  if the property value is null, return the no value event
				if (obj == null) {
					retEvent = Events.NoValue;

				} else {
					object compareValue = CompareValueIn; ;
							//  if the compare value is enclosed within [ ], it means get the value
							//  from the request parameter
					if (CompareValueIn.StartsWith("[") && CompareValueIn.EndsWith("]")) {
						string paramName = CompareValueIn.Substring(1, CompareValueIn.Length - 2);
						if (request[paramName] == null) {
							throw new NullReferenceException(string.Format(
									"No request parameter '{0}' found.", paramName));
						}
						compareValue = request[paramName];
					}

					if (!(obj is string)) {
						//if (!ReflectionUtilities.HasMethod(obj, "Parse")) {
						//    throw new MissingMethodException(string.Format(
						//            "The type '{0}' does not contain a Parse method.", obj.GetType().FullName));
						//}
						//compareValue = ReflectionUtilities.CallMethod(obj, "Parse", CompareValueIn);
						compareValue = Convert.ChangeType(compareValue, obj.GetType());
					}

					if (!ReflectionUtilities.HasMethod(obj, "CompareTo", obj.GetType())) {
						throw new MissingMethodException(string.Format(
								"The type '{0}' does not contain a CompareTo method.", obj.GetType().FullName));
					}
					int result = (int)ReflectionUtilities.CallMethod(obj, "CompareTo", compareValue);

					if (result == 0) {
						retEvent = Events.Equal;
					} else if (result < 0) {
						retEvent = Events.LessThan;
					} else {
						retEvent = Events.GreaterThan;
					}
				}
			}

		} catch (Exception e) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error(error => error("Error occurred comparing object value."), e);
		}

		return retEvent;
	}

	#endregion


	#region Nested type: Events

	public class Events
	{
		public static string Equal
		{
			get {
				return EventNames.EQUAL;
			}
		}

		public static string LessThan
		{
			get {
				return EventNames.LESS_THAN;
			}
		}

		public static string GreaterThan
		{
			get {
				return EventNames.GREATER_THAN;
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
