using System;
using Common.Logging;
using Triton.Controller;
using Triton.Controller.Action;
using Triton.Controller.Request;
using Triton.Logic.Support;
using Triton.Support.Error;
using Triton.Validator.Logic.Support;
using Triton.Validator.Model;

namespace Triton.Validator.Logic
{

	#region History

	// History:
	// 09/09/2009	GV	Changed the interface to inherit from IAction
	// 12/03/2009	GV	Changed the name to be ValidateRequest, changed the returns to be pass, fail, error.
	// 07/27/1020	SD	Added support to be able to take a comma-delimited list of validator
	//					names and run each of the validators listed.

	#endregion

	/// <summary>
	/// <b>ValidateRequestAction</b> uses the Validator system to validate the parameters of
	/// a request.  A <c>validatorName</c> attribute in the state identifying the action
	/// indicates the name of the validator to use to perform the validation.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ValidateRequestAction : IAction
	{
		protected char	VALIDATOR_NAME_DELIMITER	= ',';


		public ValidateRequestAction()
		{
			ErrorsItemNameIn = "errors";
			ErrorsItemNameOut = "errors";
		}


		/// <summary>
		/// Gets or sets the name of the <c>Validator</c> to use to perform
		/// the validation.  Can be a comma-delimited list of names of <c>Validator</c>s
		/// to run.
		/// </summary>
		public string ValidatorName
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


		#region IAction Members

		/// <summary>
		/// Executes the action to perform the validation.
		/// </summary>
		/// <param name="context">The <c>TransitionContext</c> in which the action is executing.</param>
		/// <returns>The event resulting from the action's execution.</returns>
		public string Execute(
			TransitionContext context)
		{
			string retEvent = Events.Error;
			MvcRequest request = context.Request;

			try {
				string[] validatorNames = ValidatorName.Split(VALIDATOR_NAME_DELIMITER);

				bool passed = true;
				foreach (string validatorName in validatorNames) {
							//  get the Validator from the ValidatorManager
					Model.Rules.Validator validator = ValidatorManager.GetInstance().GetValidator(validatorName);

					if (validator == null) {
						throw new MissingMemberException(string.Format(
								"No validator found for '{0}'.", validatorName));
					}

							//  evaluate the validator
					ValidationResult result = validator.Evaluate(request);

							//  if the validation failed, build an ErrorList and add
							//  it to the request for the page
					if (!result.Passed && (result.Errors != null)) {
						ErrorList errors;

								//  if the request contains the error list, append to it, else create a new one
						if (request.Items[ErrorsItemNameIn] != null &&
								request.Items[ErrorsItemNameIn] is ErrorList &&
								request.GetItem<ErrorList>(ErrorsItemNameIn) != null) {

							errors = request.GetItem<ErrorList>(ErrorsItemNameIn);
						} else {
							errors = new ErrorList(DictionaryManager.GetDictionaryManager().GetDictionary(context.Site));
						}

						foreach (ValidationError err in result.Errors) {
							errors.Add(err.ErrorId);
						}

								//  add the errors to the request to propagate back to the page
						request.Items[ErrorsItemNameOut] = errors;
					}

					passed = (passed && result.Passed);
				}

				retEvent = passed ? Events.Pass : Events.Fail;
			} catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Error(error => error("Error in Execute."), ex);
			}

			return retEvent;
		}

		#endregion


		#region Nested type: Events

		public class Events
		{
			/// <summary>
			/// Event returned if a validation passed.
			/// </summary>
			public static string Pass
			{
				get { return ValidatorEventNames.PASS; }
			}

			/// <summary>
			/// Event returned if a validation failed.
			/// </summary>
			public static string Fail
			{
				get { return ValidatorEventNames.FAIL; }
			}

			/// <summary>
			/// Event returned if errors occured during validation.
			/// </summary>
			public static string Error
			{
				get { return ValidatorEventNames.ERROR; }
			}
		}

		#endregion
	}
}