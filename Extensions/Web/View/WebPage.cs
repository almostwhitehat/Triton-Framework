using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Xml;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Config;
using Triton.Controller.Request;
using Triton.Controller.StateMachine;
using Triton.Support.Error;
using Triton.Support.Request;
using Triton.Web.Support;

namespace Triton.Web.View
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// WebPage is the base class for all pages using the Web infrastructure.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class WebPage : Page
	{
		private string globalImagePath;
		private string sectionContentPath;

		/// <summary>
		/// WebPage object.
		/// </summary>
		public WebPage()
		{
			this.State = null;
			this.TransitionContext = (TransitionContext)this.Request.Items["transitionContext"];

			//  get the end state of the transition process if we got to this
			//  page via transition
			PageState endState = null;

			if (this.TransitionContext != null) {
				endState = (PageState)this.TransitionContext.EndState;
			}

			//  get the site the page is for
			if (this.Request["site"] != null) {
				this.Site = this.Request["site"];
			} else if (endState != null) {
				this.Site = endState.Site;
			}

			//  get the section the page is for
			if (this.Request["s"] != null) {
				this.Section = this.Request["s"];
			} else if (endState != null) {
				this.Section = endState.Section;
			}

			//  get the name of the page
			if (this.Request["p"] != null) {
				this.PageName = this.Request["p"];
			} else if (endState != null) {
				this.PageName = endState.Page;
			}

			//  get the state ID of the state the page is for
			if (endState != null) {
				this.State = endState.Id;
			}

			//  try to get the version from the TransitionContext since the page may be
			//  in a version other than the default of the site
			if ((this.TransitionContext != null) && this.TransitionContext.Version.HasValue) {
				this.Version = this.TransitionContext.Version.Value;
			} else {
				//  if there is no TransitionContext (which happens if we get here some
				//  way other than a TransitionCommand), try to get the version from the
				//  request
				try {
					this.Version = int.Parse(this.Request.Version);
				} catch {
					//  if all else fails, get the default version from sites.config
					this.Version = SitesConfig.GetInstance().GetSiteVersion(this.Site);
				}
			}

			//if the errors list is 
			if (this.Request.Items[CoreItemNames.DEFAULT_ERRORS_LIST] != null &&
			    this.Request.Items[CoreItemNames.DEFAULT_ERRORS_LIST] is ErrorList) {

				this.Errors = this.Request.GetItem<ErrorList>(CoreItemNames.DEFAULT_ERRORS_LIST);
			}

			PageFinder.FileRecord xmlFile =
				(PageFinder.FileRecord)this.Request.Items[CoreItemNames.PAGE_XML_FILE_RECORD];

			if (xmlFile != null) {
				this.LoadControlXml(xmlFile);
				this.Language = xmlFile.language;
			}
		}


		/// <summary>
		/// Gets the <b>MvcRequest</b> object for the requested page.
		/// </summary>
		public new MvcRequest Request
		{
			get { return (MvcRequest)base.Context.Items["MVCRequest"]; }
		}


		/// <summary>
		/// Gets the site the page is in.
		/// </summary>
		public new string Site { get; private set; }


		/// <summary>
		/// Gets the section the page is in.
		/// </summary>
		public string Section { get; private set; }


		/// <summary>
		/// Gets the language for the content for the page.  Null if content
		/// is not in a language sub-directory.
		/// </summary>
		public string Language { get; private set; }


		/// <summary>
		/// Get the name of the page.
		/// </summary>
		public string PageName { get; private set; }


		/// <summary>
		/// Gets the version for the site the page is for.
		/// </summary>
		public int Version { get; private set; }


		/// <summary>
		/// Gets the <c>State</c> ID of the state the page represents
		/// (in the state machine).
		/// </summary>
		public long? State { get; private set; }


		/// <summary>
		/// Gets the <c>TransitionContext</c> the page is rendering for.
		/// </summary>
		public TransitionContext TransitionContext { get; private set; }

		/// <summary>
		/// Gets the masterpage section name, since the masterpage could be in a different section.
		/// </summary>
		public string MasterPageSection { get; set;}


		/// <summary>
		/// Gets the path of the <b>parent</b> content directory of the section
		/// the page is under.  This path is relative to the application root defined
		/// by <c>rootPath</c> in web.config.
		/// </summary>
		public string SectionContentPath
		{
			get
			{
				if (this.sectionContentPath == null) {
					string lang = (this.Language == null) ? "" : "/" + this.Language;
					this.sectionContentPath = string.Format("/{0}v{1}/{2}{3}",
					                                        WebInfo.ContentPath,
					                                        this.Version,
					                                        this.Site,
					                                        lang);
				}

				return this.sectionContentPath;
			}
		}


		/// <summary>
		/// Gets the <b>parent</b> directory of the global images directory.
		/// </summary>
		public string GlobalImagePath
		{
			get
			{
				if (this.globalImagePath == null) {
					this.globalImagePath = string.Format("/{0}v{1}",
					                                     WebInfo.ContentPath,
					                                     this.Version);
				}

				return this.globalImagePath;
			}
		}


		/// <summary>
		/// Gets the <c>XmlDocument</c> that contains the content for the page.
		/// </summary>
		public XmlDocument ContentXml { get; protected set; }


		/// <summary>
		/// Gets an <c>ErrorList</c> of errors returned from the controller.
		/// </summary>
		public ErrorList Errors { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		public string Controller
		{
			get { return this.GetControllerPath(false); }
		}


		/// <summary>
		/// 
		/// </summary>
		public string SecureController
		{
			get { return this.GetControllerPath(true); }
		}


		/// <summary>
		/// Indicates whether or not there are any (validation) errors returned
		/// from the controller.
		/// </summary>
		/// <returns><c>True</c> if the page has errors to display, <c>false</c>
		///			if not.</returns>
		public bool HasErrors()
		{
			return ((this.Errors != null) && (this.Errors.Count > 0));
		}


		/// <summary>
		/// Loads a control's content XML into the page's content XML.
		/// </summary>
		/// <param name="path">relative path to the file</param>
		public void LoadControlXml(
			string path)
		{
			this.LoadControlXml(path, this.Section);
		}


		/// <summary>
		/// Loads a control's content XML into the page's content XML.
		/// </summary>
		/// <param name="path">section relative path to the xml file</param>
		/// <param name="section">section to search fro xml file in</param>
		public void LoadControlXml(
			string path,
			string section)
		{
			this.LoadControlXml(path, section, this.Site);
		}


		/// <summary>
		/// Loads a control's content XML into the page's content XML.
		/// </summary>
		/// <param name="path">section relative path to the xml file</param>
		/// <param name="section">section to search fro xml file in</param>
		/// <param name="site">site of the xml file to look in</param>
		public void LoadControlXml(
			string path,
			string section,
			string site)
		{
			PageFinder.FileRecord xmlRec = PageFinder.GetInstance().FindXml(Request, path, section, site);

			this.LoadControlXml(xmlRec);
		}


		/// <summary>
		/// Loads a control's content XML into the page's content XML.
		/// </summary>
		/// <param name="record">The record of the file that was found.</param>
		protected void LoadControlXml(
			PageFinder.FileRecord record)
		{
			try {
				if (this.ContentXml == null) {
					this.ContentXml = new XmlDocument();
				}

				if (this.ContentXml.DocumentElement == null) {
					this.ContentXml.LoadXml("<Content></Content>");
				}

				if (record != null && !string.IsNullOrEmpty(record.fullPath)) {
					XmlDocument controlXml = new XmlDocument();

					controlXml.Load(WebInfo.BasePath + record.fullPath);

					if (controlXml.DocumentElement != null) {
						//  append the control's XML to the page's
						XmlNode nodes = this.ContentXml.ImportNode(controlXml.DocumentElement, true);
						this.ContentXml.DocumentElement.AppendChild(nodes);
					}
				} else {
					LogManager.GetCurrentClassLogger().Warn(warn => warn("Could not find the xml to load."));
				}
			} catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Warn(warn => warn("Error occured when loading the xml document."), ex);
			}
		}


		/// <summary>
		/// Sets the master page according to the file name (without the extension) argument.
		/// Assumes the master page is in /PagesPath/Version/Site/Section/masterpages/ directory.
		/// This must be called in the OnPreInit override method at the page level.
		/// </summary>
		/// <param name="masterPageName"></param>
		public void LoadMasterPage(
			string masterPageName)
		{
			this.LoadMasterPage(masterPageName, this.Section, this.Site);
		}


		/// <summary>
		/// Sets the master page according to the file name (without the extension) argument.
		/// You set the section name, but site is automaticaly set.
		/// This must be called in the OnPreInit override method at the page level.
		/// </summary>
		/// <param name="masterPageName"></param>
		/// <param name="section"></param>
		public void LoadMasterPage(
			string masterPageName,
			string section)
		{
			this.LoadMasterPage(masterPageName, section, this.Site);
		}


		/// <summary>
		/// Sets the master page according to the file name (without the extension) argument.
		/// Specify the section name and site.
		/// This must be called in the OnPreInit override method at the page level.
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

			base.MasterPageFile = this.GetMasterPageFileRecord(masterPageName, section, site).fullPath;
		}


		public PageFinder.FileRecord GetMasterPageFileRecord(
			string masterPageName,
			string section,
			string site)
		{
			return PageFinder.GetInstance().FindMasterPage(Request, masterPageName, section, site)
			       ??
			       PageFinder.GetInstance().FindMasterPage(Request,
				                                           masterPageName,
			                                               section,
														   PageFinder.DEFAULT_LANGUAGE_DIR_NAME);
		}


		private string GetControllerPath(bool secure)
		{
			string result = WebInfo.Controller;

			if (this.Request is MvcHttpRequest) {
				MvcHttpRequest request = (MvcHttpRequest)this.Request;

				if (!string.IsNullOrEmpty(request.Url.Host)) {
					result = string.Format("http{0}://{1}:{2}/{3}",
					                       (secure) ? "s" : string.Empty,
					                       request.Url.Host,
					                       request.Url.Port,
					                       WebInfo.Controller);
				}
			}

			return result;
		}


		/// <summary>
		/// Reads the content XML and finds the copy for the supplied name. if the node is not found then search in the pages section.
		/// </summary>
		/// <param name="copyName">Copy XML node name</param>
		/// <returns></returns>
		public string GetCopyContent(string copyName)
		{
			return this.GetCopyContent(copyName, false);
		}


		/// <summary>
		/// Reads the content XML and finds the copy for the supplied name. if the node is not found then search in the pages section.
		/// </summary>
		/// <param name="copyName">Copy XML node name</param>
		/// <param name="returnEmptyString">Whether or not to return empty string or not found in xml text.</param>
		/// <returns></returns>
		public string GetCopyContent(string copyName,
		                             bool returnEmptyString)
		{
			string result;
			try {
				//  try to find the node matching the given name at the
				//  top level under "<Copy>"
				XmlNode copyNode = this.ContentXml.SelectSingleNode("//Copy/" + copyName);

				//  if we didn't find the copy as a direct child of <Copy>
				//  try including the section name
				if (copyNode == null) {
					copyNode = this.ContentXml.SelectSingleNode("//Copy/section[@name=\"" + this.Request["s"] + "\"]/" + copyName);
				}

				result = this.ParseCopy(copyNode.InnerText);
			} catch (Exception ex) {
				result = (returnEmptyString) ? string.Empty : "<!-- LABEL NOT RETRIEVED FROM XML: " + copyName + " -->";
				LogManager.GetCurrentClassLogger().Info(info => info("An error occured when retrieving copy content."), ex);
			}

			return result;
		}


		/// <summary>
		/// Performs any text replacement of items enclosed in square brackets with request parameters of the same name.
		/// </summary>
		/// <param name="contentCopy">string - Copy to parse</param>
		/// <returns>parsed Copy</returns>
		public string ParseCopy(string contentCopy)
		{
			// Loops all matches and replaces text with request parameter value
			foreach (Match match in Regex.Matches(contentCopy, @"[\[[\w]+]")) {
				string param = match.Value.Substring(1, match.Value.Length - 2);
				contentCopy = contentCopy.Replace(match.Value, Page.Request[param]);
			}

			return contentCopy;
		}
	}
}