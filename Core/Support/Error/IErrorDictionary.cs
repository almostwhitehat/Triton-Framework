namespace Triton.Support.Error
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <c>IErrorDictionary</c> interface defines the interface for a dictionary
	/// of error messages.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface IErrorDictionary
	{
		Error GetError(
			long id);
	}
}