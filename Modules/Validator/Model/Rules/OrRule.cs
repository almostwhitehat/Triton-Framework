using Triton.Controller.Request;

namespace Triton.Validator.Model.Rules
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>OrRule</b> is a composite <b>IValidationRule</b> that evaluates its children
	/// sequentially and returning a successful evaluation upon it first successful child
	/// evaulation.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class OrRule : BaseCompositeRule
	{
		/// <summary>
		/// Evaluates the rule for the given request.  The <b>OrRule</b> short circuits
		/// on the first successful child rule, with a passed status.
		/// </summary>
		/// <param name="request">The <b>MvcRequest</b> to evaluate the rule for.</param>
		/// <returns>A <b>ValidationResult</b> indicating the result of evaluating the
		///			rule and any validation errors that occurred.</returns>
		public override ValidationResult Evaluate(
			MvcRequest request)
		{
			ValidationResult result = new ValidationResult {
			                                               	Passed = false
			                                               };

// TODO: if cnt is 0 should we return true or false??
			int cnt = children.Count;
			for (int k = 0; k < cnt && !result.Passed; k++) {
				IValidationRule rule = children[k];
				//  evaluate the child rule
				ValidationResult childResult = rule.Evaluate(request);
				//  clear out any previous validation errors
				result.ClearErrors();
				//  if the child rule passed, set this rule to passed,
				//  if not, add the errors from the child to the OrRule's result
				if (childResult.Passed) {
					result.Passed = true;
				} else {
					result.AddErrors(childResult.Errors);
				}
			}

			//  if the OrRule did not pass and the is a specific error identified
			//  for the OrRule, replace any errors from the children with the one
			//  for the OrRule
			if (!result.Passed && (ErrorId != 0)) {
				result.ClearErrors();
				result.AddError(Field, ErrorId);
			}

			return result;
		}
	}
}