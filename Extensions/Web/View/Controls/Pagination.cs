using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Triton.Model;
using Triton.Utilities.Reflection;
using Triton.Web.Support;

namespace Triton.Web.View.Controls
{
	/// <summary>
	/// Paging control to use on search result thumbnails.
	/// </summary>
	public class Pagination : WebControl
	{
		private const int PAGE_SIZE_END = 25;
		private const int PAGE_SIZE_INCREMENT = 5;
		private const int PAGE_SIZE_START = 5;
		private const int FIRST_PAGE = 1;
		private string baseUrl = "";
		private List<String> parametersToExclude = new List<string>();
		private bool resetUrlParameters = true;
		private SearchResultBase searchResult;


		/// <summary>
		/// Default constructor takes zero arguments.
		/// </summary>
		public Pagination()
			: base(HtmlTextWriterTag.Div)
		{
			this.CurrentLinkClass = "currentPageLink";
			this.LinkClass = "pageLink";
			this.RenderPageSizeSelection = false;
			this.PageNumberDisplayStyle = "Center";
			this.PageNumberDisplaySize = int.MaxValue;
			this.PageParameterPrefix = "";
			this.TransitionName = "search";
			this.RequestItemName = string.Empty;
			this.NextRenderName = "Next";
			this.PrevRenderName = "Prev";
			this.NextClass = "pageLink";
			this.PrevClass = "pageLink";
			this.parametersToExclude.Add("st");
			this.parametersToExclude.Add("e");
			this.parametersToExclude.Add("uid");
			this.parametersToExclude.Add("page");
		}


		/// <summary>
		/// Display Options are set on the control by comma separated list and will render in the order
		/// they are given.
		/// Options are: Numbers,Next,Prev,First,Last
		/// </summary>
		public string DisplayOptions { get; set; }

		/// <summary>
		/// The title tag to be rendered on all links
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Class name to be rendered on the anchor items for styling.
		/// Defaults to pageLink
		/// </summary>
		public string LinkClass { get; set; }

		/// <summary>
		/// Text to render for the "next" link.
		/// Defaults to "Next"
		/// </summary>
		public string NextRenderName { get; set; }

		/// <summary>
		/// The name of the class to render on the "next" link.
		/// </summary>
		public string NextClass { get; set; }

		/// <summary>
		/// The name of the class to render on the "prev" link.
		/// </summary>
		public string PrevClass { get; set; }

		/// <summary>
		/// Text to render for the "prev" link.
		/// Defaults to "Prev"
		/// </summary>
		public string PrevRenderName { get; set; }

		/// <summary>
		/// Class name to be rendered on the currently selected link
		/// Defaults to currentPageLink
		/// </summary>
		public string CurrentLinkClass { get; set; }

		/// <summary>
		/// Name of the item in the request that contains the paged search results.
		/// </summary>
		public string RequestItemName { get; set; }

		/// <summary>
		/// Transition event for each page link.
		/// </summary>
		public string TransitionName { get; set; }

		/// <summary>
		/// Prefix added to pagesize and page for sorting a particulate item type.
		/// </summary>
		public string PageParameterPrefix { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int PageNumberDisplaySize { get; set; }

		/// <summary>
		/// Takes a string: "Center", "Right", or "Left" and denotes which side of the
		/// page control the currently selected page is placed at.
		/// </summary>
		public string PageNumberDisplayStyle { get; set; }

		/// <summary>
		/// The current WebPage.
		/// </summary>
		public new WebPage Page
		{
			get { return (WebPage)base.Page; }
		}

		/// <summary>
		/// Boolean value that, if set to true, will set the baseUrl to "/"
		/// </summary>
		public bool ResetUrlParameters
		{
			get { return this.resetUrlParameters; }
			set
			{
				this.resetUrlParameters = value;
				if (value) {
					this.baseUrl = "/";
				}
			}
		}

		/// <summary>
		/// Boolean value that determines if a page size control is rendered on the page.
		/// The values for the control are listed in the page's content copy under
		/// "PageSizeSelectorCopy".  Choices are defaulted to increment from 0 to 5.
		/// </summary>
		public bool RenderPageSizeSelection { get; set; }

		/// <summary>
		/// Comma seperated list of request Parameters to be removed from the url
		/// </summary>
		public string ParametersToExclude
		{
			set
			{
				// split the string 
				string[] parameters = value.Split(',');
				foreach (string parameter in parameters) {
					this.parametersToExclude.Add(parameter);
				}
			}
		}


		protected override void OnLoad(EventArgs e)
		{
			this.parametersToExclude.Add(this.PageParameterPrefix + "page");
			//get the nav node
			if (this.searchResult == null) {
				// get the searchresult from the request
				try {
					this.searchResult = (SearchResultBase)this.Page.Request.Items[this.RequestItemName];
				} catch (Exception) {
					this.searchResult = null;
				}
			}
		}


		/// <summary>
		/// Determines if the parameter is should be included in the rendered tag.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private bool IncludeParameter(
			string key)
		{
			return (!this.parametersToExclude.Contains(key));
		}


		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			this.baseUrl += string.Format("{0}?st={1}&e={2}", WebInfo.Controller, this.Page.State, this.TransitionName);
			foreach (string key in this.Page.Request.Params.AllKeys) {
				if (this.IncludeParameter(key)) {
					this.baseUrl += string.Format("&{0}={1}", key, this.Page.Request.Params[key]);
				}
			}
		}


		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "pagination");
			base.RenderBeginTag(writer);
		}


		protected override void Render(
			HtmlTextWriter output)
		{
			if ((this.searchResult != null) && (this.searchResult.TotalPages > 1)) {
				base.Render(output);
				this.RenderBeginTag(output); // DIV
				output.RenderBeginTag(HtmlTextWriterTag.Ul); // UL

				if (!string.IsNullOrEmpty(this.DisplayOptions)) {
					string[] displayFunctions = this.DisplayOptions.Split(',');
					foreach (string methodName in displayFunctions) {
						string method = methodName.Trim();
						ReflectionUtilities.CallMethod(this, method, new Object[] {output});
					}
				} else {
					// Default to just numbers if no option is specified.
					this.Numbers(output);
				}

				if (this.RenderPageSizeSelection) {
					this.RenderPageSizeSelector(output);
				}
				output.RenderEndTag(); // UL
				RenderEndTag(output); // DIV
			}
		}


		public void Numbers(HtmlTextWriter output)
		{
			int startpage = FIRST_PAGE;
			int endpage = this.searchResult.TotalPages;

			if (this.PageNumberDisplaySize != int.MaxValue) {
				switch (this.PageNumberDisplayStyle) {
					case "Center":
						startpage = Math.Max(this.searchResult.Page - (this.PageNumberDisplaySize/2), FIRST_PAGE);

						if (this.searchResult.Page > (this.PageNumberDisplaySize/2)) {
							endpage = Math.Min(this.searchResult.Page + (this.PageNumberDisplaySize/2), this.searchResult.TotalPages);
						} else {
							endpage = Math.Min(this.PageNumberDisplaySize, this.searchResult.TotalPages);
						}
						break;
					case "Left":
						startpage = this.searchResult.Page;
						endpage = Math.Min(this.searchResult.Page + this.PageNumberDisplaySize, this.searchResult.TotalPages);
						break;
					case "Right":
						startpage = Math.Max(this.searchResult.Page - this.PageNumberDisplaySize, FIRST_PAGE);
						endpage = Math.Min(this.searchResult.Page + this.PageNumberDisplaySize, this.searchResult.TotalPages);
						break;
				}
			}

			for (int pageNumber = startpage; pageNumber <= endpage; pageNumber++) {
				output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
				if (pageNumber == this.searchResult.Page) {
					output.AddAttribute(HtmlTextWriterAttribute.Title, this.GetTitle("Current Page"));
					output.AddAttribute(HtmlTextWriterAttribute.Class, this.CurrentLinkClass);
					output.RenderBeginTag(HtmlTextWriterTag.Span); // SPAN
					output.Write(String.Format("{0}", pageNumber));
					output.RenderEndTag(); // SPAN
				} else {
					output.AddAttribute(HtmlTextWriterAttribute.Href, this.GeneratePageLink(pageNumber));
					output.AddAttribute(HtmlTextWriterAttribute.Title, this.GetTitle(string.Format("page {0}", pageNumber + 1)));
					output.AddAttribute(HtmlTextWriterAttribute.Class, this.LinkClass);
					output.RenderBeginTag(HtmlTextWriterTag.A); // A
					output.Write(String.Format("{0}", pageNumber));
					output.RenderEndTag(); // A
				}
				output.RenderEndTag(); // LI
			}
		}


		public void First(HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.AddAttribute(HtmlTextWriterAttribute.Id, "first");
			output.AddAttribute(HtmlTextWriterAttribute.Title, this.GetTitle(string.Format("page {0}", 0)));
			output.AddAttribute(HtmlTextWriterAttribute.Href, this.GeneratePageLink(0));
			output.RenderBeginTag(HtmlTextWriterTag.A); // A
			output.Write("first", true);
			output.RenderEndTag(); // A or SPAN
			output.RenderEndTag(); // LI
		}


		public void Last(HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.AddAttribute(HtmlTextWriterAttribute.Id, "last");
			output.AddAttribute(HtmlTextWriterAttribute.Title, this.GetTitle(string.Format("page {0}", this.searchResult.TotalPages - 1)));
			if (this.searchResult.TotalPages > 0) {
				output.AddAttribute(HtmlTextWriterAttribute.Href, this.GeneratePageLink(this.searchResult.TotalPages - 1));
			} else {
				output.RenderBeginTag(HtmlTextWriterTag.Span); // Span
			}

			output.RenderBeginTag(HtmlTextWriterTag.A); // A
			output.Write("last", true);
			output.RenderEndTag(); // A or SPAN
			output.RenderEndTag(); // LI
		}


		public void Prev(HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.AddAttribute(HtmlTextWriterAttribute.Id, "prevpage");
			output.AddAttribute(HtmlTextWriterAttribute.Title, this.GetTitle(string.Format("page {0}", this.searchResult.Page - 1)));
			if (this.searchResult.Page != FIRST_PAGE) {
				output.AddAttribute(HtmlTextWriterAttribute.Href, this.GeneratePageLink(this.searchResult.Page - 1));
				output.AddAttribute(HtmlTextWriterAttribute.Class, this.PrevClass);
				output.RenderBeginTag(HtmlTextWriterTag.A); // A
				output.Write(this.PrevRenderName);
			} else {
				output.RenderBeginTag(HtmlTextWriterTag.Span); // Span
			}

			output.RenderEndTag(); // A or SPAN
			output.RenderEndTag(); // LI
		}


		public void Next(HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.RenderEndTag(); // LI
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.AddAttribute(HtmlTextWriterAttribute.Id, "nextpage");
			output.AddAttribute(HtmlTextWriterAttribute.Title, this.GetTitle(string.Format("page {0}", this.searchResult.Page + 1)));
			if (this.searchResult.Page != this.searchResult.TotalPages) {
				output.AddAttribute(HtmlTextWriterAttribute.Href, this.GeneratePageLink(this.searchResult.Page + 1));
				output.AddAttribute(HtmlTextWriterAttribute.Class, this.NextClass);
				output.RenderBeginTag(HtmlTextWriterTag.A); // A
				output.Write(this.NextRenderName);
			} else {
				output.RenderBeginTag(HtmlTextWriterTag.Span); // Span
			}

			output.RenderEndTag(); // A
			output.RenderEndTag(); // LI
		}


		protected string GeneratePageLink(int pageNumber)
		{
			string url = string.Format("{0}&{2}page={1}", this.baseUrl, pageNumber, this.PageParameterPrefix);

			return url;
		}


		protected void RenderPageSizeSelector(HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // <li>

			string text = "Add Copy Tag (PageSizeSelectorCopy)";
			text = this.Page.GetCopyContent("PageSizeSelectorCopy") != "" ? this.Page.GetCopyContent("PageSizeSelectorCopy") : text;
			output.AddAttribute(HtmlTextWriterAttribute.Class, "pagesizeseletorscopy");
			output.RenderBeginTag(HtmlTextWriterTag.Label);
			output.Write(text);
			output.AddAttribute(HtmlTextWriterAttribute.Class, "pagesizeselectors");
			output.AddAttribute(HtmlTextWriterAttribute.Name, this.PageParameterPrefix + "pagesize");
			output.AddAttribute(HtmlTextWriterAttribute.Rel, this.GeneratePageLink(0));
			output.RenderBeginTag(HtmlTextWriterTag.Select); // <select>

			for (int pageSize = PAGE_SIZE_START; pageSize <= PAGE_SIZE_END; pageSize += PAGE_SIZE_INCREMENT) {
				output.AddAttribute(HtmlTextWriterAttribute.Value, pageSize.ToString());
				if (this.Page.Request[this.PageParameterPrefix + "pagesize"] == pageSize.ToString()) {
					output.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
				}
				output.RenderBeginTag(HtmlTextWriterTag.Option); // <option value="5">5</option>
				output.Write(pageSize.ToString());
				output.RenderEndTag();
			}
			output.RenderEndTag(); // </select>
			output.RenderEndTag(); // </Label
			output.RenderEndTag(); // </li>
		}


		protected string GetTitle(string title)
		{
			if (string.IsNullOrEmpty(this.Title)) {
				return title;
			} else {
				return this.Title;
			}
		}
	}
}