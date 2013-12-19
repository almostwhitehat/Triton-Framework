using System;
using System.Web;
using Triton.Utilities;
using System.Diagnostics;

namespace Triton.Controller
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>TimerModule</b> is an HttpModule that tracks the amount of time a request
	/// takes to process.
	/// </summary>
	/// <remarks>
	/// <b>TimerModule</b> attaches to the application BeginRequest and EndRequest
	/// events.  On BeginRequest, it creates and starts a MvcTimer and attaches it
	/// to the request.  On EndRequest, it retrieves the MvcTimer from the request
	/// and includes the elapsed time in the request response.
	///	</remarks>
	///	<author>Scott Dyke</author>
	public class TimerModule : IHttpModule
	{
		/// <summary>
		/// The name of the request Item to store the timer as.
		/// </summary>
		private const string TIMER_NAME = "timer";

		#region IHttpModule Members

		/// <summary>
		/// Implementation of IHttpModule.Init.  Sets the event handlers for BeginRequest
		/// and EndRequest to start and stop the timer.
		/// </summary>
		/// <param name="app">The HttpApplication processing the request.</param>
		public void Init(
			HttpApplication app)
		{
			app.BeginRequest += this.StartTimer;
			app.EndRequest += this.StopTimer;
		}


		/// <summary>
		/// Implementation of IHttpModule.Dispose.  No unmanaged code that needs disposing.
		/// </summary>
		public void Dispose() {}

		#endregion

		/// <summary>
		/// The event handler for the BeginRequest event. Creates a new MvcTimer, 
		/// starts it, and attaches it to the current request.
		/// </summary>
		/// <param name="s">The application the event is for.</param>
		/// <param name="e">unused</param>
		public void StartTimer(
			object s,
			EventArgs e)
		{
			Stopwatch timer = new Stopwatch();
			timer.Start();
			HttpContext.Current.Items[TIMER_NAME] = timer;
		}


		/// <summary>
		/// The event handler for the EndRequest event. Gets the MvcTimer from the
		/// request, stops it, and write the elapsed time to the response.
		/// </summary>
		/// <param name="s">The application the event is for.</param>
		/// <param name="e">unused</param>
		public void StopTimer(
			object s,
			EventArgs e)
		{
			HttpApplication app = s as HttpApplication;

			try {
				Stopwatch timer = HttpContext.Current.Items[TIMER_NAME] as Stopwatch;
				timer.Stop();

				string ip = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];

				app.Context.Response.Write(string.Format("{0}<!--/* {1} [{2}] */-->",
				                                         Environment.NewLine,
				                                         timer.ElapsedMilliseconds,
				                                         ip));
			} catch {}
		}
	}
}