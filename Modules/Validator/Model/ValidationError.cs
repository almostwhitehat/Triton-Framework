namespace Triton.Validator.Model
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// A <b>ValidationError</b> contains the information about a validation rule
	/// failure.  It has the name of the field for whcil the validation failed and
	/// an error ID to indicate what was wrong.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ValidationError
	{
		/// <summary>
		/// Constructs a new <b>ValidationError</b> with the given 
		/// field name and error ID.
		/// </summary>
		/// <param name="field">The name of the field the error applies to.</param>
		/// <param name="errorId">The ID of the error indicating what the problem was.</param>
		public ValidationError(
			string field,
			long errorId)
		{
			this.Field = field;
			this.ErrorId = errorId;
		}


		/// <summary>
		/// Gets the name of the field the error applies to.
		/// </summary>
		public string Field { get; private set; }


		/// <summary>
		/// Gets the ID of the error that occurred.
		/// </summary>
		public long ErrorId { get; private set; }
	}
}