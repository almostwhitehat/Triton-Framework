using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;
using Common.Logging;
using Triton.Web.Support;

namespace Triton.Web.View.Controls
{
	public class Navigation : XmlBasedControl
	{
		private const string LINK_ATTRIBUTE_START = "link-";

		/// <summary>
		/// List of attribute names NOT to be copied from the 
		/// definition in the XML file into the .net control 
		/// </summary>
		protected List<string> attributesToExclude =
			new List<string>(new[] {"transition", "param", "href", "statesIndependentItem", "name", "highlight", "class", "type"});

		private XmlNode exclusions;


		/// <summary>
		/// Public Default Constructor
		/// </summary>
		public Navigation() : base(HtmlTextWriterTag.Ul)
		{
			this.ActivePageTag = "span";
			this.NoLinkTag = "em";
		}


		/// <summary>
		/// If you have more then one Navigation node in your xml, use this to target it.
		/// </summary>
		public string Name { set; get; }

		/// <summary>
		/// Use this property to overwrite the default EM html tag of the items that have no link.
		/// </summary>
		public string NoLinkTag { private get; set; }

		/// <summary>
		/// Use this property to overwrite the default SPAN html tag of the items for the page being viewed.
		/// </summary>
		public string ActivePageTag { private get; set; }


		/// <summary>
		/// Using OnLoad because the XML file will not be availiable till this time.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			//get the nav node
			if (XmlNodeToRender == null) {
				//takes too long
				try {
					XmlDocument contentDoc = Page.ContentXml;
					XmlNodeToRender = this.Name == null
					                  	? contentDoc.SelectSingleNode("//Navigation")
					                  	: contentDoc.SelectSingleNode("//Navigation[@name=\"" + this.Name + "\"]");
				} catch (Exception ex) {
					LogManager.GetLogger(typeof (Navigation)).Error(
						errorMessage => errorMessage("Navigation::onload()", ex));
				}
			}
		}


		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			try {
				base.AddAttributesToRender(writer);
				if (XmlNodeToRender.Attributes.Count > 0) {
					foreach (XmlAttribute attr in XmlNodeToRender.Attributes) {
						//render the attributes
						if (!this.attributesToExclude.Contains(attr.Name) && !attr.Name.StartsWith(LINK_ATTRIBUTE_START)) {
							writer.AddAttribute(attr.Name, attr.Value);
						}
						if (attr.Name == "class") {
							writer.AddAttribute("class", attr.Value);
						}
					}
				}
			} catch {}
		}


		private Control CreateListItem(XmlNode childNode)
		{
			string tagToUse = this.ActivePageTag;
			string itemName = null;
			bool noLink = false;
			string highlight = (childNode.Attributes["highlight"] != null) ? childNode.Attributes["highlight"].Value : null;

			HtmlGenericControl list = new HtmlGenericControl("li");
			//change this to use webControls.ListControl
			/*if (this.IsSecured(childNode.SelectSingleNode("Roles")))
			{
				list.Visible = false;
			}*/
			//else
			//{
			// should be continue on checking security ???


			foreach (XmlAttribute attr in childNode.Attributes) {
				if (!this.attributesToExclude.Contains(attr.Name) && !attr.Name.StartsWith(LINK_ATTRIBUTE_START)) {
					list.Attributes.Add(attr.Name, attr.Value);
				}
			}

			if (childNode.SelectSingleNode("ItemName") != null && childNode.SelectSingleNode("ItemName").Value != "") {
				itemName = childNode.SelectSingleNode("ItemName").InnerText;
			}

			string href = "";

			string pageTransition = (childNode.Attributes["transition"] != null)
			                        	? childNode.Attributes["transition"].Value
			                        	: null;

			if((childNode.Attributes["statesIndependentItem"] != null 
				&& !string.IsNullOrEmpty(childNode.Attributes["statesIndependentItem"].Value)
				&& childNode.Attributes["statesIndependentItem"].Value == "true") 
				|| (childNode.Attributes["href"] != null))
			{
				string hrefValue = childNode.Attributes["href"].Value;
				if (hrefValue == Page.Request["pageurl"]
				    || (Page.Request.Items["Rewriter.URL"] != null && hrefValue == Page.Request.Items["Rewriter.URL"].ToString())) {
					tagToUse = this.NoLinkTag;
					noLink = true;
				} else {
					href = hrefValue;
				}
			} else if (!string.IsNullOrEmpty(pageTransition)) {
				string currentTransition = Page.Request["e"];

				string currentParam = null;

				string type = null;
				if (childNode.Attributes["type"] != null && childNode.Attributes["type"].Value != "") {
					type = childNode.Attributes["type"].Value;
				}

				if (childNode.Attributes["param"] != null && childNode.Attributes["param"].Value != "") {
					currentParam = childNode.Attributes["param"].Value;
				}

				if (currentParam != null && type != null) {
					if (type == "category") {
						if (Page.Request.Params["pcat"] != null) {
							string requestParamValue = Page.Request.Params["pcat"].ToLower();
							if (currentParam.ToLower() == requestParamValue) {
								noLink = true;
							}
						}
					}
				}

				if (this.exclusions != null
				    && this.exclusions.SelectNodes("Exclusion[@transition=\"" + currentTransition + "\"]") != null
				    && this.exclusions.SelectNodes("Exclusion[@transition=\"" + currentTransition + "\"]").Count > 0) {
					XmlNodeList exclusionList = this.exclusions.SelectNodes("Exclusion[@transition=\"" + currentTransition + "\"]");

					foreach (XmlNode exclusion in exclusionList) {
						//get the rules
						XmlNode rules = exclusion.SelectSingleNode("Rules");

						//check all the rules
						foreach (XmlNode rule in rules.ChildNodes) {
							//first check the page name that you are dealing with, only process rules applicable to you
							if (Page.PageName.ToLower() == rule.Attributes["pagename"].Value.ToLower()) {
								string param = rule.Attributes["param"].Value;

								//get both the parameter value from FPRequest that is set by SetParameterAction or just copied from the HttpRequest 
								string ruleParam = Page.Request.Params[param];
								if (currentParam != null) {
									//compare the rule param with the param out of the childNode, if they match then you are on the same page as the nav pointer
									if (ruleParam.ToLower() == currentParam.ToLower()) {
										noLink = true;
									}
								}
							}
						}
					}
				} else {
					string pageSplitTransition = pageTransition.Split('&')[0];

					if (currentTransition.ToLower() == pageSplitTransition.ToLower()) {
						noLink = true;
					}
				}

				href = String.Format("/{0}?st={1}&amp;e={2}", WebInfo.Controller, Page.State, pageTransition);
			} else {
				tagToUse = this.NoLinkTag;
				noLink = true;
			}

			if (noLink == false || (highlight != null && noLink)) {
				//check if highlight need to be rendered.
				if (noLink) {
					if (childNode.Attributes["class"] != null) {
						list.Attributes.Add("class", childNode.Attributes["class"].Value + " " + highlight);
					} else {
						list.Attributes.Add("class", highlight);
					}
				} else {
					if (childNode.Attributes["class"] != null) {
						list.Attributes.Add("class", childNode.Attributes["class"].Value);
					}
				}

				HtmlAnchor anchor = new HtmlAnchor
				                    {
				                    	HRef = href,
				                    	InnerHtml = (itemName ?? childNode.InnerText)
				                    };

				foreach (XmlAttribute attr in childNode.Attributes) {
					if (attr.Name.StartsWith(LINK_ATTRIBUTE_START)) {
						anchor.Attributes.Add(attr.Name.Remove(0, LINK_ATTRIBUTE_START.Length), attr.Value);
					}
				}


				list.Controls.Add(anchor);
			} else {
				HtmlGenericControl surroundingTag = new HtmlGenericControl(tagToUse)
				                                    {
				                                    	InnerHtml = (itemName ?? childNode.InnerText)
				                                    };

				if (childNode.Attributes["class"] != null) {
					list.Attributes.Add("class", childNode.Attributes["class"].Value);
				}

				list.Controls.Add(surroundingTag);
			}

			XmlNode items = childNode.SelectSingleNode("Items");
			if (items != null && items.HasChildNodes) {
				//create the surrounding UL
				HtmlGenericControl ul = new HtmlGenericControl("ul");
				//create the items
				foreach (XmlNode child in items) {
					ul.Controls.Add(this.CreateListItem(child));
				}
				list.Controls.Add(ul);
			}
			//}
			return list;
		}


		protected override void CreateChildControls()
		{
			try {
				XmlNode items = XmlNodeToRender.SelectSingleNode("Items");

				if (XmlNodeToRender.SelectSingleNode("Config") != null
				    && XmlNodeToRender.SelectSingleNode("Config/Exclusions") != null) {
					this.exclusions = XmlNodeToRender.SelectSingleNode("Config/Exclusions");
				}

				foreach (XmlNode childNode in items) {
					Controls.Add(this.CreateListItem(childNode));
				}

				base.CreateChildControls();
			} catch (Exception ex) {
				LogManager.GetLogger(typeof (Navigation)).Error(
					errorMessage => errorMessage("Navigation::CreateChildControls()", ex));
			}
		}


		/*protected bool IsSecured(XmlNode Roles)
		{
			bool result = true;
			Account acct = this.Page.AuthenticatedUser;
			if (Roles != null)
			{
				XmlNodeList roles = Roles.SelectNodes("Role");
				try
				{

					if (acct != null)
					{
						foreach (XmlNode role in roles)
						{
							string roleName = role.InnerText.ToString();
							if (acct.IsMemberOfRole(roleName))
							{
								result = false;
							}
						}
					}
					else
					{
						// do not secure the menu item if the there 
						// is no user logged in and there are 
						// no security roles assigned
						if (roles.Count == 0)
							result = false;
					}
				}
				catch (Exception ex)
				{
					result = true;
				}
			}
			else {
				result = false;
			}
			return result;
		}*/
	}
}