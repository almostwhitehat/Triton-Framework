using System;

namespace Triton.Support.Collections
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>CacheItem</b> is an item maintained by the <b>Cache</b> class.  It is
	/// not intended for external use.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class CacheItem
	{
		/// <summary>
		/// The time at which the item was added.
		/// </summary>
		private readonly DateTime addedAt;

		/// <summary>
		/// A reference to the application object maintained by the CacheItem.
		/// </summary>
		private readonly object refObject;


		/// <summary>
		/// Constructs a new <b>CacheItem</b> for the given object and
		/// expiration time.
		/// </summary>
		/// <param name="theObject"></param>
		/// <param name="expiresAt"></param>
		internal CacheItem(
			object theObject,
			DateTime expiresAt)
		{
			this.refObject = theObject;
			this.ExpiresAt = expiresAt;
			this.addedAt = DateTime.Now;
		}


		/// <summary>
		/// Gets or sets the time at which the item expires.
		/// </summary>
		internal DateTime ExpiresAt { get; set; }


		/// <summary>
		/// Gets the time at which the item was created.
		/// </summary>
		internal DateTime AddedAt
		{
			get { return this.addedAt; }
		}


		/// <summary>
		/// Gets the underlying application object.
		/// </summary>
		internal object Object
		{
			get { return this.refObject; }
		}
	}
}