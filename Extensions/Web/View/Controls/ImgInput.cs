using System.Web.UI;

namespace Triton.Web.View.Controls
{
	/// <summary>
	/// Renders an input tag of type image drawing tag values from Content xml .
	/// </summary>
	public class ImgInput : ImgBase
	{
		/// <summary>
		/// Calls base to render the input tag.
		/// </summary>
		public ImgInput() : base(HtmlTextWriterTag.Input) {}


		/// <summary>
		/// Calls base to render the standard attributes then adds image input tag attributes.
		/// </summary>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "image");
		}
	}
}