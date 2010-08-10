using System.Web.UI;

namespace Triton.Web.View.Controls
{
	public class ConditionalComment : BaseControl
	{
		private string condition = "";


		public ConditionalComment(string condition)
		{
			this.condition = condition;
		}


		public string Condition
		{
			get { return this.condition; }
			set { this.condition = value; }
		}


		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			writer.Write("<!--[" + this.condition + "]>");
		}


		public override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.Write("<![endif]-->");
		}
	}
}