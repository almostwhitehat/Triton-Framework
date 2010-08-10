namespace Triton.Logging
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Specifies the contract inherited filters have to follow.  Inherited classes represent special 
	/// conditions that can be applied to <c>Logger</c> objects to add custom validation/checking.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface ILogFilter
	{
		/// <summary>
		/// Determines if the <c>LogMessage</c> allowed to pass to the data source
		/// </summary>
		/// <param name="message"><c>LogMessage</c> that is to be sent to the data source</param>
		/// <returns><c>bool</c> indicating if the <c>LogMessage</c> is to be sent to the data source</returns>
		bool IsLoggable(
			LogMessage message);
	}
}