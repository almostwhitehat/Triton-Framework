using System;
using System.Collections;
using Common.Logging;

namespace Triton.Controller {

	#region History

	// History:
	//  08/17/09 - SD -	Changed the "sessions" Hashtable to be Hashtable.Synchronized to
	//					avoid potential threading issues if the clean up thread runs and
	//					removes items while other threads are trying to add items.
	//				  -	Added try/catch in AddEvent and Remove methods.
	//  09/29/09 - KP - Renamed logging methods to use GetCurrentClassLogger method
	#endregion

	/// <summary>
	/// <c>TransitionSessionManager</c> manages the <c>TransitionSession</c>s.
	/// It uses the IP address of the request for the session ID.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class TransitionSessionManager
	{
		//  the hashtable used to maintain the collection of TransitionSessions
		//  keyed on session ID
		private static readonly Hashtable sessions = Hashtable.Synchronized(new Hashtable());


		/// <summary>
		/// Default constructor.
		/// </summary>
		private TransitionSessionManager() {}


		/// <summary>
		/// Indexer to get a <c>TransitionSession</c> for a given session ID.
		/// </summary>
		public TransitionSession this[string sessionId]
		{
			get {
				return (TransitionSession)sessions[sessionId];
			}
		}


		/// <summary>
		/// Adds the given state/event information to the trace of the specified
		/// session.
		/// </summary>
		/// <param name="sessionId">The session ID of the session to add the state/event
		///			information to.</param>
		/// <param name="state">The state the event occurred in.</param>
		/// <param name="theEvent">The event.</param>
		public static void AddEvent(
			string sessionId,
			long state,
			string theEvent)
		{

			try {
				//  try to get the session from the collection
				TransitionSession session = (TransitionSession)sessions[sessionId];

				//  if there was no session for the given ID, make a new one,
				//  otherwise update the last accessed time of the session
				if (session == null) {
					session = new TransitionSession(sessionId);
					sessions[sessionId] = session;
				} else {
					session.Touch();
				}

				//  add the given state & event to the session's trace
				session.Trace += state + "->" + theEvent + "->";
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error("AddEvent failed: ", e);
			}
		}


		/// <summary>
		/// Removes a <c>TransitionSession</c> from the collection.  This is called
		/// by the time-out timer of the <c>TransitionSession</c> to remove itself
		/// from the manager.
		/// </summary>
		/// <param name="stateInfo">The session ID of the session to remove.</param>
		internal static void Remove(
			object stateInfo)
		{
			string sessionId = (string)stateInfo;

			try {
				TransitionSession session = (TransitionSession)sessions[sessionId];
				LogManager.GetCurrentClassLogger().Info(
						infoMessage => infoMessage("Removing {0} from session: {1}",sessionId, session.Trace));

				session.Dispose();

				sessions.Remove(sessionId);
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error("Remove failed: ", e);
			}
		}


		/// <summary>
		/// Gets the transition trace for the session associated with the given
		/// <c>TransitionContext</c>.
		/// </summary>
		/// <param name="context">The <c>TransitionContext</c> to get the trace for.</param>
		/// <returns>A <c>string</c> containing the trace of tranistions for the session
		///			associated with the given <c>TransitionContext</c>.</returns>
// change to internal for 2.0
//	internal static string GetTrace(
		public static string GetTrace(
			TransitionContext context)
		{
			string retVal = "";
			//  get the IP from the context -- used as the session ID
			string ip = context.Request.IP;

			try {
				retVal = ((TransitionSession)sessions[ip]).Trace;
			} catch (Exception) {}

			return retVal;
		}
	}
}