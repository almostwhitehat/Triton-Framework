using Triton.Controller.Request;

namespace Triton.Validator.Model.Rules {

#region History

// History:
//  11/22/13 - SD -	Added StopOnFail property to indicate rule processing should
//					cease if a rule with that property set fails.

#endregion

/// <summary>
/// <b>BaseRule</b> is the base implemenation for the <b>IValidationRule</b>.
/// It provides support for identifying a form field the rule applies to and
/// an error ID to identify an error message if the rule does not pass.
/// </summary>
///	<author>Scott Dyke</author>
public abstract class BaseRule : IValidationRule
{

	public BaseRule()
	{
		StopOnFail = false;
	}


	/// <summary>
	/// Gets or sets the name of the form field the rule applies to.
	/// </summary>
	public string Field
	{
		get;
		set;
	}


	/// <summary>
	/// Gets or sets the error ID to return if the rule does not pass.
	/// </summary>
	public long ErrorId
	{
		get;
		set;
	}


	#region IValidationRule Members

	/// <summary>
	/// Gets the child rules of the rule.
	/// </summary>
	public abstract IValidationRule[] Children
	{
		get;
	}


	/// <summary>
	/// Gets or sets a flag indicating if processing should stop if the rule fails.
	/// </summary>
	public bool StopOnFail
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
	public abstract ValidationResult Evaluate(
		MvcRequest request);


	/// <summary>
	/// Adds a child rule to the rule.
	/// </summary>
	/// <param name="rule">The child rule to add.</param>
	public abstract void Add(
		IValidationRule rule);


	/// <summary>
	/// Not implemented.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public abstract IValidationRule Remove(
		string name);

	#endregion
}
}