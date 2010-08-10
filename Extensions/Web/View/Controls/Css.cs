using System;
using System.Collections.Generic;
using System.Xml;
using Triton.Utilities.Reflection;

namespace Triton.Web.View.Controls
{
	/// <summary>
	///	Builds <c>link</c> tags for Css stylesheets.  Pulls all css from the page's 
	///	ContentXml and renders them. 
	/// </summary>
	public class Css : CssBase
	{
		protected override void OnLoad(EventArgs e)
		{
			attributesToExclude.Add("type");
			base.OnLoad(e);
		}


		public override bool IsStyleSheetNeeded(XmlNode node)
		{
			bool retValue = true;

			if (node.HasChildNodes) {
				XmlNode rules = node.SelectSingleNode("Rules");

				//check all the rules
				foreach (XmlNode rule in rules.ChildNodes) {
					retValue = this.IsValid(rule);
				}
			}
			return retValue;
		}


		public bool IsValid(XmlNode rule)
		{
			bool retValue = false;

			string pages = rule.Attributes["pagename"].Value;
			string[] page = pages.Split(',');
			List<string> pageNames = new List<string>(page);

			if (pageNames.Contains(Page.PageName.ToLower())) {
				retValue = true;

				if (rule.Attributes["param"] != null && rule.Attributes["param"].Value != "") {
					retValue = false;
					string param = rule.Attributes["param"].Value;
					string[] paramSet = param.Split(':');
					string[] conditionsSet = paramSet[1].Split(',');

					//get both the parameter value from MvcRequest that is set by SetParameterAction or just copied from the HttpRequest 
					string requestParamValue = Page.Request.Params[paramSet[0]];

					//before continuing check if retValue is still false
					for (int i = 0; (i < conditionsSet.Length) && !retValue; i++) {
						//compare the two, if they match then you need to include the Css
						if (requestParamValue != null && (requestParamValue.ToLower() == conditionsSet[i].ToLower())) {
							retValue = true;
						} else {
							if (rule.Attributes["type"] != null && rule.Attributes["type"].Value != "") {
								string type = rule.Attributes["type"].Value;
								if (type == "category") {
									object val = ReflectionUtilities.CallMethod(Page, "IsCategoryPartOfPage", conditionsSet[i]);
									if ((val != null) && (bool) val) {
										retValue = true;
									}
								}
							}
						}
					}
				}
			}

			return retValue;
		}
	}
}