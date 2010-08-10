using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml;
using Common.Logging;

namespace Triton.Web.View.Controls
{
	/// <summary>
	/// Renders an image-based tag using a combination of the page's xml and tag properties from the page.
	/// </summary>
	public abstract class ImgBase : XmlBasedControl
	{
		/// <summary>
		/// List of attribute names NOT to be copied from the 
		/// definition in the XML file into the .net image control 
		/// </summary>
		protected List<string> attributesToExclude = new List<string>(new[] {"file", "mouseover", "global", "name", "src"});

		protected bool global;


		/// <summary>
		/// Calls the base constructor to render the specified string tag.
		/// </summary>
		protected ImgBase(string htmlTag) : base(htmlTag) {}


		/// <summary>
		/// Calls the base constructor to render the specified HtmlTextWriterTag.
		/// </summary>
		protected ImgBase(HtmlTextWriterTag htmlTag) : base(htmlTag) {}


		/// <summary>
		/// The name attribute of the Image node in the page's content xml file to use.
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Renders the controls attributes from the XmlNode.
		/// </summary>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			try {
				if (XmlNodeToRender == null) {
					XmlDocument contentDoc = Page.ContentXml;
					XmlNodeToRender = contentDoc.SelectSingleNode("//Images/Image[@name=\"" + this.Name + "\"]");
				}

				if (XmlNodeToRender.Attributes["global"] != null && XmlNodeToRender.Attributes["global"].Value == "true") {
					this.global = true;
				}

				string src = GetPath(PathType.CONTENT, XmlNodeToRender.Attributes["file"].Value, this.global, true);
				writer.AddAttribute(HtmlTextWriterAttribute.Src, src);

				//loop through all the attributes and add them to the Img tag, exept for the excluded ones
				for (int k = 0; k < XmlNodeToRender.Attributes.Count; k++) {
					XmlAttribute currentAttribute = XmlNodeToRender.Attributes[k];
					if (!this.attributesToExclude.Contains(currentAttribute.Name)) {
						writer.AddAttribute(currentAttribute.Name, currentAttribute.Value);
					}
				}
			} catch (Exception exception) {
				//usualy means that the node is not set up correctly
				LogManager.GetLogger(typeof (ImgBase)).Error(
					errorMessage => errorMessage("Node " + this.Name + " not found.", exception));

				writer.AddAttribute("error", "Node " + this.Name + " not found");
			}
		}
	}
}