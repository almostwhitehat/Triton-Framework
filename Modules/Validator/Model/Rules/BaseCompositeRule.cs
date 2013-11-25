using System.Collections.Generic;

namespace Triton.Validator.Model.Rules
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>BaseCompositeRule</b> provides a base implementation for composite <b>IValidationRule</b>s.
	/// A composite rule is any rule with child rules whose result is some combination
	/// of its children.
	/// </summary>
	/// <author>Scott Dyke</author>
	public abstract class BaseCompositeRule : BaseRule
	{
		/// <summary>
		/// The collection of child rules.
		/// </summary>
		protected List<IValidationRule> children;


		/// <summary>
		/// Gets the child rules of the rule, or <c>null</c> if there are no child rules.
		/// </summary>
		public override IValidationRule[] Children
		{
			get {
				return this.children != null ? this.children.ToArray() : null;
			}
		}


		/// <summary>
		/// Adds a child rule to the composite rule.
		/// </summary>
		/// <param name="rule">The child rule to add.</param>
		public override void Add(
			IValidationRule rule)
		{
			if (children == null) {
				children = new List<IValidationRule>();
			}

			children.Add(rule);
		}


		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public override IValidationRule Remove(
			string name)
		{
			// TODO:  Add BaseCompositeRule.Remove implementation
			return null;
		}
	}
}