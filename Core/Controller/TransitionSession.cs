using System.Threading;

namespace Triton.Controller
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <c>TransitionSession</c> keeps track of the transitions made by a particular
	/// user during a session.  <c>TransitionSession</c>s are maintained by the
	/// <c>TransitionSessionManager</c>.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class TransitionSession
	{
		//  the default time before a session expires (in minutes)
		private const long DEFAULT_EXPIRE_TIME = 30;

		private readonly string id;

		/// <summary>
		/// The timer used to remove this entry from the session manager.
		/// </summary>
		private readonly Timer sessionTimer;

		private readonly long timeoutTime;
		private string trace = "";


		public TransitionSession(
			string sessionId)
		{
			this.id = sessionId;

			TimerCallback timerDelegate = TransitionSessionManager.Remove;

// TODO: attempt to get expire time from config file and only use default if not available
			//  convert time from minutes to millseconds
			this.timeoutTime = DEFAULT_EXPIRE_TIME*60*1000;
			this.sessionTimer = new Timer(timerDelegate, this.id, this.timeoutTime, 5000);
		}


		/// <summary>
		/// Gets or sets the trace of the transitions for the session.
		/// A trace is of the form {state}->{event}->{state}->{event}->{state}...
		/// </summary>
		public string Trace
		{
			get { return this.trace; }
			set { this.trace = value; }
		}


		/// <summary>
		/// Updates the time the session was last accessed to reset the time-out
		/// timer.
		/// </summary>
		internal void Touch()
		{
			this.sessionTimer.Change(this.timeoutTime, 5000);
		}


		/// <summary>
		/// Frees the resources used by the <c>TransitionSession</c>.
		/// </summary>
		internal void Dispose()
		{
			this.sessionTimer.Dispose();
		}
	}
}