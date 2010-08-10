using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Xml;

namespace Triton.Web.View.Controls
{
	/// <summary>
	///	Renders a piece of Copy from a matched node in the ContentXml file.
	/// </summary>
	public class Copy : XmlBasedControl
	{
		private string copyName;

		/// <summary>
		/// The name of the Copy element from the xml the control is to process.
		/// </summary>
		public String Name
		{
			get { return this.copyName; }
			set { this.copyName = value; }
		}


		/// <summary>
		/// Finds the Copy in the xml file and renders it to the page.
		/// </summary>
		/// <param name="writer"></param>
		protected override void RenderContents(HtmlTextWriter writer)
		{
			base.RenderContents(writer);
			try {
				string copyText;
		
				if (XmlNodeToRender == null) {
					copyText = this.Page.GetCopyContent(this.copyName);
				} else {
					copyText = this.Page.ParseCopy(XmlNodeToRender.InnerText);
				}

				writer.Write(copyText);
			} catch {
				//most likely did not find a node with the Copy name
				writer.Write("<!-- Error occured with Node name " + this.copyName + ". -->");
			}
		}

		/// <summary>
		/// Needed to override the span tag rendering of the web control, with nothing.
		/// </summary>
		/// <param name="writer"></param>
		public override void RenderBeginTag(HtmlTextWriter writer) {}


		/// <summary>
		/// Needed to override the span tag rendering of the web control, with nothing.
		/// </summary>
		/// <param name="writer"></param>
		public override void RenderEndTag(HtmlTextWriter writer) {}
	}
}