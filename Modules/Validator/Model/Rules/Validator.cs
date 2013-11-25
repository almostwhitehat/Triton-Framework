using Triton.Controller.Request;

namespace Triton.Validator.Model.Rules
{
	#region History

	// History:

	#endregion

	/// <summary>
	/// A <b>Validator</b> is a collection of <b>IValidationRule</b>s used to
	/// validate the values of a <b>MvcRequest</b>.
	/// </summary>
	/// <remarks>
	/// A <b>Validator</b> is obtained using the ValidatorManager.  Validators
	/// defined in the validators.config file.
	/// </remarks>
	///	<author>Scott Dyke</author>
	public class Validator : AndRule
	{
		/// <summary>
		/// Constructs a new Validator with the given name.
		/// </summary>
		/// <param name="name">The name of the Validator to construct.</param>
		internal Validator(
			string name)
		{
			Name = name;
		}


		/// <summary>
		/// Gets the name of the Validator.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}


		/// <summary>
		/// Evaluates the rule for the given request.  <b>Validator</b> "ands" all of its
		/// child rules to determine if final result.
		/// </summary>
		/// <remarks>
		/// This override locks the validator before calling the base.Evaluate to ensure
		/// thread safety.
		/// </remarks>
		/// <param name="request">The <b>MvcRequest</b> to evaluate the rule for.</param>
		/// <returns>A <b>ValidationResult</b> indicating the result of evaluating the
		///			rule and any validation errors that occurred.</returns>
		public override ValidationResult Evaluate(
			MvcRequest request)
		{
			lock (this) {
				return base.Evaluate(request);
			}
		}
	}
}