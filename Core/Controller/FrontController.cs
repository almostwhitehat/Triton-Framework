using System;
using System.Web;
using System.Web.SessionState;
using Triton.Controller.Request;

namespace Triton.Controller
{

	#region History

	// History:
	// 5/6/2009	GNV	Removed ValidateInput method call on the request, since this overwrites the setting
	//				in the web config file and on the page. This setting should be handled by the application.
	//				pages validateRequest="false" is the setting.

	#endregion

	/// <summary>
	/// <b>FrontController</b> gets incoming requests from IIS and delegates processing
	/// to the appropriate Command.
	/// </summary>
	/// <remarks>
	/// IDEAS:
	/// In the DAOs, rather than always retrieving all fields, specify fields to
	/// be retrieved by each DAO in config file.
	/// </remarks>
	///	<author>Scott Dyke</author>
	public class FrontController : BaseController, IHttpHandler, IRequiresSessionState
	{
		private const string MVC_REQUEST = "MVCRequest";

		#region IHttpHandler Members

		/// <summary>
		/// Processes an HTTP Web request.
		/// </summary>
		/// <param name="context">An <c>HttpContext</c> object that provides
		///			references to the intrinsic server objects (for example, 
		///			Request, Response, Session, and Server) used to service 
		///			HTTP requests.</param>
		public void ProcessRequest(
			HttpContext context)
		{
			//context.Request.ValidateInput();

			//  record the time the request was received
			context.Items["startTime"] = DateTime.Now;

			MvcRequest request = RequestFactory.Make(context);
			context.Items[MVC_REQUEST] = request;

			ProcessRequest(request);
		}


		/// <summary>
		/// Returns <c>true</c> if the instance is reusable, <c>false</c> otherwise.
		/// </summary>
		public bool IsReusable
		{
			get { return true; }
		}

		#endregion
	}
}