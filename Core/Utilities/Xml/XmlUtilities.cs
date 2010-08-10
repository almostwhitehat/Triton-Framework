using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Triton.Utilities.Xml
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// </summary>
	///	<author>Scott Dyke</author>
	public class XmlUtilities
	{
		/// <summary>
		/// Populates the given objects attributes from the values in the given
		/// XML <c>Element</c>.  For each attribute attribute specified
		/// in the <c>Element</c>, it attempts to call the object's
		/// writeMethod, as defined by the BeanInfo for that object.<summary>
		///
		/// <param name="obj">the Object to populate.</param>
		/// <param name="node">the XmlNode containing the data to populate the Object from.</param>
		///
		public static void fillFromElement(
			Object obj,
			XmlNode node)
		{
			//  ==========  populate object from node attributes  ==========
			IEnumerator iter = node.Attributes.GetEnumerator();
			while (iter.MoveNext()) {
				XmlAttribute attr = (XmlAttribute) iter.Current;
				String attrName = attr.Name;
				String attrValueStr = attr.Value;

				//  set the corresponding attribute of the object
				setObjectAttribute(obj, attrName, attrValueStr);
			}

			//  get a list of all of the child nodes in the given node
//		XmlNodeList children = node.ChildNodes;

			//  ==========  populate object from node children  ==========
			//  for each child in the given node...
			iter = node.ChildNodes.GetEnumerator();
			while (iter.MoveNext()) {
				XmlElement attr = (XmlElement) iter.Current;
				String attrName = attr.Name;
				String attrValueStr = attr.InnerText;

				//  set the corresponding attribute of the object
				setObjectAttribute(obj, attrName, attrValueStr);
			}
		}


		public static void setObjectAttribute(
			Object obj,
			String attrName,
			String attrValue)
		{
			Type theType = obj.GetType();

			// look for the "set" method for the attribute
			MemberInfo[] mbrInfoArray = theType.FindMembers(MemberTypes.Method,
			                                                BindingFlags.Instance | BindingFlags.Public,
			                                                Type.FilterNameIgnoreCase,
			                                                "set" + attrName);

			if (mbrInfoArray.Length == 1) {
				MethodInfo method = (MethodInfo) mbrInfoArray[0];
				Object param = null;

				switch (Type.GetTypeCode(method.GetParameters()[0].ParameterType)) {
					case TypeCode.String:
						param = attrValue;
						break;

//  since these are all "Parse", can we dynamically create a instance of the type
//  and call it's Parse method?

					case TypeCode.Boolean:
						param = Boolean.Parse(attrValue);
						break;

					case TypeCode.Int16:
						param = Int16.Parse(attrValue);
						break;

					case TypeCode.Int32:
						param = Int32.Parse(attrValue);
						break;

					case TypeCode.Int64:
						param = Int64.Parse(attrValue);
						break;

					case TypeCode.DateTime:
						try {
							param = DateTime.Parse(attrValue);
						} catch (FormatException) {}
						break;

					case TypeCode.Decimal:
						param = Decimal.Parse(attrValue);
						break;

					case TypeCode.Double:
						param = Double.Parse(attrValue);
						break;

					case TypeCode.Single:
						param = Single.Parse(attrValue);
						break;
				}


				try {
					if (param != null) {
						//  invoke the "set" method for the attribute
						Object returnVal = method.Invoke(obj, new[]{param});
					}
				} catch (MissingMethodException e) {
//Console.WriteLine("<b>MissingMethodException:</b> " + e.ToString() + "<br>");
				}
			}
		}


		/**
	 * Returns the given String encoded within CDATA format to be safe for
	 * use in XML, if and only if it contains characters problematic when
	 * used in XML data.
	 *
	 * @param valToEncode	the string to encode.
	 *
	 * @return	the given string encoded as CDATA if it contains characters
	 *			not allowed in XML data, or the given string unchanged if
	 *			it contains no such characters.
	 */


		public static String cdataEncode(
			String valToEncode)
		{
			//  only encode if the string contains <, >, &, ', or \
			Regex regex = new Regex(".*[<>&'\"].*");
			if ((valToEncode != null) && regex.IsMatch(valToEncode)) {
				return "<![CDATA[" + valToEncode + "]]>";
			} else {
				return valToEncode;
			}
		}


		/**
	 * Returns the given String encoded for safe use within an XML attribute.
	 *
	 * @param valToEncode	the string to encode for an attribute value.
	 *
	 * @return	the given string encoded for safe use as an XML attribute.
	 */


		public static String attributeEncode(
			String valToEncode)
		{
			String retVal = valToEncode;

			if (valToEncode != null) {
				//  only encode if the string contains <, >, &, ', or \
				Regex regex = new Regex(".*[<&'\"].*");
				if (regex.IsMatch(valToEncode)) {
					retVal = retVal.Replace("<", "&lt");
					retVal = retVal.Replace("&", "&amp");
					retVal = retVal.Replace("'", "&apos");
					retVal = retVal.Replace("\"", "&quot");
				}
			}

			return retVal;
		}
	}
}