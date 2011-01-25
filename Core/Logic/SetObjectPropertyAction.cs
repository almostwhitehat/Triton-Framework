using System;
using System.Reflection;
using System.Collections.Specialized;
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
/// Action to set the property of an object.
/// </summary>
/// <remarks>
/// The value to set the property to may be specified in one of 3 ways:
/// <ol>
/// <li>The value of a request parameter.</li>
/// <li>Another object from Request.Items.</li>
/// <li>A fixed value.</li>
/// </ol>
/// <para>Returned events:</para>
/// <c>ok</c> - the property of the object was successfully set.<br/>
/// <c>error</c> - an error occurred processing the action.<br/>
/// </remarks>
///	<author>Scott Dyke</author>
///	<created>6/28/10</created>
public class SetObjectPropertyAction : IAction
{
	protected const string	NULL_VALUE		= "[null]";


	/// <summary>
	/// Gets or sets the name of the item in Request.Items containing the
	/// object to set the property of.
	/// </summary>
	public string ObjectItemNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the property of the object to set.
	/// </summary>
	public string ObjectPropertyNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or set the name of the request parameter containing the
	/// value to set the property to.
	/// </summary>
	public string ValueParamNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the item in Request.Items to set
	/// the property value to.
	/// </summary>
	public string ValueItemNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets a fixed value to set the property to.
	/// </summary>
	public string ValueIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the flag indicating whether or not the action should
	/// create intermediate objects in the path to the property to be set.
	/// </summary>
	public bool CreateObjects
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the flag indicating whether a value coming from Request.Items should be
	/// unwrapped from an enclosing <c>SearchResult</c> if it is in one. This is only valid
	/// when using the <c>ValueItemNameIn</c> option.
	/// </summary>
	public bool Unwrap
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the flag indicating whether a value coming from Request.Items should be
	/// unwrapped from an enclosing <c>SearchResult</c> as the <c>SearchResult.Items</c> array
	/// if it is in one. This is only valid when using the <c>ValueItemNameIn</c> option.
	/// </summary>
	public bool UnwrapAsArray
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
			ActionContract.Requires<ApplicationException>(!(string.IsNullOrEmpty(ValueParamNameIn) && string.IsNullOrEmpty(ValueItemNameIn) && string.IsNullOrEmpty(ValueIn)),
					"No  name given for the value. One of ValueParamNameIn, ValueItemNameIn, or ValueIn must be provided.");

			object obj = request.GetRequestItem<object>(ObjectItemNameIn, true);

			if (obj == null) {
				throw new NullReferenceException(string.Format("No item found in Request.Items[{0}].", ObjectItemNameIn));
			}

			if (obj.GetType().Name.StartsWith("SearchResult")) {
				Array list = (Array)ReflectionUtilities.GetPropertyValue(obj, "Items");
				obj = list.GetValue(0);
			}

					//  loop thru the path to the lowest level object
			string[] path = ObjectPropertyNameIn.Split('.');
			string currentPath = "";
			for (int k = 0; k < path.Length - 1; k++) {
				currentPath += ((currentPath.Length == 0) ? "" : ".") + path[k];

				if (!ReflectionUtilities.HasProperty(obj, path[k])) {
					throw new MissingMemberException(string.Format(
							"The object (from Request.Items[{0}]) of type {1} does not contain a property named '{3}'.",
							ObjectItemNameIn, obj.GetType().Name, path[k]));
				}

				object newObj = ReflectionUtilities.GetPropertyValue(obj, path[k]);

				if (newObj == null) {
					if (CreateObjects) {
						PropertyInfo propInfo = ReflectionUtilities.GetProperty(obj, path[k]);
						newObj = Activator.CreateInstance(propInfo.PropertyType);
						ReflectionUtilities.SetPropertyValue(obj, path[k], newObj);
					} else {
						throw new NullReferenceException(string.Format(
								"'{0}' is null in {1} of Request.Items[{2}].",
								currentPath, ObjectPropertyNameIn, ObjectItemNameIn));
					}
				}

				obj = newObj;
			}

			object value = null;
			if (!string.IsNullOrEmpty(ValueIn)) {
				if (ValueIn.ToLower() != NULL_VALUE) {
					value = ValueIn;
				}
			} else if (!string.IsNullOrEmpty(ValueParamNameIn)) {
				value = request[ValueParamNameIn];
			} else if (!string.IsNullOrEmpty(ValueItemNameIn)) {
				value = request.GetRequestItem<object>(ValueItemNameIn, true);

						//  if the item from the request is wrapped in a "SearchResult"
						//  and we're instructed to unwrap it, get the first item from 
						//  SearchResult.Items, or the SearchResult.Items array
				if (value.GetType().Name.StartsWith("SearchResult")) {
					if (Unwrap) {
						value = ((object[])ReflectionUtilities.GetPropertyValue(value, "Items")).GetValue(0);
					} else if (UnwrapAsArray) {
						value = ReflectionUtilities.GetPropertyValue(value, "Items");
					}
				}
			}

			PropertyInfo propertyInfo = ReflectionUtilities.GetProperty(obj, path[path.Length - 1]);
			if ((value == null) || (propertyInfo.PropertyType == value.GetType()) || propertyInfo.PropertyType.IsAssignableFrom(value.GetType())) {
				ReflectionUtilities.SetPropertyValue(obj, path[path.Length - 1], value);
			} else {
				NameValueCollection nvc = new NameValueCollection();
				nvc.Add(path[path.Length - 1], value.ToString());
				ReflectionUtilities.Deserialize(obj, nvc);
			}
			
			retEvent = Events.Ok;
		} catch (Exception e) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error(error => error("Error setting object property value."), e);
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
