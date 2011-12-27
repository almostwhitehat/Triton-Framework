using System;
using System.Text.RegularExpressions;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Logic.Support;
using Triton.Support.Error;
using Triton.Support.Request;
using Triton.Utilities;

namespace Triton.Logic
{

	#region History

	// History:
	//  09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
	//  11/12/2009	GV	Added subclass of events, for use for returning events.
	//   3/16/2011	SD	Added support for replacing of placeholders in the message
	//					with the value of like-named request parameters.
	//   6/22/2011	SD	Added support for getting the error ID from a request parameter.
	//   6/24/2011	SD	Added support for being able to specify a specific index in the
	//					errors list to insert the given error at.

	#endregion

	/// <summary>
	/// Set an error message to the errors list. If error id is not provided 
	/// the action takes its id as the error id.
	/// </summary>
	/// <remarks>
	/// Action Parameters: <br/>
	/// <b>ErrorId</b> - the error id to be inserted itno the error collection. <br/>
	/// <b>ErrorsItemNameOut</b> - the name in Request.Items to put the ErrorList into. <br/>
	/// Returned events:<br/>
	/// <b>ok</b> - The error was successfully added/set <br/>
	/// <b>error</b> - an error occurred while setting the error message<br/>
	/// </remarks>
	public class SetErrorAction : IAction
	{

		public SetErrorAction()
		{
			ErrorsItemNameIn = CoreItemNames.DEFAULT_ERRORS_LIST;
			ErrorsItemNameOut = CoreItemNames.DEFAULT_ERRORS_LIST;
		}


		/// <summary>
		/// Gets or sets ErrorId for the Error to be added.
		/// </summary>
		public string ErrorId
		{
			get;
			set;
		}


		/// <summary>
		/// Gets or sets the name of the request parameter containing
		/// the error ID for the Error to be added.
		/// </summary>
		public string ErrorIdParamNameIn
		{
			get;
			set;
		}


		/// <summary>
		/// Errors collection item name to retrieve.
		/// </summary>
		public string ErrorsItemNameIn
		{
			get;
			set;
		}


		/// <summary>
		/// Errors collection item name to append the errors.
		/// </summary>
		public string ErrorsItemNameOut
		{
			get;
			set;
		}


		/// <summary>
		/// Gets or sets the zero-based index in the errors list
		/// into which to place the specified error.
		/// [Optional - if not provided error is added at end of list.]
		/// </summary>
		public int? Index
		{
			get;
			set;
		}


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
			MvcRequest request = context.Request;

			try {
						//  if ErrorId not specified on the state, check for request parameter or use state ID
				if (string.IsNullOrEmpty(ErrorId)) {
					if (!string.IsNullOrEmpty(ErrorIdParamNameIn) && !string.IsNullOrEmpty(request[ErrorIdParamNameIn])) {
						ErrorId = request[ErrorIdParamNameIn];
					} else {
							//  if error id was not set use the current state id.
						ErrorId = context.CurrentState.Id.ToString();
					}
				}

				ErrorList errors;

						//  if the request contains the error list, append to it, else create a new one
				if (request.Items[ErrorsItemNameIn] != null
						&& request.Items[ErrorsItemNameIn] is ErrorList
						&& request.GetItem<ErrorList>(ErrorsItemNameIn) != null) {

					errors = request.GetItem<ErrorList>(ErrorsItemNameIn);
				} else {
					errors = new ErrorList(DictionaryManager.GetDictionaryManager().GetDictionary(context.Site));
				}

				Error err = errors.getError(Convert.ToInt64(ErrorId));

						//  check for parameter replacement placeholders in the message
						//  placeholders are of the form: {[param_name]}
				MatchCollection parameterMatches = Regex.Matches(err.Message, @"\{\[([\w-]+)\]\}");

						//  if there are parameter placeholders, replace them with the
						//  parameter values
				if (parameterMatches.Count > 0) {
					foreach (Match match in parameterMatches) {
						err.Message = err.Message.Replace(match.Value, request[match.Groups[1].Value.EvaluatePropertyValue()]);
					}
				}

				if (Index.HasValue && (Index.Value < errors.Count)) {
					errors.Insert(Index.Value, err);
				} else {
					errors.Add(err);
				}

				request.Items[ErrorsItemNameOut] = errors;

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