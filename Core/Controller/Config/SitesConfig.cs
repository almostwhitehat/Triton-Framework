using System;
using System.Configuration;
using Common.Logging;
using Triton.Utilities.Configuration;

namespace Triton.Controller.Config
{

	#region History

	// History:
	// 5/30/2009	KP  Changed the logging to be Common.Logging
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// Summary description for SitesConfig.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class SitesConfig : XmlConfiguration
	{
		private static readonly object syncRoot = new object();
		private static SitesConfig instance;


		/// <summary>
		/// Default constructor.  Private to enforce singleton implementation.
		/// </summary>
		private SitesConfig() {}


		/// <summary>
		/// Returns the global (singleton) <c>SitesConfig</c> object.
		/// </summary>
		/// <returns></returns>
		public static SitesConfig GetInstance()
		{
			if (instance == null) {
				//  ensure only one thread is able to instantiate an instance at a time.
				lock (syncRoot) {
					if (instance == null) {
						//  get the path to the sites.config file from web.config
						String sitesConfigPath = ConfigurationSettings.AppSettings["sitesConfigPath"];
						//  TODO: make sure we got a path -- throw exception?

						try {
							LogManager.GetCurrentClassLogger().Info(
								infoMessage => infoMessage("SitesConfig - starting load."));
							//  make the XmlConfiguration object for the sites.config file
							instance = new SitesConfig();
							//instance.Load(Info.BasePath + sitesConfigPath);
							instance.Load(ConfigurationSettings.AppSettings["rootPath"] + sitesConfigPath);
							LogManager.GetCurrentClassLogger().Info(
								infoMessage => infoMessage("SitesConfig - load completed."));
						} catch (Exception e) {
							LogManager.GetCurrentClassLogger().Error(
								errorMessage => errorMessage("SitesConfig.GetInstance: "), e);
							throw;
						}
					}
				}
			}

			return instance;
		}


		/// <summary>
		/// Gets the version number of the given site.
		/// </summary>
		/// <param name="site">The site code to get the version for.</param>
		/// <returns>The version for the specified site.</returns>
		public int GetSiteVersion(
			String site)
		{
			int ver = 0;
			String verStr;

			try {
				//  get the configuration section for the specified site
				XmlConfiguration config = GetConfig("sites", site.ToUpper());
				//  get the value of the version node of the site
				verStr = config.GetValue("//version");
				ver = Convert.ToInt16(verStr);
			} catch (Exception) {
				try {
					//  get the default version from the config file
					verStr = GetInstance().GetValue("/siteConfiguration/general/defaultVersion");
					ver = Convert.ToInt16(verStr);
				} catch (Exception e2) {
					LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("SitesConfig.GetSiteVersion "), e2);
//  TODO: throw something here.
				}
			}

			return ver;
		}


		/// <summary>
		/// Resets the SitesConfig singleton, forcing it to be reinitialized when
		/// next referenced.
		/// </summary>
		public void Reset()
		{
			instance = null;
			LogManager.GetCurrentClassLogger().Info(
				infoMessage => infoMessage("SitesConfig reset."));
		}
	}
}