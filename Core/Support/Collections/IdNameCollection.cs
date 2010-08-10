using System;
using System.Collections;
using System.Collections.Specialized;

namespace Triton.Support.Collections
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>GetIdNameEntryName</b> delegate defines a method to get
	/// the name of an entry in the collection.
	/// </summary>
	public delegate string GetIdNameEntryName(object obj);

	/// <summary>
	/// The <b>GetIdNameEntryId</b> delegate defines a method to get
	/// the ID of an entry in the collection.
	/// </summary>
	public delegate long GetIdNameEntryId(object obj);

	/// <summary>
	/// <b>IdNameCollection</b> is a collection of objects that can be indexed by
	/// <i>ID</i> and <i>name</i>.  The ID and name used by the collection are
	/// identified by the GetIdNameEntryName and GetIdNameEntryId delegates specified
	/// when the collection is instantiated.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class IdNameCollection : IEnumerable
	{
		/// <summary>
		/// The GetIdNameEntryId to get the ID of an entry
		/// </summary>
		private readonly GetIdNameEntryId idGetter;

		/// <summary>
		/// Hashtable of items keyed on ID.
		/// </summary>
		private readonly Hashtable idHash = Hashtable.Synchronized(new Hashtable());

		/// <summary>
		/// The GetIdNameEntryName to get the name of an entry
		/// </summary>
		private readonly GetIdNameEntryName nameGetter;

		/// <summary>
		/// Hashtable of items keyed on name.
		/// </summary>
		private readonly Hashtable nameHash = Hashtable.Synchronized(CollectionsUtil.CreateCaseInsensitiveHashtable());


		/// <summary>
		/// Constructs a new <b>IdNameCollection</b> with the given delegates for getting
		/// an entry's ID and name.
		/// </summary>
		/// <param name="idGetter">The <b>GetIdNameEntryId</b> delegate to get IDs for collection entries.</param>
		/// <param name="nameGetter">The <b>GetIdNameEntryName</b> delegate to get names for collection entries.</param>
		public IdNameCollection(
			GetIdNameEntryId idGetter,
			GetIdNameEntryName nameGetter)
		{
			this.idGetter = idGetter;
			this.nameGetter = nameGetter;
		}


		/// <summary>
		/// Indexer to get an entry based on its id.
		/// </summary>
		public object this[long id]
		{
			get { return this.idHash[id]; }
			set
			{
				this.idHash[id] = value;
				this.nameHash[this.nameGetter(value)] = value;
			}
		}


		/// <summary>
		/// Indexer to get an entry based on its name.
		/// </summary>
		public object this[string name]
		{
			get { return this.nameHash[name]; }
			set
			{
				this.idHash[this.idGetter(value)] = value;
				this.nameHash[name] = value;
			}
		}


		/// <summary>
		/// Gets the number of entries contained in the <b>IdNameCollection</b>.
		/// </summary>
		public int Count
		{
			get { return this.idHash.Count; }
		}


		/// <summary>
		/// Gets the ids for all the attributes in the <b>IdNameCollection</b>.
		/// </summary>
		public ICollection Ids
		{
			get { return this.idHash.Keys; }
		}


		/// <summary>
		/// Gets the names for all the attributes in the <b>IdNameCollection</b>.
		/// </summary>
		public ICollection Names
		{
			get { return this.nameHash.Keys; }
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return new IdNameCollectionEnumerator(this.idHash.GetEnumerator());
		}

		#endregion

		/// <summary>
		/// Adds a new entry to the collection.
		/// </summary>
		/// <param name="entry">The <c>object</c> to add to the collection.</param>
		public void Add(
			object entry)
		{
			long id = this.idGetter(entry);
			string name = this.nameGetter(entry);

			if (this.idHash.Contains(id)) {
				throw new ApplicationException(
					string.Format("IdNameCollection already contains an item with ID '{0}'.", id));
			}
			if (this.nameHash.Contains(name)) {
				throw new ApplicationException(
					string.Format("IdNameCollection already contains an item with name '{0}'.", name));
			}

			this.idHash[id] = entry;
			this.nameHash[name] = entry;
		}
	}

	public class IdNameCollectionEnumerator : IEnumerator
	{
		private readonly IDictionaryEnumerator baseEnumerator;


		public IdNameCollectionEnumerator(
			IDictionaryEnumerator baseEnum)
		{
			this.baseEnumerator = baseEnum;
		}

		#region IEnumerator Members

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		public void Reset()
		{
			this.baseEnumerator.Reset();
		}


		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get
			{
				object val = (this.baseEnumerator.Current == null) ? null : ((DictionaryEntry) this.baseEnumerator.Current).Value;
				return val;
			}
		}


		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns><b>true</b> if the enumerator was successfully advanced to the next element; 
		///			<b>false</b> if the enumerator has passed the end of the collection.</returns>
		public bool MoveNext()
		{
			return this.baseEnumerator.MoveNext();
		}

		#endregion
	}
}