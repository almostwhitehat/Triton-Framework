using System;
using Triton.Controller.Request;

namespace Triton.Controller.Format
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <c>Formatter</c> in an interface that defines a formatter for transforming
	/// a list of TransferObjects into a different format, for example, XML.
	/// </summary>
	///	<author>Scott Dyke</author>
	public interface Formatter
	{
		/// <summary>
		/// Gets the <b>MvcRequest</b> the Formatter is called for.
		/// </summary>
		MvcRequest Request { get; set; }


		/// <summary>
		/// Transforms the data in the given object to the desired format.
		/// </summary>
		/// <param name="obj">The object containing the data to format.</param>
		/// <returns>An object containing the formatted data of the <c>SearchResult</c>.</returns>
		object Format(
			object obj);


		/// <summary>
		/// Returns a list of Types supported by the Formatter.
		/// </summary>
		/// <returns>An array of <c>Type</c>s of TransferObjects that the 
		///			Formatter supports.</returns>
		Type[] GetSupportedTypes();
	}
}