using Triton.Controller.Request;

namespace Triton.Controller.Command
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The Command interface defines the API for a command used by the 
	/// FrontController.  A Command contains a specific unit of business
	/// logic.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface Command
	{
		/// <summary>
		/// Carries out the actions required to execute the command.
		/// </summary>
		/// <param name="request">The <c>MvcRequest</c> request
		///			that triggered the command to be executed.</param>
		void Execute(
			MvcRequest request);
	}
}