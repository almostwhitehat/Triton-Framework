namespace Triton.Validator.Model {

#region History

// History:
//   6/30/2011	SD	Made the AddError/AddErrors and ClearErrors methods public so
//					that validator rules in applications can access.
//    11/22/13	SD	Added StopProcessing property to indicate rule processing should
//					cease if that property is set to true.

#endregion

/// <summary>
/// A <b>ValidationResult</b> contains the results of evaluating a <b>IValidationRule</b>.
/// It indicates whether or not the rule passed, and if not, what errors occurred.
/// </summary>
///	<author>Scott Dyke</author>
public class ValidationResult
{
	/// <summary>
	/// Initializes Passed to true by default.
	/// </summary>
	public ValidationResult()
	{
		Passed = true;
		StopProcessing = false;
	}


	/// <summary>
	/// Gets or sets the flag indicating whether or not the validation passed.
	/// </summary>
	public bool Passed
	{
		get;
		set;
	}


	/// <summary>
	/// Gets the collection of errors that occurred during vaidation if the
	/// validation did not pass.
	/// </summary>
	public ValidationErrorCollection Errors
	{
		get;
		private set;
	}


	/// <summary>
	/// Gets or sets a flag indicating if processing should stop.
	/// </summary>
	public bool StopProcessing
	{
		get;
		set;
	}


	/// <summary>
	/// Adds a new <b>ValidationError</b> to the collection of errors for the
	/// given field and error ID.
	/// </summary>
	/// <param name="field">The name of the field to which the error applies.</param>
	/// <param name="errorId">The ID of the error that occurred.</param>
	public void AddError(
		string field,
		long errorId)
	{
		if (errorId != 0) {
			AddError(new ValidationError(field, errorId));
		}
	}


	/// <summary>
	/// Adds a new <b>ValidationError</b> to the collection of errors.
	/// </summary>
	/// <param name="error">The <b>ValidationError</b> to add.</param>
	public void AddError(
		ValidationError error)
	{
		if (error != null) {
			if (Errors == null) {
				Errors = new ValidationErrorCollection();
			}

			Errors.Add(error);
		}
	}


	/// <summary>
	/// Adds all of the <b>ValidationError</b>s from the given collection to
	/// the current error collection.
	/// </summary>
	/// <param name="errors">The <b>ValidationErrorCollection</b> to add the errors from.</param>
	public void AddErrors(
		ValidationErrorCollection errors)
	{
		if (errors != null) {
			if (Errors == null) {
				Errors = new ValidationErrorCollection();
			}

			foreach (ValidationError err in errors) {
				Errors.Add(err);
			}
		}
	}


	/// <summary>
	/// Removes all of the errors associated with this <b>ValidationResult</b>.
	/// </summary>
	public void ClearErrors()
	{
		if (Errors != null) {
			Errors.Clear();
		}
	}
}
}