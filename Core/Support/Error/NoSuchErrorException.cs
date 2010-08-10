using System;
using System.Runtime.Serialization;

namespace Triton.Support.Error
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// 
	/// </summary>
	/// <author>Scott Dyke</author>
	public class NoSuchErrorException : Exception
	{
		public NoSuchErrorException() {}


		public NoSuchErrorException(
			String message)
			: base(message) {}


		public NoSuchErrorException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) {}


		public NoSuchErrorException(
			string message,
			Exception innerException)
			: base(message, innerException) {}
	}
}