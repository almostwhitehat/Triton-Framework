using System.Collections;

namespace Triton.Support.Session
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISessionState : ICollection, IEnumerable
	{
		/// <summary>
		/// Gets or sets the value of an item in the cache with the given key.
		/// </summary>
		object this[string key]
		{
			get; 
			set; 
		}
	}

}
