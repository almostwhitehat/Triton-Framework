using System;
using System.Collections;

namespace Triton.Logging
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// LogManager is a Singleton.  It is used to keep track of unique Logger objects.  To retrieve a Logger
	/// of given name, call LogManager.GetLogManager().GetLogger(loggerName).  If found in the internal 
	/// Hashtable, it is returned, else null.  To add a logger to the collection, call LogManager.GetLogManager().AddLogger(Logger).
	/// To clear all Logger objects, call LogManager.GetLogManager().Reset().
	/// </summary>
	///	<author>Scott Dyke</author>
	public class LogManager
	{
		private static LogManager instance;
		private readonly Hashtable loggerCollection = Hashtable.Synchronized(new Hashtable()); // Uses a thread-safe wrapper


		protected LogManager() {}


		/// <summary>
		/// Returns a <c>LogManager</c> object.
		/// </summary>
		/// <returns><c>LogManager</c> instance</returns>
		public static LogManager GetLogManager()
		{
			if (instance == null) {
				instance = new LogManager();
			}

			return instance;
		}


		/// <summary>
		/// Adds a <c>Logger</c> object to the loggerCollection.
		/// </summary>
		/// <param name="log"><c>Logger</c> to add to the loggerCollection</param>
		/// <returns><c>bool</c> indicating if the <c>Logger</c> was added, if already existing, false is returned</returns>
		public bool AddLogger(
			Logger log)
		{
			bool returnVal = false;

			// Avoid ArgumentException if the key already exists in the Hashtable
			if (!this.loggerCollection.Contains(log.Name)) {
				try {
					this.loggerCollection.Add(log.Name, log);
					returnVal = true;
				} catch (NotSupportedException) {
					// In case Hashtable could become read-only or has a fixed size
				}
			}

			return returnVal;
		}


		/// <summary>
		/// Returns a <c>Logger</c> object of the desired name.
		/// </summary>
		/// <param name="loggerName"><c>String</c> indicating the name of the <c>Logger</c> to return</param>
		/// <returns><c>Logger</c> with the requested name if available, null otherwise</returns>
		public Logger GetLogger(
			String loggerName)
		{
			return (this.loggerCollection.Contains(loggerName)) ? (Logger) this.loggerCollection[loggerName] : null;
		}


		/// <summary>
		/// Clears the loggerCollection.
		/// </summary>
		public void Reset()
		{
			this.loggerCollection.Clear();
			instance = null;
		}
	}
}