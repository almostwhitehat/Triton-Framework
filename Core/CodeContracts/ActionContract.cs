using System;

namespace Triton.CodeContracts {

	#region History

	// History:
	//   2/19/10 - SD -	Added params object[] args parameter to overloaded Requires method to allow
	//					implicit string.Format call if and only if needed to avoid the overhead of
	//					string.Format having to be executed to pass in the message whether used or not.

	#endregion


	/// <summary>
	/// Contains static methods for a Requires action precondition that throws an exception if condition fails.
	/// </summary>
	public static class ActionContract
	{
		/// <summary>
		/// Specifies an action precondition contract for the enclosing execute method, and throws an
		/// exception if the condition for the contract fails.
		/// </summary>
		/// <typeparam name="TException">The exception to throw if the condition is false.</typeparam>
		/// <param name="condition">The conditional expression to test.</param>
		public static void Requires<TException>(
			bool condition) where TException : Exception
		{
			Requires<TException>(condition, null);
		}


		/// <summary>
		/// Specifies an action precondition contract for the enclosing execute method, and throws an
		/// exception if the condition for the contract fails.
		/// </summary>
		/// <typeparam name="TException">The exception to throw if the condition is false.</typeparam>
		/// <param name="condition">The conditional expression to test.</param>
		/// <param name="userMessage">The message to display if the condition is false.</param>
		/// <param name="args">An object array of values to include in userMessage.</param>
		public static void Requires<TException>(
			bool condition,
			string userMessage,
			params object[] args) where TException : Exception
		{
			if (!condition) {
				TException exception = (TException)Activator.CreateInstance(typeof(TException), 
						((userMessage != null) && (args != null)) ? string.Format(userMessage, args) : userMessage);
				
				throw exception;
			}
		}
	}
}