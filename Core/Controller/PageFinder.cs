using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Text;
using Common.Logging;
using Triton.Controller.Config;
using Triton.Controller.Utilities;
using Triton.Utilities.Configuration;

namespace Triton.Controller
{

	#region History

	// History:
	// 6/2/2009		KP  Changing the logging to Common.logging.
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// <c>PageFinder</c> handles finding the location of the proper page
	/// (aspx file), master page, static content file (xml), image, etc. file
	/// based on the version, site, section, and filename.
	/// </summary>
	/// <remarks>
	/// The <c>PageFinder</c> searches for files using the following algorithm:<br/>
	/// 1. Look for {version}/{site}/{section}/{file}.<br/>
	/// If the file is a page:<br/>
	///  2a. Look for {version}/[configuration["defaultDirName"]/{section}/{file}.<br/>
	/// If the file is a content:<br/>
	///  2b. Look for {version}/{site}/{language}/{section}/{file}.<br/>
	/// 3. Get the site's search method.<br/>
	/// If search method is defaultDir:<br/>
	///  4a. Look for {version}/[configuration["defaultDirName"]/{file}.<br/>
	/// If search method is prevVersion:<br/>
	///  4b. Repeat search on previous version.<br/>
	/// <br/>
	/// If the master page(s) are stored in a subdirectory from where the aspx page
	/// is located, the relative path to the master pages directory is specified in the
	/// <b>masterSubPath</b> appSetting in the config file.  For example, "/masterpages".
	///	</remarks>
	///	<author>Scott Dyke</author>
	public class PageFinder
	{
		#region FileType enum

		public enum FileType
		{
			CONTENT,
			PAGE
		}

		#endregion

		/// <summary>
		/// The name of the AppSetting in the config file for the path from
		/// the "section" to the directory containing the master pages.
		/// </summary>
		public const string MASTER_PAGE_PATH_CONFIG = "masterSubPath";

		/// <summary>
		/// The path for the site config setting where to get the default directory name
		/// </summary>
		public const string DEFAULT_LANGUAGE_DIR_NAME = "/siteConfiguration/general/defaultLanguageDirName";

		private const string MASTER_EXTENSTION = ".master";

		

		private const string PAGE_EXTENSTION = ".aspx";

		private const string SEARCH_METHOD_DEFAULT = "defaultDir";
		private const string SEARCH_METHOD_PREVIOUS_VER = "prevVersion";
		private const string STATUS_LOGGER = "Singletons";
		private const string XML_EXTENSTION = ".xml";
		private static readonly object syncRoot = new object();
		private static PageFinder instance;

		private readonly string basePath;
		private readonly string contentPath;
		private readonly string pagesPath;
		private readonly string defaultLanguageDirName;

		/// <summary>
		/// The path relative to the "section" where the master pages are stored.
		/// </summary>
		private string masterPagesPath;

		private Hashtable pageCache;
		private Hashtable xmlCache;


		/// <summary>
		/// Private constructor to enforce singleton implementation.
		/// </summary>
		private PageFinder()
		{
			//  get the base file path for the root directory
			//basePath = Info.BasePath;
			this.basePath = ConfigurationManager.AppSettings["rootPath"];
			//  initialize the caches (for the file paths)
			this.InitCache();

			//  get the relative path to the "pages" directory from sites.config
			this.pagesPath = SitesConfig.GetInstance().GetValue("/siteConfiguration/general/pagesPath");
			//  get the relative path to the "content" directory from sites.config
			this.contentPath = SitesConfig.GetInstance().GetValue("/siteConfiguration/general/contentPath");
			//	get the name of the default language directory
			this.defaultLanguageDirName = SitesConfig.GetInstance().GetValue(DEFAULT_LANGUAGE_DIR_NAME);
		}


		/// <summary>
		/// Returns a refernce to the <i>Singleton</i> instance of the <c>PageFinder</c>.
		/// </summary>
		/// <returns>The <i>Singleton</i> instance of the <c>PageFinder</c>.</returns>
		public static PageFinder GetInstance()
		{
			if (instance == null) {
				lock (syncRoot) {
					if (instance == null) {
						instance = new PageFinder();
						LogManager.GetCurrentClassLogger().Info(
							infoMessage => infoMessage("PageFinder - instantiated new PageFinder."));
					}
				}
			}

			return instance;
		}


		/// <summary>
		/// Flushes the caches to force a new page find for subsequent requests.
		/// </summary>
		public void Reset()
		{
			this.InitCache();
			LogManager.GetCurrentClassLogger().Info(
					infoMessage => infoMessage("PageFinder - full reset."));
		}


		/// <summary>
		/// Flushes the xml and page caches for the desired site/section combination to force a new page find for subsequent requests.
		/// </summary>
		public void Reset(
			string site,
			string section)
		{
			this.ResetCache(site, section, FileType.PAGE);
			this.ResetCache(site, section, FileType.CONTENT);
			LogManager.GetCurrentClassLogger().Info(
					infoMessage => infoMessage("PageFinder - reset site '{0}', section '{1}'.", site, section));
		}


		/// <summary>
		/// Flushes a cached <c>Hashtable</c> for the desired site/section combination to force a new page find for subsequent requests. 
		/// If section is null, the whole site will be reset.
		/// </summary>
		public void ResetCache(
			string site,
			string section,
			FileType cache)
		{
			Hashtable hash = null;
			section = (section != null) ? section.ToLower().Trim() : "";

			// Need to have site, section is optional
			if (site != null) {
				site = site.ToLower().Trim();

				// Determine which cache object to reset
				switch (cache) {
					case FileType.PAGE:
						hash = this.pageCache;
						break;

					case FileType.CONTENT:
						hash = this.xmlCache;
						break;
				}

				try {
					string[] keys = new string[hash.Keys.Count];
					hash.Keys.CopyTo(keys, 0);

					// Loop cache and remove item if found
					foreach (string key in keys) {
						if (key.IndexOf(site + section) >= 0) {
							hash.Remove(key);
#if (TRACE)
							LogManager.GetCurrentClassLogger().Info(
									infoMessage => infoMessage("PageFinder - removing: {0} ", key));
#endif
						}
					}
				} catch (Exception e) {
					LogManager.GetCurrentClassLogger().Error(
							errorMessage => errorMessage(e.Message, e));
				}
			}

			LogManager.GetCurrentClassLogger().Info(
					infoMessage => infoMessage("PageFinder - reset site '{0}', section '{1}', type '{2}'.",
                             site,
                             section,
                             cache));
		}


		/// <summary>
		/// Finds the page (aspx file) for the given page name, section, and site.
		/// </summary>
		/// <param name="pageName">The base name of the page to find, excluding ".aspx".</param>
		/// <param name="section">The name of the section the page is for.</param>
		/// <param name="site">The site code for the site the page is for.</param>
		/// <returns>A <b>FileRecord</b> containing the information on the page's location.</returns>
		internal FileRecord FindPage(
			string pageName,
			string section,
			string site)
		{
			return this.FindPage(pageName, section, site, PAGE_EXTENSTION);
		}


		/// <summary>
		/// Finds the master page (.master file) for the given name, section, and site.
		/// </summary>
		/// <param name="pageName">The base name of the master page to find, excluding ".master".</param>
		/// <param name="section">The name of the section the page is for.</param>
		/// <param name="site">The site code for the site the page is for.</param>
		/// <returns>A <b>FileRecord</b> containing the information on the master page's location.</returns>
		public FileRecord FindMasterPage(
			string pageName,
			string section,
			string site)
		{
			//  if we haven't loaded the master pages path yet, do so
			if (this.masterPagesPath == null) {
				this.masterPagesPath = ConfigurationManager.AppSettings[MASTER_PAGE_PATH_CONFIG];

				//  if there is no setting in the config file, set the path to nothing
				if (this.masterPagesPath == null) {
					this.masterPagesPath = "";
				}
			}

			return this.FindPage(pageName, section + this.masterPagesPath, site, MASTER_EXTENSTION);
		}


		/// <summary>
		/// Finds the page (aspx file) for the given page name, section, and site.
		/// </summary>
		/// <param name="pageName">The base name of the page to find, excluding ".aspx".</param>
		/// <param name="section">The name of the section the page is for.</param>
		/// <param name="site">The site code for the site the page is for.</param>
		/// <param name="extension">The file extension of the file to find (including ".").</param>
		/// <returns>A <b>FileRecord</b> containing the information on the page's location.</returns>
		private FileRecord FindPage(
			string pageName,
			string section,
			string site,
			string extension)
		{
			string key = site + section + pageName + extension;
			//  get the path for the requested page from the cache
			FileRecord pageFileRec = (FileRecord) this.pageCache[key];

			//  if the page is not in the cache...
			if (pageFileRec == null) {
				//  get the version of the site
				int version = SitesConfig.GetInstance().GetSiteVersion(site);

				//  attempt to find the appropriate aspx file
				pageFileRec = this.FindFile(pageName + extension, section, site, version, this.pagesPath, FileType.PAGE);

				//  if we successfully found a path to the target page,
				//  put it in page cache.
				if (pageFileRec != null) {
					this.pageCache[key] = pageFileRec;
				}
			}

			return pageFileRec;
		}


		/// <summary>
		/// Finds the XML file containing the static content for the given page name,
		/// section, and site.
		/// </summary>
		/// <param name="pageName">The base name of the page to find the xml file
		/// for, excluding the file extension.</param>
		/// <param name="section">The name of the section the page is for.</param>
		/// <param name="site">The site code for the site the page is for.</param>
		/// <returns>The full path to the XML file containing the static content
		/// for the page.</returns>
// change to internal for 2.0
//	internal FileRecord FindXml(
		public FileRecord FindXml(
			string pageName,
			string section,
			string site)
		{
			string key = site + section + pageName;
			FileRecord xmlFileRec = (FileRecord) this.xmlCache[key];

			if (xmlFileRec == null) {
				int version = SitesConfig.GetInstance().GetSiteVersion(site);

				xmlFileRec = this.FindFile(pageName + XML_EXTENSTION, section, site, version, this.contentPath, FileType.CONTENT);

				//  if we successfully found a path to the target page,
				//  put it in page cache.
				if (xmlFileRec != null) {
					this.xmlCache[key] = xmlFileRec;
				}
			}

			return xmlFileRec;
		}


		/// <summary>
		/// Finds the appropriate file based on version, site, and section.
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="fileName">The name of the file to find, including extension.</param>
		/// <param name="section">The section of the site the file is for.</param>
		/// <param name="site">The site the file is for.</param>
		/// <param name="version">The version of the site the file is for.</param>
		/// <param name="rootPath">The relative path from the application root to
		///			the directory to search for the file.</param>
		/// <param name="type">The<c>FileType</c> of the file.</param>
		/// <returns>A <c>FileRecord</c> containing the information on the file located,
		///			or <c>null</c> if no file was found.</returns>
		private FileRecord FindFile(
			String fileName,
			String section,
			String site,
			int version,
			String rootPath,
			FileType type)
		{
			bool found = false;
			FileRecord fileRec = new FileRecord {
			                                    	rootPath = rootPath,
			                                    	version = version.ToString(),
			                                    	site = site,
			                                    	section = section,
			                                    	subPath = fileName
			                                    };

			if (this.FileExists(fileRec)) {
				fileRec.fullPath = "/" + fileRec.BuildPath();
				found = true;
			} else {
				switch (type) {
					case FileType.PAGE:
						//  check for <version>/[configuration["defaultDirName"]/<section>/<file>
						if (string.IsNullOrEmpty(this.defaultLanguageDirName)) {
							throw new ApplicationException("Default directory name not found in sites.config ["
									+ DEFAULT_LANGUAGE_DIR_NAME + "]");
						}
						fileRec.site = this.defaultLanguageDirName;
						if (this.FileExists(fileRec)) {
							fileRec.fullPath = "/" + fileRec.BuildPath();
							found = true;
						}
						break;

					case FileType.CONTENT:
						//  check for <version>/<site>/<language>/<section>/<file>
						//  (NOTE: this is only applicable for content)
						fileRec.language = LanguageUtilities.GetLanguage(site);
						if (this.FileExists(fileRec)) {
							fileRec.fullPath = "/" + fileRec.BuildPath();
							found = true;
						}
						break;
				}

				if (!found) {
					try {
						//  get the configuration section for the specified site
						XmlConfiguration config = SitesConfig.GetInstance().GetConfig("sites", site.ToUpper());
						//  get the value of the defaultPageSearchMethod setting for the site
						String searchMethod = config.GetValue("//defaultPageSearchMethod");

						//  if the defaultPageSearchMethod setting is to check
						//  the previous version(s), recursively call FindFile
						//  with the next lower version
						if (SEARCH_METHOD_PREVIOUS_VER.Equals(searchMethod) && (version > 1)) {
							fileRec = this.FindFile(fileName, section, site, version - 1, rootPath, type);
							found = this.FileExists(fileRec);

							//  check for <version>/[configuration["defaultDirName"]/<file>
						} else if (SEARCH_METHOD_DEFAULT.Equals(searchMethod)) {
							fileRec.site = this.defaultLanguageDirName;
							fileRec.section = null;
							fileRec.language = null;
							if (this.FileExists(fileRec)) {
								fileRec.fullPath = "/" + fileRec.BuildPath();
								found = true;
							}
						}
					} catch (Exception e) {
						LogManager.GetCurrentClassLogger().Error(
							errorMessage => errorMessage("PageFinder.FindFile : "), e);
						fileRec = null;
					}
				}
			}

			//  log info if the file was not found
			if (!found) {
				//  get the components of the file name
				string[] pieces = fileName.Split('.');
				//  we only want to log items like 'file.xml' and ignore ones like 'file.css.xml'
				if (pieces.Length == 2) {
					LogManager.GetCurrentClassLogger().Warn(
						warnMessage => warnMessage("PageFinder.FindFile: file not found: file = '{0}', section = '{1}', site = '{2}', version = {3}",
						                           	fileName,
						                           	section,
						                           	site,
						                           	version));
				}
				fileRec = null;
			}

			return fileRec;
		}


		/// <summary>
		/// Determines if the given file exists.
		/// </summary>
		/// <param name="fileRec">A <b>FileRecord</b> containing the file information 
		///			to check the existance of.</param>
		/// <returns>True if the file exists, false if not.</returns>
		private bool FileExists(
			FileRecord fileRec)
		{
			return ((fileRec != null) && File.Exists(this.basePath + fileRec.BuildPath()));
		}


		/// <summary>
		/// Initializes the caches used by PageFinder.  This clears any previously
		/// cached values and re-creates the caches.
		/// </summary>
		private void InitCache()
		{
			if (this.pageCache != null) {
				this.pageCache.Clear();
			}
			if (this.xmlCache != null) {
				this.xmlCache.Clear();
			}

			this.pageCache = Hashtable.Synchronized(new Hashtable());
			this.xmlCache = Hashtable.Synchronized(new Hashtable());
		}

		#region Nested type: FileRecord

		/// <summary>
		/// <c>FileRecord</c> defines attributes about a file that <c>PageFinder</c>
		/// locates.
		/// </summary>
		public class FileRecord
		{
			private readonly StringBuilder pathBuilder = new StringBuilder();
			public string fullPath;
			public string language;
			public string rootPath;
			public string section;
			public string site;
			public string subPath;
			public string version;


			internal string BuildPath()
			{
				this.pathBuilder.Length = 0;

				if (this.rootPath != null) {
					this.pathBuilder.Append(this.rootPath);
					if (this.pathBuilder[this.pathBuilder.Length - 1] == '/') {
						this.pathBuilder.Length--;
					}
				}
				if (this.version != null) {
					this.pathBuilder.Append(@"/v" + this.version);
				}
				if (this.site != null) {
					this.pathBuilder.Append(@"/" + this.site);
				}
				if (this.language != null) {
					this.pathBuilder.Append(@"/" + this.language);
				}
				if (this.section != null) {
					this.pathBuilder.Append(@"/" + this.section);
				}
				if (this.subPath != null) {
					this.pathBuilder.Append(@"/" + this.subPath);
				}

				return this.pathBuilder.ToString();
			}
		}

		#endregion
	}
}