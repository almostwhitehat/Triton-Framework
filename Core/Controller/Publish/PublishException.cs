using System;
using Triton.Controller.Request;

namespace Triton.Controller.Publish
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>PublishException</b> is the Exception that gets thrown by the publishing system
	/// if an error occurs while publishing or referencing a published page.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class PublishException : ApplicationException
	{
		protected TransitionContext context;
		protected PublishRecord publishRecord;
		protected MvcRequest request;


		/// <summary>
		/// Constructs a <b>PublishException</b> for the given TransitionContext 
		/// and PublishRecord, with the specified message.
		/// </summary>
		/// <param name="context">The <b>TransitionContext</b> in which the exception occurred.</param>
		/// <param name="publishRecord">The <b>PublishRecord</b> in use at the time 
		///			the exception occurred.</param>
		/// <param name="message">The error message to return.</param>
		public PublishException(
			TransitionContext context,
			PublishRecord publishRecord,
			string message) : base(message)
		{
			this.context = context;
			this.publishRecord = publishRecord;
		}

		/// <summary>
		/// Constructs a <b>PublishException</b> for the given TransitionContext 
		/// and PublishRecord, with the specified message.
		/// </summary>
		/// <param name="context">The <b>TransitionContext</b> in which the exception occurred.</param>
		/// <param name="publishRecord">The <b>PublishRecord</b> in use at the time 
		///			the exception occurred.</param>
		/// <param name="message">The error message to return.</param>
		/// <param name="innerException">The exception that occured.</param>
		public PublishException(
			TransitionContext context,
			PublishRecord publishRecord,
			string message,
			Exception innerException)
			: base(message, innerException)
		{
			this.context = context;
			this.publishRecord = publishRecord;
		}


		/// <summary>
		/// Constructs a <b>PublishException</b> for the given MvcRequest 
		/// and PublishRecord, with the specified message.
		/// </summary>
		/// <param name="request">The <b>MvcRequest</b> in which the exception occurred.</param>
		/// <param name="publishRecord">The <b>PublishRecord</b> in use at the time 
		///			the exception occurred.</param>
		/// <param name="message">The error message to return.</param>
		public PublishException(
			MvcRequest request,
			PublishRecord publishRecord,
			string message) : base(message)
		{
			this.request = request;
			this.publishRecord = publishRecord;
		}

		/// <summary>
		/// Constructs a <b>PublishException</b> for the given MvcRequest 
		/// and PublishRecord, with the specified message.
		/// </summary>
		/// <param name="request">The <b>MvcRequest</b> in which the exception occurred.</param>
		/// <param name="publishRecord">The <b>PublishRecord</b> in use at the time 
		///			the exception occurred.</param>
		/// <param name="message">The error message to return.</param>
		/// <param name="innerException">The exception that occured.</param>
		public PublishException(
			MvcRequest request,
			PublishRecord publishRecord,
			string message,
			Exception innerException)
			: base(message, innerException)
		{
			this.request = request;
			this.publishRecord = publishRecord;
		}
	}
}