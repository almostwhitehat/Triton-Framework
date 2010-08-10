using System;
using System.Collections.Specialized;
using System.Xml;

namespace Triton.Utilities.Configuration
{

	#region History

	// History:

	#endregion

	public class XmlConfiguration : XmlDocument
	{
		/// <summary>
		/// Returns an <c>XmlConfiguration</c> object containing the child nodes specified by the input parameters
		/// </summary>
		/// <remarks>
		/// <P>This method looks under the current node context for a node named <c>section</c>. If found,
		/// it then looks for any child elements that contain a node with an attribute called <c>name</c> that
		/// have a value as defined by the <c>name</c> parameter. If found, then the <c>section</c> node
		/// is returned. If either the <c>section</c> node or the subnode with the <c>name</c> attribute
		/// value are not found, <c>Null</c> is returned</P>
		/// <P><B>Note:</B> if multiple matches are possible in the XML, only the first match is returned.</P></remarks>
		/// <param name="section"><c>String</c> that specifies the XML node name to search</param>
		/// <param name="name"><c>String</c> that specifies the value of the name attribute to match</param>
		/// <returns><c>XmlConfiguration</c> object with the identified XML node as the root node, or <c>Null</c>
		/// if not found</returns>
		public XmlConfiguration GetConfig(
			String section,
			String name)
		{
			XmlConfiguration retConfig = null;

			String xpath = ".//" + section + "/*[@name=\"" + name + "\"]";
//System.Diagnostics.Debug.WriteLine("XmlConfiguration.GetConfig: xpath= " + xpath);

			XmlNode node = SelectSingleNode(xpath);

			if (node != null) {
				retConfig = new XmlConfiguration();
				retConfig.LoadXml(node.OuterXml);
//System.Diagnostics.Debug.WriteLine("XmlConfiguration.GetConfig: xml= " + node.OuterXml);
			}

			return retConfig;
		}


		/// <summary>
		/// Returns the value of the specified XML node, or <c>Null</c> if the node is not found
		/// </summary>
		/// <remarks>If the xpath is not properly formed, a null string is returned</remarks>
		/// <param name="xpath"><c>Xpath</c> that specifies the XML node to retrieve</param>
		/// <returns><c>String</c> that contains the value of the XML tag requested</returns>
		public String GetValue(
			String xpath)
		{
			String retVal = null;
			XmlNode node = SelectSingleNode(xpath);

			if (node != null) {
				retVal = node.InnerText;
			}

			return retVal;
		}


		/// <summary>
		/// Returns all attributes from a specified <c>xpath</c>
		/// </summary>
		/// <remarks>The atributes are returned in a <c>NameValueCollection</c> object, one entry for each
		/// attribute.</remarks>
		/// <param name="xpath"><c>Xpath</c> that specifies the XML node to retrieve the attributes from</param>
		/// <returns><c>NameValueCollection</c> containing the name/value pairs of the attributes</returns>
		public NameValueCollection GetAttributes(
			String xpath)
		{
			NameValueCollection retColl = null;
			XmlNode node = SelectSingleNode(xpath);

			if (node != null) {
				XmlAttributeCollection tmpColl = node.Attributes;

				if (tmpColl != null) {
					retColl = new NameValueCollection(tmpColl.Count);
					foreach (XmlAttribute tmpAttr in tmpColl) {
						retColl.Add(tmpAttr.Name, tmpAttr.Value);
					}
				}
			}

			return retColl;
		}
	}
}