using System;

namespace Triton.Support.Mail
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// The exception that is thrown when there is an attempt to send out email 
	/// but a required parameter is missing in the mailer object.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class MissingParamException : Exception
	{
		public MissingParamException() {}


		public MissingParamException(
			String message)
			: base(message) {}


		public MissingParamException(
			String message,
			Exception innerException)
			: base(message, innerException) {}
	}
}