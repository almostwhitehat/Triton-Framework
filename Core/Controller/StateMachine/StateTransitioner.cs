using System;
using System.Configuration;
using Common.Logging;

namespace Triton.Controller.StateMachine {

#region History

// History:
// 06/02/2009	KP	Changed the logging to Common.logging
// 06/22/2009	SD	Added try/catch around TransitionSessionManager.AddEvent -- we don't want an
//					error in transition tracing to cause the request processing to fail.
// 08/17/2009	SD	Changed DoTransition to use a loop rather than recursion to iterate through
//					a series of transitions to avoid stack overflow errors with very large numbers
//					of transitions.
// 09/15/2009	SD	Updated DoTransition to use the new config options for determining tracing of
//					state transitions via TransitionSessionManager and log file.
// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
// 11/16/2009	GV	Fixed the logging to include the inner exception. Removed unnessesary commented out old code
//					Added a throw at the global try catch.
// 02/18/2011	SD	Added support for default transitions.
// 11/18/2011	SD	Added support for state prerequisites.
// 12/20/2011	SD	Added transContext.SetCurrentState after the execution of prerequisites so that the correct
//					state will be in transContext.CurrentState when it is executed.

#endregion

/// <summary>
/// Summary description for StateTransitioner.
/// </summary>
///	<author>Scott Dyke</author>
public class StateTransitioner
{
	private const string STATES_CONFIG_PATH = "controllerSettings/states";


	/// <summary>
	/// Performs a transition from the given state to the start indicated by
	/// the transition defined by the state map matching the given event.
	/// </summary>
	/// <param name="startState">The state being transitioned from.</param>
	/// <param name="eventName">The event causing the transition.</param>
	/// <param name="transContext">The context of the request the transition is being
	///			performed for.</param>
	/// <returns>A <c>string</c> containing the name of the target of the transition.
	///			This may be a page name if the resulting state is a <c>PageState</c>,
	///			or an action name if the resulting state is a <c>ActionState</c></returns>
	public IState DoTransition(
		IState startState,
		string eventName,
		TransitionContext transContext)
	{
		IState targetState = null;
		StateManager sm = StateManager.GetInstance();
		string evnt = eventName;

				//  get the settings from config file
		StatesConfigHandler.StatesConfig statesConfig =
			(StatesConfigHandler.StatesConfig)ConfigurationManager.GetSection(STATES_CONFIG_PATH);

		do {
			try {
				if (statesConfig.ManagerTrace) {
					TransitionSessionManager.AddEvent(transContext.Request.IP, startState.Id, evnt);
				}

						//  get the transition for the given state and event
				Transition trans = startState.GetTransition(evnt);

						//  if we didn't find a transition for the real event and there is
						//  a default event defined, try for a transition for the default event
				if ((trans == null) && (statesConfig.DefaultEventName != null)) {
					LogManager.GetCurrentClassLogger().Info(msg => msg(string.Format(
							"No transition found for '{0}' on state {1}.  Attempting default ['{2}'].", evnt, startState.Id, statesConfig.DefaultEventName)));
					trans = startState.GetTransition(statesConfig.DefaultEventName);
				}

						//  make sure we found a transition matching the given event
				if (trans == null) {
					// TODO: this should be a custom Exception type
					throw new ApplicationException(string.Format(
						   	"No transition found: state= {0} event= {1} [{2}]",
						   	startState.Id, evnt, TransitionSessionManager.GetTrace(transContext)));
				}

						//  get the State that is the destination of the tranistion
				targetState = sm.GetState(trans.ToState);

				if (targetState == null) {
					// TODO: this should be a custom Exception type
					throw new ApplicationException(string.Format(
						   	"State not found: state= {0} [{1}]",
						   	trans.ToState, TransitionSessionManager.GetTrace(transContext)));
				}

						//  log the transition if logging is on
				if (statesConfig.LogTrace) {
					LogManager.GetCurrentClassLogger().Debug(
						traceMessage => traceMessage("Transition: {0} {1} -> {2} -> {3} {4}",
								 startState.Id,
								 string.IsNullOrEmpty(startState.Name) ? "" : "[" + startState.Name + "]",
								 evnt,
								 targetState.Id,
								 string.IsNullOrEmpty(targetState.Name) ? "" : "[" + targetState.Name + "]"));
				}

						//  set the current state that is being processed
				transContext.SetCurrentState(targetState);

						//  handle the state
				try {
							//  if there is a prerequisite on the state, recursively call DoTransition
							//  for the prerequisite
					if (targetState.HasPrerequisite) {
						for (int p = 0; p < targetState.Prerequisite.Length; p++) {
							LogManager.GetCurrentClassLogger().Debug(traceMessage => traceMessage(
									"Begin prerequisite {0}", targetState.Prerequisite[p].Name));
							DoTransition(sm.GetState(targetState.Prerequisite[p].StartStateId), targetState.Prerequisite[p].StartEvent, transContext);
							LogManager.GetCurrentClassLogger().Debug(traceMessage => traceMessage(
									"End prerequisite {0}", targetState.Prerequisite[p].Name));
						}

								//  need to reset the current state here since it will be the end state of the prerequisite otherwise
						transContext.SetCurrentState(targetState);
						// ?? what if ended on page state
					}

					evnt = targetState.Execute(transContext);
				} catch (Exception ex) {
					throw new ApplicationException(string.Format("Error executing state {0}.",
							targetState.Id), ex);
				}

						//  the destination state becomes the start state for the next iteration
				startState = targetState;
			} catch (Exception e) {
						//  to exit the loop if something goes wrong
				evnt = null;
				LogManager.GetCurrentClassLogger().Error(errorMessage => errorMessage(e.Message), e);

				throw;
			}

				//  if we got an event back from the execution of the state, it
				//  means continue processing states
		} while (evnt != null);

		return targetState;
	}
}
}