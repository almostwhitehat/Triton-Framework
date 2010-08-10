namespace Triton.Support.Error
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <c>ErrorDictionary</c> is an abstract base class for implementing concrete
	/// error dictionaries.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class ErrorDictionary : IErrorDictionary
	{

		public string Name 
		{ 
			get; 
			set; 
		}


		#region IErrorDictionary Members

		public abstract Error GetError(
			long id);

		#endregion
	}
}