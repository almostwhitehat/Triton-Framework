using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;
using Triton.Logging;
using Triton.Utilities;
using Triton.Web.Support;

namespace Triton.Web.View.Controls
{
	public abstract class CssBase : XmlBasedControl
	{
		/// <summary>
		/// List of attribute names NOT to be copied from the 
		/// definition in the XML file into the .net image control 
		/// also contains the defaulted attributes
		/// </summary>
		protected List<string> attributesToExclude =
			new List<string>(new[] {
			                       	"sortorder", "ieCondition", "href", "section", "name", "folders", "src", "rel", "media",
			                       	"type", "default"
			                       });


		private void LoadNodes()
		{
			if (XmlNodeToRender == null) {
				XmlDocument contentXml = Page.ContentXml;
				XmlNodeList cssList = contentXml.SelectNodes("//Style/Css");
				List<XmlNode> nodeList = this.SortNodeList(cssList);
				XmlNodeToRender = contentXml.CreateElement("Style");

				foreach (XmlNode cssNode in nodeList) {
					if (this.IsStyleSheetNeeded(cssNode)) {
						XmlNodeToRender.AppendChild(cssNode);
					}
				}
			}
		}


		public abstract bool IsStyleSheetNeeded(XmlNode node);


		public virtual List<XmlNode> SortNodeList(XmlNodeList list)
		{
			List<XmlNode> nodeList = new List<XmlNode>();
			int nodeSortOrder = 50;
			foreach (XmlNode node in list) {
				if (node.Attributes["sortorder"] == null) {
					node.Attributes.Append(Page.ContentXml.CreateAttribute("sortorder"));
					node.Attributes["sortorder"].Value = nodeSortOrder.ToString();
					nodeSortOrder++;
				}
				nodeList.Add(node);
			}
			nodeList.Sort(new XmlNodeSortOrderComparer());
			return nodeList;
		}


		protected override void CreateChildControls()
		{
			this.LoadNodes();
			if (XmlNodeToRender.HasChildNodes) {
				foreach (XmlNode cssNode in XmlNodeToRender.ChildNodes) {
					if (cssNode.Attributes["ieCondition"] != null && cssNode.Attributes["ieCondition"].Value != "") {
						string ieCondition = cssNode.Attributes["ieCondition"].Value;
						ConditionalComment condition = new ConditionalComment(ieCondition);
						condition.Controls.Add(this.CreateLinkControl(cssNode));
						Controls.Add(condition);
					} else {
						Controls.Add(this.CreateLinkControl(cssNode));
					}
				}
			}

			base.CreateChildControls();
		}


		protected virtual Control CreateLinkControl(
			XmlNode cssNode)
		{
			HtmlLink css = new HtmlLink();
			CssType type = CssType.STANDARD;
			string href = "";
			string media;

			css.Attributes.Add("type", "text/Css");

			string relAttribute = "stylesheet";
			if (cssNode.Attributes["rel"] != null && cssNode.Attributes["rel"].Value != "") {
				relAttribute = cssNode.Attributes["rel"].Value;
			}

			css.Attributes.Add("rel", relAttribute);

			if (cssNode.Attributes["media"] != null && cssNode.Attributes["media"].Value != "") {
				media = cssNode.Attributes["media"].Value;
			} else {
				media = "screen";
			}

			css.Attributes.Add("media", media);

			if (cssNode.Attributes["href"] != null && cssNode.Attributes["href"].Value != "") {
				string tmpHref = cssNode.Attributes["href"].Value;

				if (tmpHref.Contains(".aspx")) {
					try {
						href = StringUtilities.Split(tmpHref, ".aspx")[0];
						type = CssType.COMMAND_DEPENDENT;
					} catch (IndexOutOfRangeException exception) {
						//should never happen, but just in case
						Logger.GetLogger("View").Error("While parsing href in Css, could not get the Css file name.", exception);
					}
				} else {
					href = tmpHref;
				}
			}

			if (cssNode.Attributes["src"] != null && cssNode.Attributes["src"].Value != "") {
				string src = cssNode.Attributes["src"].Value;

				if (src.Contains(".aspx")) {
					type = CssType.COMMAND_DEPENDENT;
				} else {
					href = src;
					type = CssType.LEGACY;
				}
			}

			switch (type) {
				case CssType.LEGACY:
					href = this.BuildLegacyHref(href);
					break;
				case CssType.COMMAND_DEPENDENT:
					href = this.BuildCommandDependentHref(cssNode);
					break;
				case CssType.STANDARD:
					href = this.BuildStandardHref(cssNode);
					break;
			}

			css.Attributes.Add("href", href);

			foreach (XmlAttribute attr in cssNode.Attributes) {
				if (!this.attributesToExclude.Contains(attr.Name)) {
					css.Attributes.Add(attr.Name, attr.Value);
				}
			}

			return css;
		}


		protected virtual string BuildLegacyHref(
			string href)
		{
			//call the bases get path method
			string temp = GetPath(PathType.PAGES, href, false, false);
			return temp;
		}


		protected virtual string BuildCommandDependentHref(
			XmlNode cssNode)
		{
			string section = Page.Section;
			string href;

			if (cssNode.Attributes["src"] != null && cssNode.Attributes["src"].Value != "") {
				string src = cssNode.Attributes["src"].Value;

				//get the name of the Css out of the string
				int pIndex = src.IndexOf("p=");
				string cssName = src.Substring(pIndex, (src.IndexOf('&', pIndex) - pIndex));
				//returns p=..., remove the p=, assign to href for further processing
				href = cssName.Remove(0, 2);
				//get the section of the Css
				int sIndex = src.IndexOf("s=");
				string cssSection = src.Substring(sIndex, (src.IndexOf('&', sIndex) - sIndex));
				section = cssSection.Remove(0, 2);
			} else {
				href = StringUtilities.Split(cssNode.Attributes["href"].Value, ".aspx")[0];
			}

			if (cssNode.Attributes["section"] != null && cssNode.Attributes["section"].Value != "") {
				section = cssNode.Attributes["section"].Value;
			}

			if (cssNode.Attributes["folders"] != null && cssNode.Attributes["folders"].Value != "") {
				section += "/" + cssNode.Attributes["folders"].Value;
			}

			string command = String.Format(WebInfo.Controller + "?t=page&a=go&p={0}&s={1}&site={2}", href, section, Page.Site);
			return command;
		}


		protected virtual string BuildStandardHref(
			XmlNode cssNode)
		{
			string href = "";
			if (cssNode.Attributes["href"] != null && cssNode.Attributes["href"].Value != "") {
				href = cssNode.Attributes["href"].Value;
			}

			bool useDefault = true;
			if (cssNode.Attributes["default"] != null && cssNode.Attributes["default"].Value != "") {
				useDefault = (cssNode.Attributes["default"].Value == "false") ? false : true;
			}
			string site = (useDefault) ? "default" : Page.Site;

			string sectionName = "";
			if (cssNode.Attributes["section"] != null && cssNode.Attributes["section"].Value != "") {
				sectionName = cssNode.Attributes["section"].Value;
			}
			string section = (sectionName != "") ? sectionName : Page.Section;


			if (cssNode.Attributes["folders"] != null && cssNode.Attributes["folders"].Value != "") {
				string folders = cssNode.Attributes["folders"].Value;
				section += "/" + folders;
			}

			return string.Format("/{0}v{1}/{2}/{3}/{4}", WebInfo.ContentPath, Page.Version, site, section, href);
		}


		/// <summary>
		/// Needed to override the span tag rendering of the web control, with nothing.
		/// </summary>
		/// <param name="writer"></param>
		public override void RenderBeginTag(
			HtmlTextWriter writer) {}


		/// <summary>
		/// Needed to override the span tag rendering of the web control, with nothing.
		/// </summary>
		/// <param name="writer"></param>
		public override void RenderEndTag(
			HtmlTextWriter writer) {}

		#region Nested type: CssType

		protected enum CssType
		{
			COMMAND_DEPENDENT,
			LEGACY,
			STANDARD
		}

		#endregion

		#region Nested type: XmlNodeSortOrderComparer

		private class XmlNodeSortOrderComparer : IComparer<XmlNode>
		{
			#region IComparer<XmlNode> Members

			public int Compare(
				XmlNode x,
				XmlNode y)
			{
				int xValue = 50;
				int yValue = 50;

				if (x.Attributes["sortorder"] != null && x.Attributes["sortorder"].Value != "") {
					xValue = int.Parse(x.Attributes["sortorder"].Value);
				}

				if (y.Attributes["sortorder"] != null && y.Attributes["sortorder"].Value != "") {
					yValue = int.Parse(y.Attributes["sortorder"].Value);
				}

				return xValue - yValue;
			}

			#endregion
		}

		#endregion
	}
}