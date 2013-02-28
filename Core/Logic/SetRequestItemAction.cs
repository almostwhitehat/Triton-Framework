using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Logic.Support;

namespace Triton.Logic
{

	#region History

	// History:
	//    2/8/13 - SD -	Added support for setting Item value from parameter.

	#endregion

	/// <summary>
	/// <b>SetRequestItemAction</b> is an Action to set a request item to a value.
	/// If the value is empty string, then set the item to null
	/// </summary>
	/// <remarks>
	/// SetRequestItemAction accepts the following attributes:<br/>
	/// <b>requestItemName</b> - the name used in Request.Items for the message.  The
	///						default is "response".<br/>
	/// <b>value</b> - the value of the item. <br/>
	/// </remarks>
	///	<author>Garun Vagidov</author>
	public class SetRequestItemAction : IAction
	{
		/// <summary>
		/// Gets or sets the name used for the message in Request.Items.
		/// </summary>
		public string RequestItemName
		{
			get;
			set;
		}


		/// <summary>
		/// Gets or sets the message content.
		/// </summary>
		public string Value
		{
			get;
			set;
		}


		/// <summary>
		/// Gets or sets the name of the parameter whose value is
		/// used for the value of the request item.
		/// </summary>
		public string ValueParamNameIn
		{
			get;
			set;
		}


		#region IAction Members

		/// <summary>
		/// Carries out the actions of the Action.
		/// </summary>
		/// <param name="context">The <b>TransitionContext</b> the Action is executing in.</param>
		/// <returns>A string containing the event returned by the action.</returns>
		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;
			MvcRequest request = context.Request;

			try {
			
				if (string.IsNullOrEmpty(RequestItemName)) {
					throw new ApplicationException("Could not get the RequestItemName to set to the value.");
				}
				
				if (!string.IsNullOrEmpty(Value)) {
					request.Items[RequestItemName] = Value;
				} else if (!string.IsNullOrEmpty(ValueParamNameIn)) {
					request.Items[RequestItemName] = request[ValueParamNameIn];
				} else {
					request.Items[RequestItemName] = null;
				}

				retEvent = Events.Ok;

			} catch(Exception ex) {
				LogManager.GetCurrentClassLogger().Error(error => error("Could not set the request item to a value.", ex));
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