using System;
using System.Xml;

namespace Triton.Utilities.Xml
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The Xmlable interface defines the interface for objects that can
	/// be converted to and from XML.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface Xmlable
	{
		void fill(
			XmlNode element);


		String toXML();
	}
}