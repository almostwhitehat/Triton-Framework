using System;
using System.Configuration;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using Common.Logging;
using Triton.Controller.Publish;
using Triton.Controller.Request;
using Triton.Controller.StateMachine;

namespace Triton.Controller.Command
{

	#region History

	// History:
	// 5/30/2009   KP Changed the logging to Common.Logging
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method

	#endregion

	/// <summary>
	/// <b>TransitionCommand</b> is the command that handles requests that utilize the
	/// state machine.
	/// </summary>
	/// <remarks>
	/// <b>TransitionCommand</b> handles the processing of states and transitions and 
	/// provides hooks in the form of abstract and virtual methods to control the
	/// specific operation.
	///	</remarks>
	///	<author>Scott Dyke</author>
	public class TransitionCommand : Command
	{
		private const string CUR_STATE_NAME = "State";
		private const string EVENT_PARAM_NAME = "e";

		private const string STATE_PARAM_NAME = "st";


		#region Command Members

		/// <summary>
		/// Carries out the actions required to execute the command.
		/// </summary>
		/// <param name="request">The <c>MvcRequest</c> request
		///			that triggered the command to be executed.</param>
		public void Execute(
			MvcRequest request)
		{
			string startEvent = request[EVENT_PARAM_NAME].ToLower();

					//  get the starting start for the request
			IState startState = this.GetStartState(request);
			ContentProvider contentProvider = null;

			try {
				string content = null;
						//  make the context for the request
				TransitionContext context = new TransitionContext(startState, startEvent, ((PageState)startState).Site, request);
						//  make a ContentProvider for the request
				contentProvider = ContentProviderFactory.Make(context);
						//  get a Publisher from the ContentProvider
				Publisher publisher = contentProvider.GetPublisher();

						//  assume we need to perform the transition(s).  if published
						//  content is sucessfully used, we'll set this to false.
				bool doTransitions = true;

						//  make sure we got a publisher
				if (publisher != null) {
							//  generate a publish key and remember it in the context
					context.PublishKey = publisher.MakeKey(context);

							//  determine if we have publihsed content to fulfill this request
					if (publisher.IsPublished(context.PublishKey)) {
								//  if so, get it
						content = contentProvider.GetPublishedContent(context);

								//  if there is no published content, do the transitions
						if (content != null) {
							doTransitions = false;
						}
					}
				}

						//  if we didn't use published content, we need to generate the content
						//  by performing the transitions
				if (doTransitions) {
					//  put the TransitionContext into the request context so Page can get it
					request.Items["transitionContext"] = context;

							//  perform the transition(s)
					StateTransitioner transitioner = new StateTransitioner();
					IState targetState = transitioner.DoTransition(startState, startEvent, context);
							//  target state should be an EndState
					if (targetState is EndState) {
						context.EndState = targetState;
						request.Items["targetStateId"] = targetState.Id;
						request.Items["targetState"] = targetState;
					} else {
//  TODO: what if targgetState is not an EndState?
						LogManager.GetCurrentClassLogger().Warn(
							warn => warn("Target state {0}{1} is not an EndState.",
							             targetState.Id,
							             string.IsNullOrEmpty(targetState.Name) ? "" : " [" + targetState.Name + "]"));
					}

							//  save the ending state to the session and cookie
					try {
						HttpSessionState session = HttpContext.Current.Session;
						if (session != null) {
							session[CUR_STATE_NAME] = targetState.Id;
						}
					} catch {}

					try {
						request.SetResponseCookie(new MvcCookie(CUR_STATE_NAME, targetState.Id.ToString()));
					} catch {}

							//  determine if the content generated to fulfill the request should
							//  be published for use by future requests
					if ((publisher != null) && contentProvider.ShouldBePublished(context)) {
						content = contentProvider.RenderPublishContent(context);
// TODO: what if "content" is not string??
// should RenderPublishContent throw exception if no publish call wanted?
						if (content != null) {
							publisher.Publish(content, context);
						}
					} else {
						content = contentProvider.RenderContent(context);
					}
				}

				if (content != null) {
					request.WriteResponse(content, false);
				}
			} catch (ThreadAbortException) {
				//  ignore this one, since when a .aspx page is creating a new thread
				//  and killing the current thread thus throwing this exception
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("Could not execute the command. "), e);
						//since we cant do anything about this here, rethrow.
				throw;
			} finally {
				if (contentProvider != null) {
					contentProvider.Dispose();
				}
			}
		}

		#endregion


		/// <summary>
		/// Determines whether or not the system should be forced to use dynamic content
		/// regardless of existance of published content.
		/// </summary>
		/// <param name="request">The <b>MvcRequest</b> the determination is being made for.</param>
		/// <returns><b>True</b> to force dynamic content to be returned to the client,
		///		or <b>false</b> if published content can be used if it's available.</returns>
		public virtual bool UseDynamicContent(
			MvcRequest request)
		{
			return false;
		}


		/// <summary>
		/// Determines whether or not the published content should be forced to be regenerated.
		/// </summary>
		/// <param name="request">The <b>MvcRequest</b> the determination is being made for.</param>
		/// <returns><b>True</b> if the published content should be force to be regenerated,
		///		or <b>false</b> if not.</returns>
		public virtual bool ForceRepublish(
			MvcRequest request)
		{
			return false;
		}


		private IState GetStartState(
			MvcRequest request)
		{
			long? curStateId = null;
			bool useStateParam = false;

			//  get the setting from the config for whether or not to get the state
			//  ID from the "st" parameter
			try {
				if (ConfigurationManager.AppSettings["useStateParam"] != null) {
					useStateParam = bool.Parse(ConfigurationManager.AppSettings["useStateParam"]);
				}
			} catch {}

			//  try getting the state from the request, if needed
			if (useStateParam) {
				try {
					curStateId = long.Parse(request[STATE_PARAM_NAME]);
				} catch {}
			}

			//  if we don't have a start state yet, try getting it from session
			if (!curStateId.HasValue) {
				try {
					// TODO: this should NOT reference HttpContext!
					curStateId = long.Parse((string)HttpContext.Current.Session[CUR_STATE_NAME]);
				} catch {}
			}

			//  if we still don't have a start state, try getting it from the cookie
			if (!curStateId.HasValue) {
				try {
					curStateId = long.Parse(request.GetCookie(CUR_STATE_NAME).Value);
				} catch {}
			}

			//  as a last resort, try the parameter if we didn't already
			if ((!curStateId.HasValue) && !useStateParam) {
				try {
					curStateId = long.Parse(request[STATE_PARAM_NAME]);
				} catch {}
			}

			if (!curStateId.HasValue) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("DoTransitionCommand: State or Event missing."));
				throw new ApplicationException("Start State is missing.");
			}

			IState startState = StateManager.GetInstance().GetState(curStateId.Value);

			if (!(startState is StartState)) {
				throw new ApplicationException("State is not a StartState: startState= " + startState.Id);
			}

			return startState;
		}
	}
}