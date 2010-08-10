using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Controller.StateMachine;
using Triton.Logic.Support;
using System.Text.RegularExpressions;
using System.Linq;

namespace Triton.Logic
{

	#region History

	// History:
	// 05/20/2009	KP	Changed the Logging to use Common.Logging
	// 08/12/2009	GV	Rename of BizAction to IAction
	// 09/20/2009	GV	Moved to Logic namespace
	// 09/29/2009	KP	Changed logging methods to GetClassLogger
	// 11/12/2009	GV	Added the static class for Events to be returned
	// 10/09/2008 - MC - Added the ability to specify a range of parameters to remove by using a regex
	#endregion

	/// <summary>
	/// The <b>ClearParameterAction</b> clears the values of the parameter(s) specified.
	/// The names of the parameters to clear are specified in the <c>parameters</c>
	/// attribute of the action's state.
	/// </summary>
	/// <remarks>
	/// Only values set by the <b>SetParameterAction</b> can be cleared using
	/// this action.  Values that are provided in the initial request can not be cleared.
	/// </remarks>
	///	<author>Scott Dyke</author>
	public class ClearParameterAction : IAction
	{
		#region IAction Members

		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				NameValueCollection paramList = ((ActionState)context.CurrentState).Parameters;
				MvcRequest req = context.Request;

				for (int i = 0; i < paramList.Count; i++) {
					try {
						//  remove the parameter from parameter name value collection
						string paramName = paramList.GetKey(i);

						if (req.Params.AllKeys.Contains(paramName)) {
							req.Params.Remove(paramName);
						} else {
							List<String> removalList = new List<String>();
							foreach (string param in req.Params) {
								if (Regex.Matches(param, paramName).Count > 0) {
									removalList.Add(param);
								}
							}
							foreach (string param in removalList) {
								req.Params.Remove(param);
							}
						}
					} catch (Exception e) {
						LogManager.GetCurrentClassLogger().Warn(
							errorMessage => errorMessage("Could not clear a parameter."), e);
					}
				}

				retEvent = Events.Ok;
			} catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("ClearParameterAction.Execute:"), ex);
			}

			return retEvent;
		}

		#endregion

		#region Nested type: Events

		public class Events
		{
			public static string Ok
			{
				get { return EventNames.OK; }
			}

			public static string Error
			{
				get { return EventNames.ERROR; }
			}
		}

		#endregion
	}
}