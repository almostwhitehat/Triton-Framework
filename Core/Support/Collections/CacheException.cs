using System;

namespace Triton.Support.Collections
{
	/// <summary>
	/// The exception that is thrown when a Cache error occurs.
	/// </summary>
	public class CacheException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the CacheException 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public CacheException(
			string message)
			: base(message) {}


		/// <summary>
		/// Initializes a new instance of the CacheException 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">Inner exception that occured.</param>
		public CacheException(string message,
		                      Exception innerException)
			: base(message, innerException) {}
	}
}