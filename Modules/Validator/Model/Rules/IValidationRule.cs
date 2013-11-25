using Triton.Controller.Request;

namespace Triton.Validator.Model.Rules {

#region History

// History:
//  11/22/13 - SD -	Added StopOnFail property to indicate rule processing should
//					cease if a rule with that property set fails.

#endregion

/// <summary>
/// The <b>IValidationRule</b> interface is implemented to define validation rules for
/// web form fields.
/// </summary>
/// <remarks>
/// This is the base interface for implementation of the Composite pattern.
///	</remarks>
///	<author>Scott Dyke</author>
public interface IValidationRule
{
	/// <summary>
	/// Gets the child rules of the rule.
	/// </summary>
	IValidationRule[] Children
	{
		get;
	}


	/// <summary>
	/// Gets or sets a flag indicating if processing should stop if the rule fails.
	/// </summary>
	bool StopOnFail
	{
		get;
		set;
	}


	/// <summary>
	/// Evaluates the rule to determine if the given request passes the rule or not.
	/// </summary>
	/// <param name="request">The request to apply the rule to.</param>
	/// <returns>A <b></b> containing the result of the rule evaluation and any validation 
	///			errors that where encounter during evaluation.</returns>
	ValidationResult Evaluate(
		MvcRequest request);


	/// <summary>
	/// Adds a child rule to the rule.
	/// </summary>
	/// <param name="rule">The child rule to add.</param>
	void Add(
		IValidationRule rule);


	/// <summary>
	/// Not implemented.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	IValidationRule Remove(
		string name);
}
}