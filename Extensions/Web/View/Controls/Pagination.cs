using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Triton.Model;
using Triton.Utilities.Reflection;
using Triton.Web.Support;

namespace Triton.Web.View.Controls {

	#region History

	// History:
	//   4/20/2011	SD	Added PageParameterName and PageSizeParameterName properties to allow the
	//					control to work with type-safe paging parameters.

	#endregion

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
			CurrentLinkClass = "currentPageLink";
			LinkClass = "pageLink";
			RenderPageSizeSelection = false;
			PageNumberDisplayStyle = "Center";
			PageNumberDisplaySize = int.MaxValue;
			PageParameterPrefix = "";
			TransitionName = "search";
			RequestItemName = string.Empty;
			NextRenderName = "Next";
			PrevRenderName = "Prev";
			NextClass = "pageLink";
			PrevClass = "pageLink";
			PageParameterName = "page";
			PageSizeParameterName = "pagesize";
			parametersToExclude.Add("st");
			parametersToExclude.Add("e");
			parametersToExclude.Add("uid");
			parametersToExclude.Add("page");
		}


		/// <summary>
		/// Display Options are set on the control by comma separated list and will render in the order
		/// they are given.
		/// Options are: Numbers,Next,Prev,First,Last
		/// </summary>
		public string DisplayOptions
		{
			get;
			set;
		}


		/// <summary>
		/// The title tag to be rendered on all links
		/// </summary>
		public string Title
		{
			get;
			set;
		}


		/// <summary>
		/// Class name to be rendered on the anchor items for styling.
		/// Defaults to pageLink
		/// </summary>
		public string LinkClass
		{
			get;
			set;
		}


		/// <summary>
		/// Text to render for the "next" link.
		/// Defaults to "Next"
		/// </summary>
		public string NextRenderName
		{
			get;
			set;
		}


		/// <summary>
		/// The name of the class to render on the "next" link.
		/// </summary>
		public string NextClass
		{
			get;
			set;
		}


		/// <summary>
		/// The name of the class to render on the "prev" link.
		/// </summary>
		public string PrevClass
		{
			get;
			set;
		}


		/// <summary>
		/// Text to render for the "prev" link.
		/// Defaults to "Prev"
		/// </summary>
		public string PrevRenderName
		{
			get;
			set;
		}


		/// <summary>
		/// Class name to be rendered on the currently selected link
		/// Defaults to currentPageLink
		/// </summary>
		public string CurrentLinkClass
		{
			get;
			set;
		}

		/// <summary>
		/// Name of the item in the request that contains the paged search results.
		/// </summary>
		public string RequestItemName
		{
			get;
			set;
		}


		/// <summary>
		/// Transition event for each page link.
		/// </summary>
		public string TransitionName
		{
			get;
			set;
		}


		/// <summary>
		/// Prefix added to pagesize and page for sorting a particulate item type.
		/// </summary>
		public string PageParameterPrefix
		{
			get;
			set;
		}


		/// <summary>
		/// Gets or sets the name of the request parameter for the page number.
		/// </summary>
		public string PageParameterName
		{
			get;
			set;
		}


		/// <summary>
		/// Gets or sets the name of the request parameter for the page size.
		/// </summary>
		public string PageSizeParameterName
		{
			get;
			set;
		}


		/// <summary>
		/// 
		/// </summary>
		public int PageNumberDisplaySize
		{
			get;
			set;
		}


		/// <summary>
		/// Takes a string: "Center", "Right", or "Left" and denotes which side of the
		/// page control the currently selected page is placed at.
		/// </summary>
		public string PageNumberDisplayStyle
		{
			get;
			set;
		}


		/// <summary>
		/// The current WebPage.
		/// </summary>
		public new WebPage Page
		{
			get {
				return (WebPage)base.Page;
			}
		}


		/// <summary>
		/// Boolean value that, if set to true, will set the baseUrl to "/"
		/// </summary>
		public bool ResetUrlParameters
		{
			get {
				return resetUrlParameters;
			}
			set {
				resetUrlParameters = value;
				if (value) {
					baseUrl = "/";
				}
			}
		}


		/// <summary>
		/// Boolean value that determines if a page size control is rendered on the page.
		/// The values for the control are listed in the page's content copy under
		/// "PageSizeSelectorCopy".  Choices are defaulted to increment from 0 to 5.
		/// </summary>
		public bool RenderPageSizeSelection
		{
			get;
			set;
		}


		/// <summary>
		/// Comma seperated list of request Parameters to be removed from the url
		/// </summary>
		public string ParametersToExclude
		{
			set {
				parametersToExclude.AddRange(value.Split(','));
			}
		}


		protected override void OnLoad(
			EventArgs e)
		{
			parametersToExclude.Add(PageParameterPrefix + PageParameterName);
					//get the nav node
			if (searchResult == null) {
						// get the searchresult from the request
				try {
					searchResult = (SearchResultBase)Page.Request.Items[RequestItemName];
				} catch (Exception) {
					searchResult = null;
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
			return (!parametersToExclude.Contains(key));
		}


		protected override void OnPreRender(
			EventArgs e)
		{
			base.OnPreRender(e);

			baseUrl += string.Format("{0}?st={1}&e={2}", WebInfo.Controller, Page.State, TransitionName);
			foreach (string key in Page.Request.Params.AllKeys) {
				if (IncludeParameter(key)) {
					baseUrl += string.Format("&{0}={1}", key, Page.Request.Params[key]);
				}
			}
		}


		public override void RenderBeginTag(
			HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "pagination");
			base.RenderBeginTag(writer);
		}


		protected override void Render(
			HtmlTextWriter output)
		{
			if ((searchResult != null) && (searchResult.TotalPages > 1)) {
				base.Render(output);
				RenderBeginTag(output); // DIV
				output.RenderBeginTag(HtmlTextWriterTag.Ul); // UL

				if (!string.IsNullOrEmpty(DisplayOptions)) {
					string[] displayFunctions = DisplayOptions.Split(',');
					foreach (string methodName in displayFunctions) {
						string method = methodName.Trim();
						ReflectionUtilities.CallMethod(this, method, new Object[] {output});
					}
				} else {
					// Default to just numbers if no option is specified.
					Numbers(output);
				}

				if (RenderPageSizeSelection) {
					RenderPageSizeSelector(output);
				}
				output.RenderEndTag(); // UL
				RenderEndTag(output); // DIV
			}
		}


		public void Numbers(
			HtmlTextWriter output)
		{
			int startpage = FIRST_PAGE;
			int endpage = searchResult.TotalPages;

			if (PageNumberDisplaySize != int.MaxValue) {
				switch (PageNumberDisplayStyle) {
					case "Center":
						startpage = Math.Max(searchResult.Page - (PageNumberDisplaySize/2), FIRST_PAGE);

						if (searchResult.Page > (PageNumberDisplaySize/2)) {
							endpage = Math.Min(searchResult.Page + (PageNumberDisplaySize/2), searchResult.TotalPages);
						} else {
							endpage = Math.Min(PageNumberDisplaySize, searchResult.TotalPages);
						}
						break;
					case "Left":
						startpage = searchResult.Page;
						endpage = Math.Min(searchResult.Page + PageNumberDisplaySize, searchResult.TotalPages);
						break;
					case "Right":
						startpage = Math.Max(searchResult.Page - PageNumberDisplaySize, FIRST_PAGE);
						endpage = Math.Min(searchResult.Page + PageNumberDisplaySize, searchResult.TotalPages);
						break;
				}
			}

			for (int pageNumber = startpage; pageNumber <= endpage; pageNumber++) {
				output.RenderBeginTag(HtmlTextWriterTag.Li); // LI

				if (pageNumber == searchResult.Page) {
					output.AddAttribute(HtmlTextWriterAttribute.Title, GetTitle("Current Page"));
					output.AddAttribute(HtmlTextWriterAttribute.Class, CurrentLinkClass);
					output.RenderBeginTag(HtmlTextWriterTag.Span); // SPAN
					output.Write(String.Format("{0}", pageNumber));
					output.RenderEndTag(); // SPAN
				} else {
					output.AddAttribute(HtmlTextWriterAttribute.Href, GeneratePageLink(pageNumber));
					output.AddAttribute(HtmlTextWriterAttribute.Title, GetTitle(string.Format("page {0}", pageNumber + 1)));
					output.AddAttribute(HtmlTextWriterAttribute.Class, LinkClass);
					output.RenderBeginTag(HtmlTextWriterTag.A); // A
					output.Write(String.Format("{0}", pageNumber));
					output.RenderEndTag(); // A
				}
				output.RenderEndTag(); // LI
			}
		}


		public void First(
			HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.AddAttribute(HtmlTextWriterAttribute.Id, "first");
			output.AddAttribute(HtmlTextWriterAttribute.Title, GetTitle(string.Format("page {0}", 0)));
			output.AddAttribute(HtmlTextWriterAttribute.Href, GeneratePageLink(0));
			output.RenderBeginTag(HtmlTextWriterTag.A); // A
			output.Write("first", true);
			output.RenderEndTag(); // A or SPAN
			output.RenderEndTag(); // LI
		}


		public void Last(
			HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.AddAttribute(HtmlTextWriterAttribute.Id, "last");
			output.AddAttribute(HtmlTextWriterAttribute.Title, GetTitle(string.Format("page {0}", searchResult.TotalPages - 1)));

			if (searchResult.TotalPages > 0) {
				output.AddAttribute(HtmlTextWriterAttribute.Href, GeneratePageLink(searchResult.TotalPages - 1));
			} else {
				output.RenderBeginTag(HtmlTextWriterTag.Span); // Span
			}

			output.RenderBeginTag(HtmlTextWriterTag.A); // A
			output.Write("last", true);
			output.RenderEndTag(); // A or SPAN
			output.RenderEndTag(); // LI
		}


		public void Prev(
			HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.AddAttribute(HtmlTextWriterAttribute.Id, "prevpage");
			output.AddAttribute(HtmlTextWriterAttribute.Title, GetTitle(string.Format("page {0}", searchResult.Page - 1)));

			if (searchResult.Page != FIRST_PAGE) {
				output.AddAttribute(HtmlTextWriterAttribute.Href, GeneratePageLink(searchResult.Page - 1));
				output.AddAttribute(HtmlTextWriterAttribute.Class, PrevClass);
				output.RenderBeginTag(HtmlTextWriterTag.A); // A
				output.Write(PrevRenderName);
			} else {
				output.RenderBeginTag(HtmlTextWriterTag.Span); // Span
			}

			output.RenderEndTag(); // A or SPAN
			output.RenderEndTag(); // LI
		}


		public void Next(
			HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.RenderEndTag(); // LI
			output.RenderBeginTag(HtmlTextWriterTag.Li); // LI
			output.AddAttribute(HtmlTextWriterAttribute.Id, "nextpage");
			output.AddAttribute(HtmlTextWriterAttribute.Title, GetTitle(string.Format("page {0}", searchResult.Page + 1)));

			if (searchResult.Page != searchResult.TotalPages) {
				output.AddAttribute(HtmlTextWriterAttribute.Href, GeneratePageLink(searchResult.Page + 1));
				output.AddAttribute(HtmlTextWriterAttribute.Class, NextClass);
				output.RenderBeginTag(HtmlTextWriterTag.A); // A
				output.Write(NextRenderName);
			} else {
				output.RenderBeginTag(HtmlTextWriterTag.Span); // Span
			}

			output.RenderEndTag(); // A
			output.RenderEndTag(); // LI
		}


		protected string GeneratePageLink(
			int pageNumber)
		{
			string url = string.Format("{0}&{1}{2}={3}", baseUrl, PageParameterPrefix, PageParameterName, pageNumber);

			return url;
		}


		protected void RenderPageSizeSelector(
			HtmlTextWriter output)
		{
			output.RenderBeginTag(HtmlTextWriterTag.Li); // <li>

			string text = "Add Copy Tag (PageSizeSelectorCopy)";
			text = Page.GetCopyContent("PageSizeSelectorCopy") != "" ? Page.GetCopyContent("PageSizeSelectorCopy") : text;
			output.AddAttribute(HtmlTextWriterAttribute.Class, "pagesizeseletorscopy");
			output.RenderBeginTag(HtmlTextWriterTag.Label);
			output.Write(text);
			output.AddAttribute(HtmlTextWriterAttribute.Class, "pagesizeselectors");
			output.AddAttribute(HtmlTextWriterAttribute.Name, PageParameterPrefix + PageSizeParameterName);
			output.AddAttribute(HtmlTextWriterAttribute.Rel, GeneratePageLink(0));
			output.RenderBeginTag(HtmlTextWriterTag.Select); // <select>

			for (int pageSize = PAGE_SIZE_START; pageSize <= PAGE_SIZE_END; pageSize += PAGE_SIZE_INCREMENT) {
				output.AddAttribute(HtmlTextWriterAttribute.Value, pageSize.ToString());
				if (Page.Request[PageParameterPrefix + PageSizeParameterName] == pageSize.ToString()) {
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


		protected string GetTitle(
			string title)
		{

			if (string.IsNullOrEmpty(Title)) {
				return title;
			} else {
				return Title;
			}
		}
	}
}