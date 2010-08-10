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
	///	<author>Scott Dyke</author>
	public class NoDictionaryException : Exception
	{
		public NoDictionaryException() {}


		public NoDictionaryException(
			String message)
			: base(message) {}


		public NoDictionaryException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) {}


		public NoDictionaryException(
			string message,
			Exception innerException)
			: base(message, innerException) {}
	}
}