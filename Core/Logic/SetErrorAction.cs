using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Logic.Support;
using Triton.Support.Error;
using Triton.Support.Request;

namespace Triton.Logic
{

	#region History

	// History:
	// 09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
	// 11/12/2009	GV	Added subclass of events, for use for returning events.

	#endregion

	/// <summary>
	/// Set an error message to the errors list. If error id is not provided 
	/// the action takes its id as the error id.
	/// </summary>
	/// <remarks>
	/// Action Parameters: <br/>
	/// <b>ErrorId</b> - the error id to be inserted itno the error collection. <br/>
	/// Returned events:<br/>
	/// <b>ok</b> - The error was successfully added/set <br/>
	/// <b>error</b> - an error occurred while setting the error message<br/>
	/// </remarks>
	/// <summary> Add an error object into the Errors collection on the Request.Items collection
	/// </summary>
	public class SetErrorAction : IAction
	{
		public SetErrorAction()
		{
			this.ErrorsItemNameIn = CoreItemNames.DEFAULT_ERRORS_LIST;
			this.ErrorsItemNameOut = CoreItemNames.DEFAULT_ERRORS_LIST;
		}


		/// <summary>
		/// Gets or sets ErrorId for the Error to be added.
		/// </summary>
		public string ErrorId { get; set; }

		/// <summary>
		/// Errors collection item name to retrieve.
		/// </summary>
		public string ErrorsItemNameIn { get; set; }

		/// <summary>
		/// Errors collection item name to append the errors.
		/// </summary>
		public string ErrorsItemNameOut { get; set; }

		#region IAction Members

		/// <summary>Adds and Error to the Errors Collection.
		/// </summary>
		/// <param name="context">
		/// The context.
		/// </param>
		/// <returns> event name for the next transisiton
		/// </returns>
		/// <exception cref="ApplicationException">
		/// </exception>
		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;

			try {
				//if error id was not set use the current state id.
				if (string.IsNullOrEmpty(this.ErrorId)) {
					this.ErrorId = context.CurrentState.Id.ToString();
				}

				ErrorList errors;

				//if the request contains the error list, append to it, else create a new one
				if (context.Request.Items[this.ErrorsItemNameIn] != null &&
				    context.Request.Items[this.ErrorsItemNameIn] is ErrorList &&
				    context.Request.GetItem<ErrorList>(this.ErrorsItemNameIn) != null) {

					errors = context.Request.GetItem<ErrorList>(this.ErrorsItemNameIn);
				} else {
					errors = new ErrorList(DictionaryManager.GetDictionaryManager().GetDictionary(context.Site));
				}

				errors.Add(Convert.ToInt64(this.ErrorId));

				context.Request.Items[this.ErrorsItemNameOut] = errors;

				retEvent = Events.Ok;
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("Error occured in Execution of the Action."), e);
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