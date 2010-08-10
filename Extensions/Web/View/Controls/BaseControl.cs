using System.Web.UI;
using System.Web.UI.WebControls;
using Triton.Web.Support;

namespace Triton.Web.View.Controls
{
	public abstract class BaseControl : WebControl
	{
		protected BaseControl() {}

		protected BaseControl(string tag) : base(tag) {}

		protected BaseControl(HtmlTextWriterTag tag) : base(tag) {}

		public new WebPage Page
		{
			get { return (WebPage) base.Page; }
		}


		/// <summary>
		/// Standard way of creating file paths in the Framework 
		/// </summary>
		/// <param name="type">Use ConfigurableControl.PathType enum</param>
		/// <param name="filePath">path to process</param>
		/// <param name="global">non-site specific path</param>
		/// <param name="useSite">site specific path</param>
		/// <param name="useDefault"></param>
		/// <param name="useSection"></param>
		/// <returns></returns>
		protected virtual string GetPath(
			PathType type,
			string filePath,
			bool global,
			bool useSite,
			bool useDefault,
			bool useSection)
		{
			//check if starts with pages or content, remove "/pages/v*/" or "/content/v*/"
			if (filePath.StartsWith("/pages")) {
				filePath = filePath.Remove(0, 9);
			} else if (filePath.StartsWith("pages")) {
				filePath = filePath.Remove(0, 8);
			} else if (filePath.StartsWith("/content")) {
				filePath = filePath.Remove(0, 11);
			} else if (filePath.StartsWith("content")) {
				filePath = filePath.Remove(0, 10);
			}

			string site;
			//if the path is global or not site specific, set site to nothing
			if (useSite && !global) {
				site = "/" + this.Page.Site;
			} else {
				site = "";
			}

			string rootPath = "";
			switch (type) {
				case PathType.CONTENT:
					rootPath = WebInfo.ContentPath;
					//if pages language defined then append it to the site
					string language = (this.Page.Language == null) ? "" : "/" + this.Page.Language;
					site = site + language;
					break;

				case PathType.PAGES:
					if (useDefault) {
						site = "default";
					}
					rootPath = WebInfo.PagesPath;
					break;
			}

			return string.Format("/{0}v{1}{2}{3}", rootPath, this.Page.Version, site, filePath);
		}


		protected virtual string GetPath(
			PathType type,
			string filePath,
			bool global,
			bool useSite)
		{
			return this.GetPath(type, filePath, global, useSite, false, false);
		}

		#region Nested type: PathType

		/// <summary>
		/// Path Type of the control.
		/// </summary>
		protected enum PathType
		{
			CONTENT,
			PAGES
		}

		#endregion
	}
}