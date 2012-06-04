using System.Configuration;
using System.IO;
using System.Web.UI;
using Triton.Controller;

namespace Triton.Web.View
{
	public class WebMasterPage : MasterPage
	{
		public new WebPage Page
		{
			get { return ((WebPage)base.Page) ?? ((WebPage)Context.CurrentHandler); }
		}


		/// <summary>
		/// Gets the masterpage section name, since the masterpage could be in a different section.
		/// </summary>
		public string MasterPageSection { get; set;}


		protected override void OnInit(System.EventArgs e)
		{
			string masterName = Path.GetFileNameWithoutExtension(this.AppRelativeVirtualPath);

			this.Page.LoadControlXml(ConfigurationManager.AppSettings[PageFinder.MASTER_PAGE_PATH_CONFIG] + "\\" + masterName, this.Page.MasterPageSection);

			base.OnInit(e);
		}

		/// <summary>
		/// Sets the master page according to the file name (without the extension) argument.
		/// Assumes the master page is in /PagesPath/Version/Site/Section/masterpages/ directory.
		/// This must be called in the Page_PreInit override method at the child masterpage level.
		/// </summary>
		/// <param name="masterPageName"></param>
		public void LoadMasterPage(
			string masterPageName)
		{
			this.LoadMasterPage(masterPageName, this.Page.Section, this.Page.Site);
		}


		/// <summary>
		/// Sets the master page according to the file name (without the extension) argument.
		/// You set the section name, but site is automaticaly set.
		/// This must be called in the Page_PreInit override method at the child masterpage level.
		/// </summary>
		/// <param name="masterPageName"></param>
		/// <param name="section"></param>
		public void LoadMasterPage(
			string masterPageName,
			string section)
		{
			this.LoadMasterPage(masterPageName, section, this.Page.Site);
		}


		/// <summary>
		/// Sets the master page according to the file name (without the extension) argument.
		/// Specify the section name and site.
		/// This must be called in the Page_PreInit override method at the child masterpage level.
		/// </summary>
		/// <param name="masterPageName"></param>
		/// <param name="section"></param>
		/// <param name="site"></param>
		public void LoadMasterPage(
			string masterPageName,
			string section,
			string site)
		{
			this.MasterPageSection = section;

			base.MasterPageFile = this.Page.GetMasterPageFileRecord(masterPageName, section, site).fullPath;
		}
	}
}