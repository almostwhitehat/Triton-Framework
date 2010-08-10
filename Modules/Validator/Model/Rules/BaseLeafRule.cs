namespace Triton.Validator.Model.Rules
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>BaseLeafRule</b> is the base implementation for a <b>IValidationRule</b> that
	/// has no child rules.
	/// </summary>
	/// <remarks>
	/// <b>BaseLeafRule</b> provides "empty" implementations for the child related
	/// properties and methods.
	///	</remarks>
	///	<author>Scott Dyke</author>
	public abstract class BaseLeafRule : BaseRule
	{
		/// <summary>
		/// Returns null for the list of children since leaf rules can't have children.
		/// </summary>
		public override IValidationRule[] Children
		{
			get { return null; }
		}


		/// <summary>
		/// Dummy implementation to satisfy the <b>IValidationRule</b> interface for
		/// a leaf rule that can't have children.
		/// </summary>
		/// <param name="rule">Ignored.</param>
		public override void Add(
			IValidationRule rule) {}


		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public override IValidationRule Remove(
			string name)
		{
			return null;
		}
	}
}