using System;
using System.Configuration;
using Common.Logging;
using Triton.Utilities.Configuration;

namespace Triton.Controller
{

	#region History

	// History:
	// 6/2/2009		KP  Changed the logging to Common.logging.
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// Singleton to maintain the mappings for XML Actions.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class XmlActionMap : XmlConfiguration
	{
		private const string STATUS_LOG = "Singletons";

		private static readonly object syncRoot = new object();
		private static XmlActionMap instance;


		/// <summary>
		/// Default constructor.  Private to enforce singleton implementation.
		/// </summary>
		private XmlActionMap() {}


		/// <summary>
		/// Returns the global (singleton) <c>XmlActionMap</c> object.
		/// </summary>
		/// <returns></returns>
		public static XmlActionMap GetInstance()
		{
			if (instance == null) {
				//  ensure only one thread is able to instantiate an instance at a time.
				lock (syncRoot) {
					if (instance == null) {
						LogManager.GetCurrentClassLogger().Info(
							infoMessage => infoMessage("XmlActionMap - starting load."));

						//  get the path to the sites.config file from web.config
						String configPath = ConfigurationSettings.AppSettings["xmlActionMapConfigPath"];
//  TODO: make sure we got a path -- throw exception?

						try {
							//  make the XmlConfiguration object for the sites.config file
							instance = new XmlActionMap();
							//instance.Load(Info.BasePath + configPath);
							instance.Load(ConfigurationSettings.AppSettings["rootPath"] + configPath);
							LogManager.GetCurrentClassLogger().Debug(
								debugMessage => debugMessage("XmlActionMap -  successfully loaded action map"));
						} catch (Exception e) {
							LogManager.GetCurrentClassLogger().Error(
								errorMessage => errorMessage("XmlActionMap.GetInstance: "), e);
							throw;
						}

						LogManager.GetCurrentClassLogger().Info(
							infoMessage => infoMessage("XmlActionMap - load completed."));
					}
				}
			}

			return instance;
		}


		public void Reset()
		{
			instance = null;
			LogManager.GetCurrentClassLogger().Info(
				infoMessage => infoMessage("XmlActionMap reset."));
		}
	}
}