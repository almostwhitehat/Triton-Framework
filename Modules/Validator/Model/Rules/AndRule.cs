using Triton.Controller.Request;

namespace Triton.Validator.Model.Rules
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>AndRule</b> is a composite <b>IValidationRule</b> that accumulates the
	/// results of all of its children and whose final pass/fail status is <c>true</c>
	/// only if <u>all</u> of the child rules evaulate to <c>true</c>.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class AndRule : BaseCompositeRule
	{
		/// <summary>
		/// Evaluates the rule for the given request.  The <b>AndRule</b> accumulates the
		/// results of all of its child rules.  It evaluates all of the child rules
		/// regardless of whether or not any particular rule passes or fails.
		/// </summary>
		/// <param name="request">The <b>MvcRequest</b> to evaluate the rule for.</param>
		/// <returns>A <b>ValidationResult</b> indicating the result of evaluating the
		///			rule and any validation errors that occurred.</returns>
		public override ValidationResult Evaluate(
			MvcRequest request)
		{
			ValidationResult result = new ValidationResult();

			foreach (IValidationRule rule in children) {
				//  evaluate the child rule
				ValidationResult childResult = rule.Evaluate(request);
				//  if the child rule failed, set this rule to failure and add the
				//  error(s) from the child
				if (!childResult.Passed) {
					result.Passed = false;
					result.AddErrors(childResult.Errors);
				}
			}

			return result;
		}
	}
}