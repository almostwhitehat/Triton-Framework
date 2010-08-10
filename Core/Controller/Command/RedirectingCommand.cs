using System;
using System.Threading;
using Triton.Controller.Request;
using Triton.Logging;

namespace Triton.Controller.Command
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <c>RedirectingCommand</c> is an abstract class that any command that requires
	/// a redirect to a target page can inherit from.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class RedirectingCommand : Command
	{
		private const string LOGGER = "ControllerLogger";

		private bool doRedirect = true;
		private string targetPage;

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>change to internal for 2.0</remarks>
		public bool DoRedirect
		{
			get { return this.doRedirect; }
			set { this.doRedirect = value; }
		}

		#region Command Members

		/// <summary>
		/// Calls the sub class's <c>OnExecute</c> method to perform the desired
		/// action, then transfers control to the target page for rendering.
		/// </summary>
		/// <param name="request">The <c>MvcRequest</c> the command is being
		///			executed for.</param>
		public void Execute(
			MvcRequest request)
		{
			//  perform any pre-execute initialization
			this.Init(request);

			//  call the abstract OnExecute implemented by the sub-class
			this.OnExecute(request);

			if (this.doRedirect) {
				//  get the target page
				this.targetPage = this.GetTargetPage(request);

				//  get the path of the content for the target page
				PageFinder.FileRecord xmlFileRec = this.GetXmlPath(request);

				//  put the XML file info into the request context
				//  so the page can get it
				if (xmlFileRec != null) {
					request.Items["pageXml"] = xmlFileRec.fullPath; // for backward compatibility
					request.Items["pageXmlFile"] = xmlFileRec;
				}

				//  transfer control to the target page
#if (DEBUG)
				this.Transfer(request);
#else

				try {
					this.Transfer(request);
					//  Server.Transfer always throws a ThreadAbortException on successful
					//  completion, so we ignore it here
				} catch (ThreadAbortException) {} catch (Exception e) {
					Logger.GetLogger(LOGGER).ReportInnerExceptions = true;
					Logger.GetLogger(LOGGER).Error("RedirectingCommand.Execute - error Transferring: ", e);
// TODO: transfer to error page??
				}
#endif
			}
		}

		#endregion

		/// <summary>
		/// Carries out the actions taken by the command.
		/// Classes that inherit from RedirectingCommand must implement
		/// this method.
		/// </summary>
		/// <param name="request">The <c>MvcRequest</c> the command is being
		///			executed for.</param>
		protected abstract void OnExecute(
			MvcRequest request);


		/// <summary>
		/// <b>Init</b> gets called before <b>OnExecute</b> to perform any initialization
		/// tasks required before the command is executed.  By default <b>Init</b> does nothing.
		/// Subclasses can provide an implementation for this method.
		/// </summary>
		/// <param name="request">The <b>MvcRequest</b> the command is being initialized for.</param> 
		protected virtual void Init(
			MvcRequest request) {}


		/// <summary>
		/// Transfer control to the target page of the request for rendering of the results.
		/// </summary>
		/// <param name="request">The <c>MvcRequest</c> the transfer is being
		///			executed for.</param>
		protected virtual void Transfer(
			MvcRequest request)
		{
			//  get the URL to redirect to
			string targetUrl = this.GetRedirectUrl(request);

			request.Transfer(targetUrl);
		}


		/// <summary>
		/// Gets the name of the target page for the command.
		/// </summary>
		/// <param name="request">The <c>MvcRequest</c> the command is being
		///			executed for.</param>
		/// <returns>The name of the target page for the command.</returns>
		protected virtual String GetTargetPage(
			MvcRequest request)
		{
//  TODO: what if no "p"??
			return request["p"];
		}


		/// <summary>
		/// Returns the URL of the target page to render the results of the command.
		/// This is used by the <c>Execute</c> method to determine where to redirect to.
		/// This method can be overridden to change the default implementation for
		/// finding the target page.
		/// </summary>
		/// <param name="request">The <c>MvcRequest</c> the command is being
		///			executed for.</param>
		/// <returns>The URL of the target page for the command.</returns>
		protected virtual String GetRedirectUrl(
			MvcRequest request)
		{
			string url = null;

			PageFinder.FileRecord fileRec = PageFinder.GetInstance().FindPage(this.targetPage, request["s"], request["site"]);

			if (fileRec != null) {
				request.Version = fileRec.version;
				url = fileRec.fullPath;
			}

			return url;
		}


		/// <summary>
		/// Returns the path to the XML file containing the content for the target page.
		/// This mehotd is used by the <c>Execute</c> method to determine the location
		/// of the XML content file for the target page.  If an inherited class
		/// overrides <c>GetRedirectUrl</c>, it should also override this method
		/// to provide a path to the corresponding content file.
		/// This method can be overridden to change the default implementation for
		/// finding the XML content file for the target page.
		/// </summary>
		/// <param name="request">The <c>MvcRequest</c> the command is being
		///			executed for.</param>
		/// <returns>The path of the target page's XML content file, relative to the
		///			application root directory.</returns>
		protected virtual PageFinder.FileRecord GetXmlPath(
			MvcRequest request)
		{
			return (PageFinder.GetInstance().FindXml(this.targetPage, request["s"], request["site"]));
		}
	}
}