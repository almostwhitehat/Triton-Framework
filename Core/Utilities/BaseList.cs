using System;
using System.Collections;

namespace Triton.Utilities
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Abstract definition for a list.  Subclasses should override <code>add</code> 
	/// and <code>get</code> methods to enforce type checking.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class BaseList : IList
	{
		/**
	 * The base object used for maintaining the list.
	 */
		protected ArrayList theList = new ArrayList();


		//==================================================
		//			ICollection implementation
		//==================================================

		#region IList Members

		public int Count
		{
			get { return this.theList.Count; }
		}


		public bool IsSynchronized
		{
			get { return this.theList.IsSynchronized; }
		}


		public object SyncRoot
		{
			get { return this.theList.SyncRoot; }
		}


		public void CopyTo(
			Array array,
			int index)
		{
			this.theList.CopyTo(array, index);
		}


		//==================================================
		//			IEnumerable implementation
		//==================================================

		public IEnumerator GetEnumerator()
		{
			return this.theList.GetEnumerator();
		}


		//==================================================
		//			IList implementation
		//==================================================

		public bool IsFixedSize
		{
			get { return this.theList.IsFixedSize; }
		}


		public bool IsReadOnly
		{
			get { return this.theList.IsReadOnly; }
		}


		public object this[int index]
		{
			get { return this.theList[index]; }
			set { this.theList[index] = value; }
		}


		public int Add(
			object value)
		{
			return this.theList.Add(value);
		}


		public void Clear()
		{
			this.theList.Clear();
		}


		public bool Contains(
			object value)
		{
			return this.theList.Contains(value);
		}


		public int IndexOf(
			object value)
		{
			return this.theList.IndexOf(value);
		}


		public void Insert(
			int index,
			object value)
		{
			this.theList.Insert(index, value);
		}


		public void Remove(
			object value)
		{
			this.theList.Remove(value);
		}


		public void RemoveAt(
			int index)
		{
			this.theList.RemoveAt(index);
		}

		#endregion
	}
}