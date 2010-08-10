using System.Web.UI;

namespace Triton.Web.View.Controls
{
	public abstract class ConfigurableControl : BaseControl
	{
		/// <summary>
		/// Calls the base constructor to render the specified string tag.
		/// </summary>
		protected ConfigurableControl() {}


		/// <summary>
		/// Calls the base constructor to render the specified string tag.
		/// </summary>
		protected ConfigurableControl(string htmlTag) : base(htmlTag) {}


		/// <summary>
		/// Calls the base constructor to render the specified HtmlTextWriterTag tag.
		/// </summary>
		protected ConfigurableControl(HtmlTextWriterTag htmlTag) : base(htmlTag) {}
	}
}