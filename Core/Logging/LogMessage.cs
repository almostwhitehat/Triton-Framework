using System;

namespace Triton.Logging
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Represents a message that that needs to be logged to a data source,
	/// allows for message text and level.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class LogMessage
	{
		private readonly DateTime messageTime = DateTime.Now;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <returns><c>LogMessage</c> object</returns>
		public LogMessage() {}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message"><c>String</c> that specifies the name of the <c>LogMessage</c>.</param>
		/// <param name="level"><c>MessageLevel</c> that specifies the level of the <c>LogMessage</c>.</param>
		/// <returns><c>LogMessage</c> object</returns>
		public LogMessage(
			String message,
			MessageLevel level)
		{
			this.Message = message;
			this.Level = level;
		}


		/// <summary>
		/// Message property.
		/// </summary>
		/// <remarks>Gets/Sets the text of the LogMessage.</remarks>
		/// <param name="value"><c>String</c> that specifies the name of the <c>LogMessage</c>.</param>
		/// <returns><c>String</c> text of message.</returns>
		public string Message { get; set; }


		/// <summary>
		/// Level property.
		/// </summary>
		/// <remarks>Gets/Sets the level of the LogMessage.</remarks>
		/// <param name="value"><c>MessageLevel</c> that specifies the name of the <c>LogMessage</c>.</param>
		/// <returns><c>MessageLevel</c> level of message.</returns>
		public MessageLevel Level { get; set; }


		/// <summary>
		/// Gets or sets the event argument for the message.  The event argument
		/// provides optional context information about the source of the message.
		/// </summary>
		public object EventArg { get; set; }


		/// <summary>
		/// Gets the time the message was generated.
		/// </summary>
		public DateTime MessageTime
		{
			get { return this.messageTime; }
		}
	}
}