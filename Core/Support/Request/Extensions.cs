using System;
using System.Collections.Generic;
using Triton.Controller.Request;
using Triton.Model;
using Triton.Utilities.Reflection;

namespace Triton.Support.Request {


public static class Extensions
{


	/// <summary>
	/// Gets the item with the specified name from the given <c>MvcRequest</c>'s
	/// Items collection.  Extracts the item from a <c>SearchResult</c> if the
	/// item for the given name is a <c>SearchResult&lt;T&gt;</c>.  If <c>required</c>
	/// is <c>true</c> an exception is thrown if there is no value in the Items
	/// collection for the given name, or the object is not of type <c>T</c>.
	/// If <c>required</c> is <c>false</c> and there is no item with the given name,
	/// or the item is not of type T or <c>SearchResult&lt;T&gt;</c>,
	/// the type's default value is returned.
	/// </summary>
	/// <typeparam name="T">The type of the object to retrieve.</typeparam>
	/// <param name="request">The <c>MvcRequest</c> to retrieve the item from.</param>
	/// <param name="itemName">The name of the item in the request's Items collection to retrieve.</param>
	/// <param name="required">Indicates if the requested item is "required". If <c>true</c> error
	///			checking is performed to ensure that a value of the correct type is returned.</param>
	/// <returns>The item with the specified name from the given <c>MvcRequest</c>'s Items collection.</returns>
	public static T GetRequestItem<T>(
		this MvcRequest request,
		string itemName,
		bool required)
	{
		T retVal = default (T);

		if (request.Items[itemName] != null) {
					//  if the item is in a SearchResult, unwrap it
			if (request.Items[itemName] is SearchResult<T>) {
				SearchResult<T> sr = (SearchResult<T>)request.Items[itemName];
				if (sr.Items.Length > 0) {
					retVal = sr.Items[0];
				}
					// could be a search result but 
					// the type of containing object is not equal but may be a descendant
			} else if (request.Items[itemName].GetType().Name.StartsWith("SearchResult")) {
				// is the cintaining type a descendant of the type T
				Type type = request.Items[itemName].GetType();
				if (type.IsGenericType &&
					typeof(T).IsAssignableFrom(type.GetGenericTypeDefinition()))
				{
					object items = ReflectionUtilities.GetPropertyValue(request.Items[itemName], "Items");
					if (items.GetType().IsArray &&
						int.Parse(ReflectionUtilities.GetPropertyValue(items, "Length").ToString()) > 0)
					{
						retVal = (T)ReflectionUtilities.CallMethod(items, "GetValue", 0);
					}
				}
					//  not in a SearchResult, check for direct reference
			} else if (request.Items[itemName] is T) {
				retVal = (T)request.Items[itemName];

					//  type does not match, throw exception if needed
			} else if (required) {
				throw new TypeMismatchException(string.Format(
						"The type of the item in Request.Items[{0}] ({1}) does not match the requested type {2}.",
						itemName, request.Items[itemName].GetType().ToString(), typeof(T).ToString()));
			}
		} else if (required) {
			throw new NullReferenceException(string.Format("Request.Items[{0}] is null.", itemName));
		}

		return retVal;
	}
}
}
