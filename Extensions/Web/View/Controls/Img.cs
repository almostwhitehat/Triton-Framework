using System.Web.UI;

namespace Triton.Web.View.Controls
{
	/// <summary>
	/// Renders an image tag.
	/// </summary>
	public class Img : ImgBase
	{
		/// <summary>
		/// Calls the base constructor to render Img tag on the page.
		/// </summary>
		public Img() : base(HtmlTextWriterTag.Img) {}
	}
}