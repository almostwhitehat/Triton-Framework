using System.Collections;
using System.Collections.Specialized;

namespace Triton.Validator.Model
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>ValidationErrorCollection</b> is a name-indexed collection of
	/// <b>ValidationError</b>s.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ValidationErrorCollection : IEnumerable
	{
		private Hashtable collection = CollectionsUtil.CreateCaseInsensitiveHashtable();


		/// <summary>
		/// Indexer to get the <b>ValidationError</b> for the specified field.
		/// </summary>
		public ValidationError this[
			string field]
		{
			get { return (ValidationError) this.collection[field]; }
		}


		/// <summary>
		/// Gets the number of <b>ValidationError</b>s in the collection.
		/// </summary>
		public int Count
		{
			get { return this.collection.Count; }
		}

		#region IEnumerable Members

		/// <summary>
		/// Gets a <b>ValidationErrorCollectionEnumerator</b> to enumerate through
		/// the collection.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return new ValidationErrorCollectionEnumerator(this.collection.GetEnumerator());
		}

		#endregion

		/// <summary>
		/// Adds a <b>ValidationError</b> to the collection.  If there is
		/// already a <b>ValidationError</b> for the same field in the collection,
		/// it is replaced with the new one.
		/// </summary>
		/// <param name="error"></param>
		public void Add(
			ValidationError error)
		{
			if (error != null) {
//			collection.Add(error.Field, error);
				this.collection[error.Field ?? string.Empty] = error;
			}
		}


		/// <summary>
		/// Clears all entries from the collection.
		/// </summary>
		public void Clear()
		{
			this.collection.Clear();
		}
	}

	/// <summary>
	/// A <b>ValidationErrorCollectionEnumerator</b> is an <c>IEnumerator</c> to
	/// enumerate a <b>ValidationErrorCollection</b>.
	/// </summary>
	public class ValidationErrorCollectionEnumerator : IEnumerator
	{
		private IDictionaryEnumerator baseEnumerator;


		/// <summary>
		/// Constructs a new <b>ValidationErrorCollectionEnumerator</b> as an adaptor
		/// on the given <c>IDictionaryEnumerator</c>.
		/// </summary>
		/// <param name="baseEnum">The underlying <c></c></param>
		internal ValidationErrorCollectionEnumerator(
			IDictionaryEnumerator baseEnum)
		{
			this.baseEnumerator = baseEnum;
		}

		#region IEnumerator Members

		/// <summary>
		/// Resets the enumerator back to before the beginning of the collection.
		/// </summary>
		public void Reset()
		{
			this.baseEnumerator.Reset();
		}


		/// <summary>
		/// Gets the curent item from the collection.
		/// </summary>
		public object Current
		{
			get
			{
				ValidationError retVal = (this.baseEnumerator.Current == null)
				                         	?
				                         		null
				                         	: (ValidationError) ((DictionaryEntry) this.baseEnumerator.Current).Value;

				return retVal;
			}
		}


		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns><b>true</b> if the enumerator was successfully advanced to 
		/// the next element; <b>false</b> if the enumerator has passed the end 
		/// of the collection.</returns>
		public bool MoveNext()
		{
			return this.baseEnumerator.MoveNext();
		}

		#endregion
	}
}