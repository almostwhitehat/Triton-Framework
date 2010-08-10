using System.Text.RegularExpressions;
using Triton.Controller.Request;

namespace Triton.Validator.Model.Rules
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>RegexRule</b> is a leaf <b>IValidationRule</b> that applies a
	/// regular expression to the value of a form field.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class RegexRule : BaseLeafRule
	{
		private const string NULL_PATTERN = "[[null]]";


		/// <summary>
		/// Gets or sets the regular expression pattern to apply to the field value.
		/// </summary>
		public string Pattern { get; set; }


		/// <summary>
		/// Evaluates the rule for the given request.  The <b>RegexRule</b> applies its
		/// regular expression pattern to its specified field in the given request.
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

			string fieldVal = request[Field];
// TODO: what if fieldVal is null??  is it valid or not??

			//  if the field value is null and the check is not explicitly for null,
			//  add the error
			if (fieldVal == null) {
				if (this.Pattern == NULL_PATTERN) {
					result.Passed = true;
				} else {
					result.AddError(Field, ErrorId);
				}
			} else {
				bool passed = Regex.IsMatch(fieldVal, this.Pattern);

				if (passed) {
					result.Passed = true;
				} else {
					result.AddError(Field, ErrorId);
				}
			}

			return result;
		}
	}
}