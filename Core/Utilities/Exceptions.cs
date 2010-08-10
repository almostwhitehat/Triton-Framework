using System;
using System.Runtime.Serialization;

namespace Triton
{
	public class TypeMismatchException : Exception
	{
		public TypeMismatchException() {}


		public TypeMismatchException(
			string message)
			: base(message) {}


		public TypeMismatchException(
			SerializationInfo info,
			StreamingContext context)
			: base(info, context) {}


		public TypeMismatchException(
			string message,
			Exception innerException)
			: base(message, innerException) {}
	}
}