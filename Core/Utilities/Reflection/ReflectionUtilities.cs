using System;
using System.Collections.Specialized;
using System.Reflection;

namespace Triton.Utilities.Reflection
{

	#region History

	// History:
	// 12/28/09 - SD  - Updated Deserialize to handle Guids. Because Guid does not have a Parse
	//					method and Guid properties can't be directly set to a string value, a special
	//					condition needed to be added for Guids.
	//  1/19/10 - SD  - Updated the Guid special condition in Deserialize to also handle nullable Guids.

	#endregion

	/// <summary>
	/// ReflectionUtilities is a utility class with useful methods for reflection support.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ReflectionUtilities
	{
		public static bool HasProperty(
			object obj,
			string propertyName)
		{
			return HasProperty(obj, propertyName, null);
		}


		public static bool HasProperty(
			object obj,
			string propertyName,
			Type propertyType)
		{
			PropertyInfo propInfo =
				propertyType == null
					? GetProperty(obj, propertyName)
					: GetProperty(obj, propertyName, propertyType);

			return (propInfo != null);
		}


		public static PropertyInfo GetProperty(
			object obj,
			string propertyName)
		{
			return GetProperty(obj, propertyName, null);
		}


		public static PropertyInfo GetProperty(
			object obj,
			string propertyName,
			Type propertyType)
		{
			PropertyInfo ret =
				propertyType == null
					? obj.GetType().GetProperty(propertyName)
					: obj.GetType().GetProperty(propertyName, propertyType);

			return ret;
		}


		public static object GetPropertyValue(
			object obj,
			string propertyName)
		{
			return GetPropertyValue(obj, propertyName, null);
		}


		public static object GetPropertyValue(
			object obj,
			string propertyName,
			Type propertyType)
		{
			object retVal = null;

			if (obj != null) {
				PropertyInfo propInfo =
					propertyType == null
						? obj.GetType().GetProperty(propertyName)
						: obj.GetType().GetProperty(propertyName, propertyType);

				if (propInfo != null) {
					retVal = propInfo.GetValue(obj, null);
				}
			}

			return retVal;
		}


		public static void SetPropertyValue(
			object obj,
			string propertyName,
			object val)
		{
			SetPropertyValue(obj, propertyName, val, null);
		}


		public static void SetPropertyValue(
			object obj,
			string propertyName,
			object val,
			Type propertyType)
		{
			if (obj != null) {
				PropertyInfo propInfo =
					propertyType == null
						? obj.GetType().GetProperty(propertyName)
						: obj.GetType().GetProperty(propertyName, propertyType);

				if (propInfo != null) {
					propInfo.SetValue(obj, val, null);
				}
			}
		}


		/// <summary>
		/// Determines if the given object has a method with the specified name and
		/// parameter types.
		/// </summary>
		/// <param name="obj">The object to check for the existance of the method.</param>
		/// <param name="methodName">The name of the method to check for.</param>
		/// <param name="types">The type(s) of the parameters of the method to check for.</param>
		/// <returns><b>True</b> if the object has the specified method, <b>false</b>
		///			if not.</returns>
		public static bool HasMethod(
			object obj,
			string methodName,
			params Type[] types)
		{
			MethodInfo meth = GetMethod(obj, methodName, types);

			return (meth != null);
		}


		/// <summary>
		/// Get the method of the given object with the specified name and
		/// parameter types.
		/// </summary>
		/// <param name="obj">The object to get the method for.</param>
		/// <param name="methodName">The name of the method to get.</param>
		/// <param name="types">The type(s) of the parameters of the method to get.</param>
		/// <returns>The <b>MethodInfo</b> of the specified method, or <b>null</b> if
		///			the method is not found in the given object.</returns>
		public static MethodInfo GetMethod(
			object obj,
			string methodName,
			params Type[] types)
		{
			MethodInfo meth = obj.GetType().GetMethod(methodName, types);

			return meth;
		}


		/// <summary>
		/// Calls the specified method of the given object, if it exists, and retruns
		/// the value returned by the called method.
		/// </summary>
		/// <param name="obj">The object to call the method on.</param>
		/// <param name="methodName">The name of the method to call.</param>
		/// <param name="parms">The parameter(s) to pass to the method.</param>
		/// <returns>The value returned by the called method.</returns>
		public static object CallMethod(
			object obj,
			string methodName,
			params object[] parms)
		{
			object retObject = null;
			Type[] types = new Type[parms.Length];

			//  get the types of the given parameters
			for (int k = 0; k < parms.Length; k++) {
				types[k] = parms[k].GetType();
			}

			//  get the method
			MethodInfo meth = GetMethod(obj, methodName, types);

			//  invoke the method if it exists
			if (meth != null) {
				retObject = meth.Invoke(obj, parms);
			}

			return retObject;
		}


		/// <summary>
		/// Deserializes an object by setting it's properties based on the names and
		/// values specified in the given <b>NameValueCollection</b>.
		/// </summary>
		/// <remarks>
		/// The names in the NameValueCollection are the names of the properties to be
		/// set.  Property names are assumed to start with an uppercase letter but after
		/// that the case fo the name in the NameValueCollection must match the property.
		/// </remarks>
		/// <param name="obj">The object to be deserialized.</param>
		/// <param name="attributes">A <b>NameValueCollection</b> containing the names of
		/// the properties to set and the values to set them to.</param>
		public static void Deserialize(
			object obj,
			NameValueCollection attributes)
		{
			for (int k = 0; k < attributes.Count; k++) {
				string attrName = attributes.Keys[k];
				string attrValue = attributes[k];

				// if the attribute name is empty or null then ignore it.
				if (string.IsNullOrEmpty(attrName)) {
					//throw new NullReferenceException(string.Format("Missing attribute name at index {0}.", k));
					continue;
				}

				string propertyName = StringUtilities.Capitalize(attrName);

				//  get the property of the object that matches the attribute name
				PropertyInfo property = GetProperty(obj, propertyName);

				if (property != null) {
					object propertyValue = null;

					//  check to see if the property type is an enum
					if (property.PropertyType.IsEnum) {
						//  get the Parse method of the property's type
						MethodInfo method = property.PropertyType.BaseType.GetMethod(
							"Parse",
							new[]
							{
								Type.GetType("System.Type"),
								"".GetType(),
								true.GetType()
							});

						//  call the Parse method to get the value
						if ((method != null) && (method.IsStatic)) {
							propertyValue = method.Invoke(
								null,
								new object[]
								{
									property.PropertyType,
									attrValue,
									true
								});
						}

							//  if the property type is directly assignable from a string, assign the value
							//  TODO: is there a dynamic way to determine if the property can be directly assigned a string?
					} else if (property.PropertyType.FullName == typeof(string).FullName) {
						propertyValue = attrValue;

							//  Guids can't be assigned a string and Guid doesn't have a Parse method, so it
							//  is a special case -- TODO: is there a more general way to handle Guids?
					} else if ((property.PropertyType.FullName == typeof(Guid).FullName)
							|| (property.PropertyType.Name.StartsWith("Nullable")
								&& (Nullable.GetUnderlyingType(property.PropertyType).Name == typeof(Guid).Name))) {
								//  if we can't generate a new Guid for the value, set the attrValue to null
								//  a direct assignment attempt is not made below
						try {
							propertyValue = new Guid(attrValue);
						} catch {
							attrValue = null;
						}

					} else {
						//  attempt to get the Parse method for the property's type
						MethodInfo method = property.PropertyType.GetMethod("Parse", new[] {"".GetType()});

						//  if we failed to get the Parse method, see if the propoerty's 
						//  type is Nullable and try with the underlying type
						if ((method == null) && property.PropertyType.Name.StartsWith("Nullable")) {
							Type type = Nullable.GetUnderlyingType(property.PropertyType);
							method = type.GetMethod("Parse", new[] {"".GetType()});
						}

						if ((method != null) && (method.IsStatic)) {
							try {
								propertyValue = method.Invoke(
									null,
									new object[] {attrValue});
							} catch {}
						}
					}

					//  determine if we should try to set the property value or use a set method
					bool useSetMethod = false;
					if ((propertyValue == null) || !property.CanWrite) {
						useSetMethod = true;
						propertyValue = attrValue;
					}

					try {
						if (propertyValue != null) {
							// if we should use the set method and it exists, call it
							if (useSetMethod && HasMethod(obj, "Set" + propertyName, "".GetType())) {
								CallMethod(obj, "Set" + propertyName, propertyValue);

								// set the property value directly
							} else if (property.CanWrite) {
								property.SetValue(obj, propertyValue, null);
							}

							//method = property.GetSetMethod();
							//method.Invoke(obj, new object[]{propertyValue});
						}
					} catch (ArgumentException ae) {
						property.SetValue(obj, null, null);
					}
				}
			}
		}
	}
}