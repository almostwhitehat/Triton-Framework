using System;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Model.Dao;
using Triton.Logic.Support;
using Triton.Support.Request;
using Triton.Support.Error;
using Triton.CodeContracts;

namespace Triton.Logic {

#region History

// History:

#endregion

/// <summary>
/// Action to compare the values of two request parameters or items.
/// </summary>
/// <remarks>
/// <para>Returned events:</para>
/// <c>yes</c> - the two objects given to compare are equal.<br/>
/// <c>no</c> - the two objects given to compare are not equal.<br/>
/// <c>error</c> - an error occurred while attempting to compare the objects.<br/>
/// </remarks>
///	<author>Scott Dyke</author>
///	<created>6/30/10</created>
public class EqualAction : IAction
{


	/// <summary>
	/// Gets or sets the name of the first request parameter to compare.
	/// </summary>
	public string ParamName1In
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the second request parameter to compare.
	/// </summary>
	public string ParamName2In
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the first request item to compare.
	/// </summary>
	public string ItemName1In
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the name of the second request item to compare.
	/// </summary>
	public string ItemName2In
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the value to compare to.
	/// </summary>
	public string CompareValueIn
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
			ActionContract.Requires<ApplicationException>(
					(!string.IsNullOrEmpty(ParamName1In) && (!string.IsNullOrEmpty(ParamName2In) || !string.IsNullOrEmpty(CompareValueIn)))
					|| (!string.IsNullOrEmpty(ItemName1In) && (!string.IsNullOrEmpty(ItemName2In) || !string.IsNullOrEmpty(CompareValueIn))),
					"Two parameter names (ParamName1In, ParamName2In) or two item names (ItemName1In, ItemName2In) or a parameter or attribute name along with a value must be provided.");

					//  compare parameter values
			if (!string.IsNullOrEmpty(ParamName1In)) {
				if (!string.IsNullOrEmpty(ParamName2In)) {
					retEvent = (request[ParamName1In] == request[ParamName2In]) ? Events.Yes : Events.No;
				} else if (!string.IsNullOrEmpty(CompareValueIn)) {
					retEvent = (request[ParamName1In] == CompareValueIn) ? Events.Yes : Events.No;
				}
			}

					//  compare item values
			if (!string.IsNullOrEmpty(ItemName1In)) {
				if (!string.IsNullOrEmpty(ItemName2In)) {
					retEvent = (request.Items[ItemName1In] == request.Items[ItemName2In]) ? Events.Yes : Events.No;
				} else if (!string.IsNullOrEmpty(CompareValueIn)) {
					retEvent = (request.Items[ItemName1In].ToString() == CompareValueIn) ? Events.Yes : Events.No;
				}
			}

		} catch (Exception e) {
			ILog logger = LogManager.GetCurrentClassLogger();
			logger.Error(error => error("Error occurred in EqualAction."), e);
		}

		return retEvent;
	}

	#endregion


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
