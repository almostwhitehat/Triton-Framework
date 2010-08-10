using System;
using System.Collections;

namespace Triton.Support.Error
{

	#region History

	// History:
	// 11/29/08 GNV	Added a property Instance to get the instance of the Manager.

	#endregion

	/// <summary>
	/// <c>DictionaryManager</c> is a singleton that manages <c>ErrorDictionary</c>s.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class DictionaryManager
	{
		private static DictionaryManager instance;

		private readonly Hashtable dictionaryCollection = Hashtable.Synchronized(new Hashtable());
		                           // use a thread-safe wrapper


		private DictionaryManager() {}

		public static DictionaryManager Instance
		{
			get { return GetDictionaryManager(); }
		}


		/// <summary>
		/// Returns the singleton instance of the <c>DictionaryManager</c>.
		/// </summary>
		/// <returns><c>DictionaryManager</c> instance</returns>
		public static DictionaryManager GetDictionaryManager()
		{
			if (instance == null) {
				instance = new DictionaryManager();
			}

			return instance;
		}


		/// <summary>
		/// Add a error dictionary to the DictionaryManager.
		/// </summary>
		/// <param name="dict">The IErrorDictionary to add to the manager.</param>
		/// <returns><c>True</c> if the add was successful, <c>false</c> if not.</returns>
		public bool AddDictionary(
			ErrorDictionary dict)
		{
			bool returnVal = false;

			// Avoid ArgumentException if the key already exists in the Hashtable
			if (!this.dictionaryCollection.Contains(dict.Name)) {
				try {
					this.dictionaryCollection.Add(dict.Name, dict);
					returnVal = true;
				} catch (NotSupportedException e) {
					//  TODO: shoudl this be logged??
					// In case Hashtable could become read-only or has a fixed size
				}
			}

			return returnVal;
		}


		/// <summary>
		/// Gets the dictionary with the specified name.
		/// </summary>
		/// <param name="dictionaryName">The name of the dictionary to get.</param>
		/// <returns>The dictionary for the given name, or null if no dictionary
		///			exists in the manager with that name.</returns>
		public ErrorDictionary GetDictionary(
			String dictionaryName)
		{
			return (this.dictionaryCollection.Contains(dictionaryName))
			       	? (ErrorDictionary) this.dictionaryCollection[dictionaryName]
			       	: null;
		}


		/// <summary>
		/// Resets the DictionaryManager.  This clears any existing dictionaries from
		/// from the manager.
		/// </summary>
		public void Reset()
		{
			this.dictionaryCollection.Clear();
			instance = null;
		}
	}
}