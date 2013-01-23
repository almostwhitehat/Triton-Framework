using System;
using System.Linq;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Logic.Support;
using Triton.CodeContracts;

namespace Triton.Logic {

#region History

// History:
//   1/22/13 - SD -	Added support for testing multiple parameters and RequireAll property.

#endregion

/// <summary>
/// Action to determine if a parameter with a given name exists
/// in the request.  By default a parameter with no value is deemed
/// not to exist in the request.  Use 
/// </summary>
/// <author>Scott Dyke</author>
/// <created>1/23/2012</created>
public class ParameterExistsAction : IAction
{

	public ParameterExistsAction()
	{
		CountEmptyStringAsExists = false;
		RequireAll = false;
	}


	/// <summary>
	/// Gets or sets the name of the request parameter to check the existence of.
	/// </summary>
	public string ParamNameIn
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the flag indicating whether the check is for any given
	/// parameter has a value (false), or all given parametes have a value (true).
	/// </summary>
	public bool RequireAll
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the flag indicating whether or not a parameter in the request but
	/// with no value (empty string) should be deemed to exist.
	/// </summary>
	public bool CountEmptyStringAsExists
	{
		get;
		set;
	}


	#region IAction Members

	public string Execute(
		TransitionContext context)
	{
		string retEvent = Events.Error;
		MvcRequest request = context.Request;

		try {
			ActionContract.Requires<ApplicationException>(!string.IsNullOrEmpty(ParamNameIn),
					"No parameter name given in the ParamNameIn attribute.");

					//  split the input names to get the individual parameter names
			string[] paramNames = ParamNameIn.Split('|');

			if (RequireAll) {
				retEvent = paramNames.All(name => !IsEmpty(request[name])) ? Events.Yes : Events.No;
			} else {
				retEvent = paramNames.Any(name => !IsEmpty(request[name])) ? Events.Yes : Events.No;
			}
		} catch (Exception e) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error(error => error("Error occurred check parameter existence."), e);
		}

		return retEvent;
	}

	#endregion


	/// <summary>
	/// Determines it the given parameter value is considered empty.
	/// </summary>
	/// <param name="paramValue">The parameter value to check.</param>
	/// <returns><c>true</c> if the given value is empty, <c>false</c> if not.</returns>
	private bool IsEmpty(
		string paramValue)
	{
		return ((paramValue == null) || (!CountEmptyStringAsExists && (paramValue.Length == 0)));
	}


	#region Nested type: Events

	public class Events
	{
		public static string Yes
		{
			get {
				return EventNames.YES;
			}
		}

		public static string No
		{
			get {
				return EventNames.NO;
			}
		}

		public static string Error
		{
			get {
				return EventNames.ERROR;
			}
		}
	}

	#endregion
}
}
