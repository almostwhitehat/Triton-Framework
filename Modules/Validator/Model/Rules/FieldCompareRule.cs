using System;
using Triton.Controller.Request;

namespace Triton.Validator.Model.Rules
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The <b>FieldCompareRule</b> is a field validation rule for a Validator that
	/// compares the values of two or more fields.  The rule passes if all of the
	/// specified fields contain equal values.
	/// </summary>
	/// <remarks>
	/// The equality check for the field values can be either case-sensitive or
	/// non case-sensitive.  The case sesitivity is set via the <b>caseSensitive</b>
	/// attribute in the rule declaration.  The default is "true" (case-sensitive).
	/// <para/>
	/// The rule declaration is as follows:
	/// <pre>
	/// 	<fieldCompare fields="password,confirmpassword" caseSensitive="false" errorId="123">
	///		</fieldCompare>
	/// </pre>
	/// <para/>
	/// Where:<br/>
	/// <b>fields</b> is a comma-delimited list of the names of the fields to compare. (required)<br/>
	/// <b>caseSensitive</b> indicates whether the compare is case-sensitive or not. (optional - default is "true")<br/>
	/// <b>errorId</b> is the ID of the error from the ErrorDictionary to return if the validation fails.
	/// </remarks>
	public class FieldCompareRule : BaseLeafRule
	{
		private const char FIELD_DELIMITER = ',';

		/// <summary>
		/// The list of fields that must match.
		/// </summary>
		private string[] fields;


		///<summary>
		/// Initializes the CaseSensitive property to true, by default.
		///</summary>
		public FieldCompareRule()
		{
			this.CaseSensitive = true;
		}


		/// <summary>
		/// Sets the list of fields whose values must match.
		/// </summary>
		public string Fields
		{
			set
			{
				if (value != null) {
					this.fields = value.Split(FIELD_DELIMITER);
				}
			}
		}


		/// <summary>
		/// Gets or sets whether the compare is case-sensitive or not.
		/// </summary>
		public bool CaseSensitive { get; set; }


		/// <summary>
		/// Evaluates the rule for the given request.  The <b>FieldCompareRule</b> determines
		/// if the specified fields in the given request all have the same value.
		/// </summary>
		/// <param name="request">The <b>FPRequest</b> to evaluate the rule for.</param>
		/// <returns>A <b>ValidationResult</b> indicating the result of evaluating the
		///			rule and any validation errors that occurred.</returns>
		public override ValidationResult Evaluate(
			MvcRequest request)
		{
			ValidationResult result = new ValidationResult {
			                                               	Passed = true
			                                               };

			//  default to true

			//  compare each subsequent field value against the first --
			//  if a mis-match is found, set the result to false, and add the error message
			string val1 = request[this.fields[0]];
			for (int k = 1; k < this.fields.Length; k++) {
				if (((val1 == null) && (request[this.fields[k]] != null))
				    ||
				    !val1.Equals(request[this.fields[k]],
				                 this.CaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) {
					result.Passed = false;
					result.AddError(this.fields[k], ErrorId);
					break;
				}
			}

			return result;
		}
	}
}