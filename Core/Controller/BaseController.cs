using Common.Logging;
using Triton.Controller.Command;
using Triton.Controller.Request;

namespace Triton.Controller
{

	#region History

	// History:
	// 6/2/2009		KP  Changed the logging to Common.logging.
	// 08/12/2009	GV	Rename of BizAction back to Action
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// <b>BaseController</b> is the base front controller for handling requests using either 
	/// <b>Command</b>s or <b>Action</b>s.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class BaseController
	{
		private const string EVENT_PARAM = "e";
		private const string STATE_PARAM = "st";
		private const string TRANSITION_COMMAND = "TransitionCommand";


		/// <summary>
		/// Processes a request.
		/// </summary>
		/// <param name="request">The <c>MvcRequest</c> object representing the
		///			request to process.</param>
		public void ProcessRequest(
			MvcRequest request)
		{
			//  if the request has the state (st) and event (e) parameters,
			//  assume we are performing a state transition
			Command.Command cmd = request[EVENT_PARAM] != null
			                      	? CommandFactory.Make(TRANSITION_COMMAND)
			                      	: CommandFactory.Make(request);

			if (cmd == null) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("FrontController.ProcessRequest: could not make Command."));
			} else {
				cmd.Execute(request);
			}
		}
	}
}