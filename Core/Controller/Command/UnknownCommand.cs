using Triton.Controller.Request;

namespace Triton.Controller.Command
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>UnknownCommand</b> is a "dummy" Command that does nothing.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class UnknownCommand : Command
	{
		#region Command Members

		public void Execute(
			MvcRequest request) {}

		#endregion
	}
}