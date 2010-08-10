using System;
using System.Text;
using System.Xml;
using Triton.Utilities.Xml;

namespace Triton.Support.Error
{

	#region History

	// History:

	#endregion

	/// <summary></summary>
	///	<author>Scott Dyke</author>
	public class Error : Xmlable
	{
		#region ErrorType enum

		public enum ErrorType
		{
			NOERROR,
			WARNING,
			DBSANITY,
			VALIDATION,
			INITIALIZATION,
			FATAL
		}

		#endregion

		protected String formField;
		protected long id;
		protected String message;
		protected ErrorType type = ErrorType.NOERROR;


		public Error() {}


		public Error(
			Error err)
		{
			this.id = err.Id;
			this.formField = err.FormField;
			this.message = err.Message;
			this.type = err.Type;
		}


		public Error(
			long id,
			String formField,
			String msg,
			ErrorType type)
		{
			this.id = id;
			this.formField = formField;
			this.message = msg;
			this.type = type;
		}


		public long Id
		{
			get { return this.id; }
			set { this.id = value; }
		}


		public String FormField
		{
			get { return this.formField; }
			set { this.formField = value; }
		}


		public String Message
		{
			get { return this.message; }
			set { this.message = value; }
		}


		public ErrorType Type
		{
			get { return this.type; }
			set { this.type = value; }
		}

/* Commented out - this needs to change since the get and set methods changed
	/**
	 * Populates the error information from the given XML Element.
	 *
	 * @param element	the XML <code>Element</code> to populate the
	 *					Error information from.
	 * /
	public void fill(
		XmlNode	element)
	{
		XmlUtilities.fillFromElement(this, element);
	}
*/

		#region Xmlable Members

		public void fill(
			XmlNode element) {}


		/**
	 * Converts the <code>Error</code> attributes to XML. 
	 *
	 * @return	the attributes of the <code>Error</code> as XML.
	 */


		public String toXML()
		{
			StringBuilder xml = new StringBuilder();

			xml.Append("<Error id=\"");
			xml.Append(this.id);
			xml.Append("\" type=\"");
			xml.Append(Enum.GetName(typeof(ErrorType), this.type));
			xml.Append("\">");
			if (this.message != null) {
				xml.Append(XmlUtilities.cdataEncode(this.message));
			}
/*
		xml.Append("<Error>");
		xml.Append("<Id>" + this.id + "</Id>");
		if (this.message != null) {
			xml.Append("<Message>" + XmlUtilities.cdataEncode(this.message) + "</Message>");
		}
*/
			xml.Append("</Error>");

			return xml.ToString();
		}

		#endregion
	}
}