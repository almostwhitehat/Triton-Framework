using System;
using System.Collections;
using System.Configuration;
using System.Threading;
using Common.Logging;
using Triton.Model.Dao;

namespace Triton.Controller.Publish
{

	#region History

	// History:
	// 6/2/2009		KP Changed the logging to Common.logging
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// PublishManager is a singleton that manages the information for publishing
	/// of pages.
	/// </summary>
	/// <remarks>
	/// PublishManager keeps track of published content information by maintaining
	/// a PublishPageRecord for each published content in a <c>PublishedPageCollection</c>. 
	/// The key for the content in the hashtable is {stateId}_{event}_{requestParams},
	/// where:<br></br>
	///   {stateId} is the ID of the state the event is fired from.<br></br>
	///   {event}  is the name of the event.<br></br>
	///   {requestParams}  is a "_"-delimited list of the <b>values</b> of the request
	///				parameters identified by the <c>PublishKeyParams</c> property
	///				of the transition corresponding to the event.<p>
	///				
	///	PublishManager also maintains a hashtable of the expiration times set for
	///	each site.  In this hashtable the key is the site code and the value is the
	///	expiration time for published pages, in minutes.  The expiration time for
	///	each site is defined by the <c>pageExpirationTime</c> attribute in 
	///	sites.config</p><p>
	///	
	///	The publishedPages collection that keeps track of published pages is periodically 
	///	checked for expired pages by a thread timer.  The interval at which the check is 
	///	made is defined by the <c>pageCheckTime</c> attribute in sites.config.  This
	///	process removes any page references from the publishedPages were the page has
	///	expired.</p><p>
	///	
	///	PublishManager does not itself publish the pages.  It provides a PagePublisher,
	/// via the GetPublisher method, to perform the actual page publishing.  Each
	/// PagePublisher is coupled to the HTTP request performing the publish so
	/// thread safety is ensured.</p><p>
	/// 
	///	The published pages are published to the appropriate sub-directory under the
	///	publish directory for the site and section they belong to.  The published
	///	file name is of the form: {event}_{requestParams}_{page},
	/// where:<br></br>
	///   {event}  is the name of the event the page is generated in response to.<br></br>
	///   {requestParams}  is a "_"-delimited list of the <b>values</b> of the request
	///				parameters identified by the <c>PublishKeyParams</c> property
	///				of the transition corresponding to the event.
	///   {page}  is the name of the page.</p> 
	///	
	///	
	///	</remarks>
	///	<author>Scott Dyke</author>
	public class PublishManager
	{
		private const string DAO_SETTING = "publishDao";

		/// <summary>
		/// The name of the Logger to use to logging trace info -- used only if PUBLISH_TRACE compile
		/// flag is defined.  This Logger must be defined in loggers.config.
		/// </summary>
		private static readonly object syncRoot = new object();

		private static PublishManager instance;

		private long checkInterval;
		private Timer cleanupTimer;
		private PublishedPageCollection publishedPages;
		private long saveInterval;
		private Timer storeTimer;


		/// <summary>
		/// Gets the interval, in minutes, at which the collection of published content 
		/// is checked for expired items.
		/// </summary>
		public long CheckInterval
		{
			get { return this.checkInterval; }
		}


		/// <summary>
		/// Gets the interval, in minutes, at which the collection of published content is
		/// written to peristent storage.
		/// </summary>
		public long StoreInterval
		{
			get { return this.saveInterval; }
		}


		/// <summary>
		/// Gets an instance of the <b>PublishManager</b>.
		/// The <b>PublishManager</b> is an instance of the class identified in the 
		/// configuration/controllerSettings/publishing section of web.config.
		/// </summary>
		/// <returns>An instance of a <b>PublishManager</b>.</returns>
		internal static PublishManager GetPublishManager()
		{
			if (instance == null) {
				lock (syncRoot) {
					if (instance == null) {
						//NameValueCollection config = (NameValueCollection)System.Configuration.ConfigurationSettings.GetConfig(PUBLISH_CONFIG_PATH);

						//if (config == null) {
						//    throw new Exception(string.Format("No PublishManager defined in '{0}' in web.config.", PUBLISH_CONFIG_PATH));
						//}

						//string mgrName = config["PublishManager"];

						//instance = (PublishManager)System.Activator.CreateInstance(Type.GetType(mgrName));
						instance = (PublishManager) Activator.CreateInstance(
						                            	Type.GetType("Triton.Controller.Publish.PublishManager"));

						instance.Initialize();
					}
				}
			}

			return instance;
		}


		internal PublishRecord GetPublishRecord(
			string key)
		{
			return this.publishedPages[key];
		}


		/// <summary>
		/// Resets the entire published page collection, forcing all pages to be
		/// re-published on next request.
		/// </summary>
		/// <returns>The number of pages affected.</returns>
		public int Reset()
		{
			int cnt = this.publishedPages.Count;

			this.publishedPages.Clear();

			return cnt;
		}


		/// <summary>
		/// Resets all of the published pages for the specified site, forcing the 
		/// pages to be re-published on next request.
		/// </summary>
		/// <returns>The number of pages affected.</returns>
		public int Reset(
			string site)
		{
			int cnt = 0;
			ArrayList keys = new ArrayList();

			keys.AddRange(this.publishedPages.Keys);

			foreach (string k in keys) {
				PublishRecord rec = this.publishedPages[k];
				if (rec.PublishedState.Site == site) {
					this.publishedPages.Remove(rec.Key);
					cnt++;
				}
			}

			return cnt;
		}


		/// <summary>
		/// Resets the published pages identified in the given list, forcing the 
		/// pages to be re-published on next request.
		/// </summary>
		/// <returns>The number of pages affected.</returns>
		public int Reset(
			params string[] keys)
		{
			int cnt = 0;

			foreach (string key in keys) {
				this.publishedPages.Remove(key);
				cnt++;
			}

			return cnt;
		}


		/// <summary>
		/// Gets the <c>PublishRecord</c> for the target page of the given
		/// context.
		/// </summary>		
		/// <param name="key"></param>
		/// <param name="context">The <c>TransitionContext</c> to get the <c>PublishRecord</c> for.</param>
		/// <param name="publisher">The <c>ContentPublisher</c></param>
		/// <param name="addIfNotPresent">If <b>true</b> adds a new <b>PublishRecord</b> 
		///		to the internal collection for the given context.</param>
		/// <returns>The <c>PublishRecord</c> for the target page of the given context.</returns>
		internal PublishRecord GetPublishRecord(
			string key,
			TransitionContext context,
			IContentPublisher publisher,
			bool addIfNotPresent)
		{
			PublishRecord pubRec = this.publishedPages[key];

			if ((pubRec == null) && addIfNotPresent) {
				pubRec = new PublishRecord(key,
				                           context.StartState.Id,
				                           context.StartEvent,
				                           context.EndState.Id,
				                           null,
				                           null,
				                           publisher.Name);
				this.publishedPages.Add(key, pubRec);
			}

			return pubRec;
		}


		/// <summary>
		/// Processes the internal collection, removing any "pages" that have expired.
		/// </summary>
		/// <remarks>
		/// This is the TimerCallback delegate called by the cleanupTimer.
		/// </remarks>
		/// <param name="state">Unused.</param>
		private void CleanUpExpired(
			object state)
		{
			DateTime now = DateTime.Now;
			string[] keys = new string[this.publishedPages.Count];
			Hashtable publishers = new Hashtable();

#if (PUBLISH_TRACE)
		MatTimer timer = new MatTimer();
		timer.Start();
		Logger.GetLogger(LOGGER).Status("Start CleanUpExpired.");
#endif
			this.publishedPages.Keys.CopyTo(keys, 0);
			foreach (string key in keys) {
				try {
					PublishRecord pubRec = this.publishedPages[key];
					bool remove = false;
					//  skip pages that are in the process of being published
					if (!pubRec.Publishing) {
						try {
							Publisher publisher = (Publisher) publishers[pubRec.PublisherName];
							if (publisher == null) {
								publisher = Publisher.GetPublisher(pubRec.PublisherName);
								publishers[pubRec.PublisherName] = publisher;
							}

							remove = publisher.IsExpired(pubRec);

							//  if something goes wrong, assume is a bogus record and remove it
						} catch (Exception e) {
							remove = true;
						}

						if (remove) {
#if (PUBLISH_TRACE)
						Logger.GetLogger(LOGGER).Status(string.Format(" Removing: {0} [hits: {1}]", key, pubRec.HitCount));
#endif
							this.publishedPages.Remove(key);
						}
					}
				} catch (Exception e) {
					LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("PublishManager.CleanUpExpired: key= {0} : {1}", key, e));
				}
			}
#if (PUBLISH_TRACE)
		timer.Stop();
		Logger.GetLogger(LOGGER).Status("End CleanUpExpired: time= " + timer.Time + ".  Remaining:  " + publishedPages.Count);
#endif
		}


		/// <summary>
		/// Initializes the <b>PublishManager</b>.
		/// </summary>
		private void Initialize()
		{
			//  call the abstract Init to allow sub-class to perform its initialization
//		Init();

			//  get the intervals for checking the published content list for expired
			//  content and for saving the list to persistant storage
			PublishConfigSection config = ConfigurationManager.GetSection(
			                              	"controllerSettings/publishing") as PublishConfigSection;

			if (config.Publish) {
				try {
					this.checkInterval = long.Parse(config.Settings["expireCheckInterval"].Value);
				} catch (Exception e) {
// TODO:
				}
				try {
					this.saveInterval = long.Parse(config.Settings["storeInterval"].Value);
				} catch (Exception e) {
// TODO:
				}

				//  set up the thread timers
				this.InitializeTimers();

				//  load the published page info from persistent store
				this.publishedPages = this.LoadPages();
				//  remove any expired pages from PublishedPageCollection
				this.CleanUpExpired(null);
			}
		}


		/// <summary>
		/// Initializes the thread timers used by <b>PublishManager</b>.
		/// </summary>
		private void InitializeTimers()
		{
			//  get the interval at which the cache should be checked for expired pages
			long checkInterval = this.CheckInterval;
			//  convert from minutes to milliseconds;
			checkInterval *= 60000;
			//  create the thread timer for checking the cache
			this.cleanupTimer = new Timer(this.CleanUpExpired, null, checkInterval, checkInterval);

			//  get the interval at which the cache should be checked for expired pages
			long storeInterval = this.StoreInterval;
			//  convert from minutes to milliseconds;
			storeInterval *= 60000;
			//  get a random value between 0 and storeInterval as the start interval --
			//  this should provide a staggered start for different servers on a farm so they
			//  don't all perform their updates at the same time
			Random rand = new Random();
			long initTime = rand.Next((int) storeInterval);
			LogManager.GetCurrentClassLogger().Info(
				infoMessage => infoMessage("Start time for saving to persistant store: {0}, interval: {1}",
				                           	initTime,
				                           	storeInterval));
			//  create the thread timer for storing the cache to persistent store
			this.storeTimer = new Timer(this.StorePages, null, initTime, storeInterval);
		}


		/// <summary>
		/// Stores the internal collection to persistent storage.
		/// </summary>
		/// <remarks>
		/// This is the TimerCallback delegate called by the storeTimer.
		/// </remarks>
		/// <param name="state">Unused.</param>
		private void StorePages(
			object state)
		{
			IPublishDao dao = GetPublishDao();

			dao.SavePublishInfo(this.publishedPages);
		}


		private PublishedPageCollection LoadPages()
		{
			IPublishDao dao = GetPublishDao();

			return dao.GetPublishInfo();
		}


		/// <summary>
		/// Returns an instance of a <c>IPublishDao</c>.  The concrete class
		/// instantiated is defined in the web.config file.
		/// </summary>
		/// <returns>A <c>IPublishDao</c>.</returns>
		private static IPublishDao GetPublishDao()
		{
			//PublishConfigSection config = ConfigurationManager.GetSection(
			//                              	"controllerSettings/publishing") as PublishConfigSection;
			//string daoClass = config.Settings[DAO_SETTING].Value;

			IPublishDao dao = DaoFactory.GetDao<IPublishDao>();

			return dao;
		}


		public Publisher GetPublisher(
			string name)
		{
			// TODO: get a Publisher from the pool
			return null;
		}


		public void AddPublisher(
			Publisher publisher)
		{
			// TODO: add the Publisher to some kind of pool
		}
	}
}