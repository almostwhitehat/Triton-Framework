using System.Web.UI;
using System.Xml;

namespace Triton.Web.View.Controls
{
	public abstract class XmlBasedControl : ConfigurableControl
	{
		/// <summary>
		/// Calls the base constructor to render the specified string tag.
		/// </summary>
		protected XmlBasedControl() {}


		/// <summary>
		/// Calls the base constructor to render the specified string tag.
		/// </summary>
		protected XmlBasedControl(string htmlTag) : base(htmlTag) {}


		/// <summary>
		/// Calls the base constructor to render the specified HtmlTextWriterTag tag.
		/// </summary>
		protected XmlBasedControl(HtmlTextWriterTag htmlTag) : base(htmlTag) {}


		/// <summary>
		/// The xml node that contains all the information about the control.
		/// </summary>
		public XmlNode XmlNodeToRender { get; set; }
	}
}