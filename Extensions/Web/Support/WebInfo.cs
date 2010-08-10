using System;
using System.Collections;
using Triton.Controller.Config;

namespace Triton.Web.Support
{

	#region History
	
	// History:
	// 09/23/2009 GV	Added a new DefaultLanguagePath to store the name of the default language directory.

	#endregion

	/// <summary>
	/// <c>WebInfo</c> is a convenience class for getting information about the environment.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class WebInfo
	{
		/// <summary>
		/// Represents application-level variables typically referenced by Application["key"].
		/// This exists due to the need to reference HttpContext.Current in global.asax.cs' 
		/// Application_Start method, which does not exist at that time if a page hasn't been
		/// referenced yet.
		/// </summary>
		private static readonly Hashtable applicationVariables = Hashtable.Synchronized(new Hashtable());


		protected WebInfo() {}


		/// <summary>
		/// Gets the file system path to the application root directory.
		/// </summary>
		public static string BasePath
		{
			get { return (String)applicationVariables["BasePath"]; }
		}


		/// <summary>
		/// Gets the relative URL to the HTTP controller.
		/// </summary>
		public static string Controller
		{
			get { return (String)applicationVariables["Controller"]; }
		}


		/// <summary>
		/// Gets the relative path from the application root to the directory
		/// containing the pages.
		/// </summary>
		public static string PagesPath
		{
			get { return SitesConfig.GetInstance().GetValue("//pagesPath"); }
		}


		/// <summary>
		/// Gets the relative path from the application root to the directory
		/// containing the page content files.
		/// </summary>
		public static string ContentPath
		{
			get { return SitesConfig.GetInstance().GetValue("//contentPath"); }
		}

		/// <summary>
		/// Gets the directory name of the default language.
		/// </summary>
		public static string DefaultLanguagePath
		{
			get { return SitesConfig.GetInstance().GetValue("//defaultLanguageDirName"); }
		}


		/// <summary>
		/// Represents application-level variables, that would typically be stored in Application["key"].
		/// </summary>
		public static Hashtable ApplicationVariables
		{
			get { return applicationVariables; }
		}
	}
}