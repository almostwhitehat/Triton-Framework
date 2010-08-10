using System;
using System.Collections;
using System.Threading;
using Common.Logging;

namespace Triton.Support.Collections
{

	#region History

	// History:
	// 06/05/2009	KP	Changed the logging to Common.Logging.
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
	// 11/24/2009	GV	Moved the CacheException to its own class.

	#endregion

	/// <summary>
	/// Defines the different types of caches that can be created.
	/// </summary>
	///	<author>Scott Dyke</author>
	public enum CacheType
	{
		/// <summary>
		/// An <b>ABSOLUTE_EXPIRATION</b> cache expires all items in the 
		/// cache at a specified point in time.
		/// </summary>
		ABSOLUTE_EXPIRATION,
		/// <summary>
		/// A <b>RELATIVE_EXPIRATION</b> cache expires items from the cache
		/// a specified amount of time after they were added.
		/// </summary>
		RELATIVE_EXPIRATION
	}

	/// <summary>
	/// The <b>CacheItemRemovedCallback</b> delegate defines a callback method 
	/// for notifying applications when a cached item is removed from the cache.
	/// </summary>
	///	<author>Scott Dyke</author>
	public delegate void CacheItemRemovedCallback(object obj);

	/// <summary>
	/// The <b>Cache</b> is a keyed collection of objects where the objects in the
	/// collection are automatically removed at a specified time or interval.
	/// </summary>
	/// <remarks>
	/// A <b>Cache</b> can be set up in one of two modes, <b>relative</b> or <b>absolute</b>.
	/// In an absolute Cache all items in the cache expire at a specified time of day.
	/// In a relative Cache items expire at some amount of time after they were added.
	/// <p></p>
	/// A relative Cache supports the <i>keep alive</i> option.  <i>Keep alive</i> resets the
	/// clock for an item's expiration each time it is used.  This affectively changes the 
	/// definition of the time interval specified for expiring objects to be from the last time
	/// used rather than from the time it was added.
	/// <p></p>
	/// A relative Cache that is set to <i>keep alive</i> can also be set to expire items
	/// after a certain amount of time regardless of when they were last used.  This is the
	/// <i>drop dead</i> time.  The <i>drop dead</i> must be greater than the regular
	/// expiration time.
	///	</remarks>
	///	<author>Scott Dyke</author>
	public class Cache
	{
		private static readonly TimeSpan NULL_TIMESPAN = TimeSpan.MinValue;

		private CacheType cacheType;
		private TimeSpan checkInterval;
		private Timer checkTimer;
		private TimeSpan dropDeadTime;
		private TimeSpan expireTimespan;
		private Hashtable hash = Hashtable.Synchronized(new Hashtable());
		private bool keepAlive;
		private CacheItemRemovedCallback removeCallback;


		/// <summary>
		/// Constructs a <b>Cache</b> of the specified type with the given check
		/// interval and item expire time.
		/// </summary>
		/// <remarks>
		/// A Cache constructed in this manner does not include the keep-alive option.
		/// </remarks>
		/// <param name="checkInterval">The inteval at which to check the cache for expired items.</param>
		/// <param name="expireTime">The time or interval at which items expire from the cache.</param>
		/// <param name="type">The type of cache to construct.</param>
		public Cache(
			TimeSpan checkInterval,
			TimeSpan expireTime,
			CacheType type)
		{
			this.Init(checkInterval, expireTime, type, false, NULL_TIMESPAN);
		}


		/// <summary>
		/// Constructs a <b>Cache</b> of the specified type with the given check
		/// interval, item expire time, and keep-alive settings.
		/// </summary>
		/// <param name="checkInterval">The inteval at which to check the cache for expired items.</param>
		/// <param name="expireTime">The time or interval at which items expire from the cache.</param>
		/// <param name="type">The type of cache to construct.</param>
		/// <param name="keepAlive">If <b>true</b> <i>expireTime</i> is from the last time the
		///		item was access, if <b>false</b> <i>expireTime</i> is from the time the item
		///		was added to the cache.</param>
		public Cache(
			TimeSpan checkInterval,
			TimeSpan expireTime,
			CacheType type,
			bool keepAlive)
		{
			this.Init(checkInterval, expireTime, type, keepAlive, NULL_TIMESPAN);
		}


		/// <summary>
		/// Constructs a <b>Cache</b> of the specified type with the given check
		/// interval, item expire time, keep-alive and drop dead settings.
		/// </summary>
		/// <param name="checkInterval">The inteval at which to check the cache for expired items.</param>
		/// <param name="expireTime">The time or interval at which items expire from the cache.</param>
		/// <param name="type">The type of cache to construct.</param>
		/// <param name="keepAlive">If <b>true</b> <i>expireTime</i> is from the last time the
		///		item was access, if <b>false</b> <i>expireTime</i> is from the time the item
		///		was added to the cache.</param>
		/// <param name="dropDeadTime">Applicable only if <i>keepAlive</i> is <b>true</b>.
		///		Defines the interval from the time an item is added to the cache to when
		///		it should expire, regardless of when it was last accessed.</param>
		public Cache(
			TimeSpan checkInterval,
			TimeSpan expireTime,
			CacheType type,
			bool keepAlive,
			TimeSpan dropDeadTime)
		{
			this.Init(checkInterval, expireTime, type, keepAlive, dropDeadTime);
		}


		/// <summary>
		/// Gets or sets the value of an item in the cache with the given key.
		/// </summary>
		public object this[string key]
		{
			get { return this.Get(key); }
			set
			{
				CacheItem item = new CacheItem(value, this.GetExpireTime());
				this.hash[key] = item;
			}
		}


		/// <summary>
		/// Gets the number of items in the cache.
		/// </summary>
		public int Count
		{
			get { return this.hash.Count; }
		}


		/// <summary>
		/// Gets the type of the cache.
		/// </summary>
		public CacheType CacheType
		{
			get { return this.cacheType; }
		}


		/// <summary>
		/// Gets or sets the <b>CacheItemRemovedCallback</b> callback method
		/// called when an item is removed from the cache.
		/// </summary>
		public CacheItemRemovedCallback RemoveCallback
		{
			get { return this.removeCallback; }
			set { this.removeCallback = value; }
		}


		/// <summary>
		/// Adds an item to the cache.
		/// </summary>
		/// <param name="key">The key for the item.</param>
		/// <param name="val">The item to add.</param>
		public void Add(
			string key,
			object val)
		{
			CacheItem item = new CacheItem(val, this.GetExpireTime());
			this.hash.Add(key, item);
#if (CACHE_TRACE)
		LogManager.GetCurrentClassLogger().Debug(
			debugMessage => debugMessage("Item Added: {0} : {1}", key, val.ToString());
#endif
		}


		/// <summary>
		/// Gets an item from the cache with the specified key.
		/// </summary>
		/// <param name="key">The key for the item to get.</param>
		/// <returns>The item from the cache with the given key, 
		///		or null if the key does not exist in the cache.</returns>
		public object Get(
			string key)
		{
			object obj = null;
			CacheItem item = (CacheItem)this.hash[key];

			if (item != null) {
				this.UpdateExpireTime(item);
				obj = item.Object;
			}

			return obj;
		}


		/// <summary>
		/// Removes the item with the given key from the cache.
		/// </summary>
		/// <param name="key">The key of the item to remove.</param>
		public void Remove(
			string key)
		{
			CacheItem item = (CacheItem)this.hash[key];

			if ((item != null) && (this.removeCallback != null)) {
				this.removeCallback(item.Object);
			}

			this.hash.Remove(key);
		}


		/// <summary>
		/// Gets an Enumerator for the cache.
		/// </summary>
		/// <returns></returns>
		public IDictionaryEnumerator GetEnumerator()
		{
			return new CacheEnumerator(this);
		}


		/// <summary>
		/// Removes all items from the cache.
		/// </summary>
		public void Clear()
		{
			this.hash.Clear();
		}


		/// <summary>
		/// Disposes of the cache.
		/// </summary>
		public void Dispose()
		{
			this.Clear();
			this.hash = null;
			this.checkTimer.Dispose();
		}


		/// <summary>
		/// Initializes the cache.
		/// </summary>
		/// <param name="checkInterval">The inteval at which to check the cache for expired items.</param>
		/// <param name="expireTime">The time or interval at which items expire from the cache.</param>
		/// <param name="type">The type of cache to construct.</param>
		/// <param name="keepAlive">If <b>true</b> <i>expireTime</i> is from the last time the
		///		item was access, if <b>false</b> <i>expireTime</i> is from the time the item
		///		was added to the cache.</param>
		/// <param name="dropDeadTime">Applicable only if <i>keepAlive</i> is <b>true</b>.
		///		Defines the interval from the time an item is added to the cache to when
		///		it should expire, regardless of when it was last accessed.</param>
		private void Init(
			TimeSpan checkInterval,
			TimeSpan expireTime,
			CacheType type,
			bool keepAlive,
			TimeSpan dropDeadTime)
		{
			if ((dropDeadTime != NULL_TIMESPAN) && (dropDeadTime <= expireTime)) {
				throw new CacheException("Invalid dropDeadTime.  The dropDeadTime must be greater than expireTime.");
			}

			this.checkInterval = checkInterval;
			this.cacheType = type;
			this.expireTimespan = expireTime;
			this.keepAlive = keepAlive;
			this.dropDeadTime = dropDeadTime;

			TimerCallback tc = this.CheckExpirations;
			this.checkTimer = new Timer(tc, null, checkInterval, checkInterval);
#if (CACHE_TRACE)
		LogManager.GetCurrentClassLogger().Debug(
			debugMessage => debugMessage(
				"Constructing Cache: checkInterval = {0}, expireTime = {1}, type = {2}, keepAlive = {3}, dropDeadTime = {4}",
				checkInterval, expireTime, type, keepAlive, (dropDeadTime == NULL_TIMESPAN) ? "none" : dropDeadTime.ToString()));
#endif
		}


		/// <summary>
		/// Gets the expiration time for a new item based on the cache type
		/// and current time.
		/// </summary>
		/// <returns>A <b>DateTime</b> representing the time at which an item
		///		being added or updated now should expire.</returns>
		private DateTime GetExpireTime()
		{
			DateTime expireTime = DateTime.Now;

			switch (this.cacheType) {
				case CacheType.ABSOLUTE_EXPIRATION:
					//  if the cache is set to expire all items at an absolute time,
					//  set the hours, minutes and seconds for the current date
					expireTime = expireTime.AddHours(this.expireTimespan.Hours - expireTime.Hour);
					expireTime = expireTime.AddMinutes(this.expireTimespan.Minutes - expireTime.Minute);
					expireTime = expireTime.AddSeconds(this.expireTimespan.Seconds - expireTime.Second);
					expireTime = expireTime.AddMilliseconds(this.expireTimespan.Milliseconds - expireTime.Millisecond);

					//  if the absolute expire time has already passed for today,
					//  add a day
					if (expireTime < DateTime.Now) {
						expireTime = expireTime.AddDays(1);
					}
					break;

				case CacheType.RELATIVE_EXPIRATION:
					//  if the cache is set to expire items a certain amount of time
					//  after they were added, add that timespan to the current time
					expireTime = expireTime.Add(this.expireTimespan);
					break;

				default:
					throw new CacheException("Unrecognized CacheType. Can not determine expiration.");
			}

			return expireTime;
		}


		/// <summary>
		/// Updates the expire time of the given item if the cache is 
		/// <b>RELATIVE_EXPIRATION</b> and keep-alive is on.
		/// </summary>
		/// <param name="item">The <b>CacheItem</b> to update the expire time for.</param>
		private void UpdateExpireTime(
			CacheItem item)
		{
			//  the item's expire time can only be updated if the cache is set
			//  for relative expiration and to keep items alive when used
			if ((this.cacheType == CacheType.RELATIVE_EXPIRATION) && this.keepAlive) {
				DateTime expireTime = this.GetExpireTime();

				//  see if there is a drop dead time specified
				if (this.dropDeadTime != NULL_TIMESPAN) {
					//  calculate the drop dead time based on the time the item was added
					DateTime dropDeadTime = item.AddedAt;
					dropDeadTime = dropDeadTime.Add(this.dropDeadTime);
					//  if the drop dead time is sooner than the extended expire time,
					//  use the drop dead time instead
					if (dropDeadTime < expireTime) {
						expireTime = dropDeadTime;
					}
				}

				//  update the item's expire time
				item.ExpiresAt = expireTime;
			}
		}


		/// <summary>
		/// This is the TimerCallback method for the checkTimer, used to remove
		/// remove expired items from the cache.
		/// </summary>
		/// <param name="info">unused</param>
		private void CheckExpirations(
			object info)
		{
#if (CACHE_TRACE)
		MatTimer	timer = new MatTimer();
		timer.Start();
		LogManager.GetCurrentClassLogger().Debug(
			debugMessage => debugMessage(string.Format("CheckExpirations start: count = {0}.", hash.Count));
#endif
			//  get the current time
			DateTime now = DateTime.Now;
			//  get an array of the keys in the hash to loop thru
			string[] keys = new string[this.hash.Count];
			this.hash.Keys.CopyTo(keys, 0);

			//  loop thru the hash checking the expiration times of each item
			//  and removing any that have expired
			for (int k = 0; k < keys.Length; k++) {
				try {
					if (((CacheItem)this.hash[keys[k]]).ExpiresAt <= now) {
#if (CACHE_TRACE)

		LogManager.GetCurrentClassLogger().Debug(
			debugMessage => debugMessage("Item Removed: {0} : {1}", keys[k], ((CacheItem)hash[keys[k]]).Object.ToString()));
#endif
						this.Remove(keys[k]);
					}
				} catch (Exception e) {
					LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage(string.Format("Cache.CheckExpirations: k = {0}, Length = {1}, key = {2}",
						                                           k,
						                                           keys.Length,
						                                           keys[k]),
						                             e));
				}
			}
#if (CACHE_TRACE)
		timer.Stop();
		LogManager.GetCurrentClassLogger().Debug(
			debugMessage => debugMessage("CheckExpirations done: count = {0}.  Execute time: {1}", hash.Count, timer.Time));
#endif
		}

		#region Nested type: CacheEnumerator

		/// <summary>
		/// Enumerates the elements of a cache.
		/// </summary>
		protected class CacheEnumerator : IDictionaryEnumerator
		{
			private readonly IDictionaryEnumerator hashEnumerator;


			/// <summary>
			/// Constructs a <b>CacheEnumerator</b> for the given <b>Cache</b>.
			/// </summary>
			/// <param name="cache"></param>
			internal CacheEnumerator(
				Cache cache)
			{
				this.hashEnumerator = cache.hash.GetEnumerator();
			}

			#region IDictionaryEnumerator Members

			/// <summary>
			/// Gets the key of the current cache item.
			/// </summary>
			public object Key
			{
				get { return this.hashEnumerator.Key; }
			}


			/// <summary>
			/// Gets the value of the current cache item.
			/// </summary>
			public object Value
			{
				get
				{
					object obj = null;
					CacheItem item = (CacheItem)this.hashEnumerator.Value;

					if (item != null) {
						obj = item.Object;
					}

					return obj;
				}
			}


			/// <summary>
			/// Gets both the key and the value of the current cache item.
			/// </summary>
			public DictionaryEntry Entry
			{
				get { return new DictionaryEntry(this.Key, this.Value); }
			}

			/// <summary>
			/// Gets the current element in the cache.
			/// </summary>
			public object Current
			{
				get { return this.Entry; }
			}


			/// <summary>
			/// Sets the enumerator to its initial position, which is 
			/// before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				this.hashEnumerator.Reset();
			}


			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns><b>true</b> if the enumerator was successfully advanced to 
			///		the next element; <b>false</b> if the enumerator has passed the end 
			///		of the collection.</returns>
			public bool MoveNext()
			{
				return this.hashEnumerator.MoveNext();
			}

			#endregion
		}

		#endregion
	}
}