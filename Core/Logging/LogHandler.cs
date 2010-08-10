namespace Triton.Logging
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Specifies the contract that all LogHandler objects need to follow
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class LogHandler
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <returns><c>LogHandler</c> object</returns>
		public LogHandler() {}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="level"><c>MessageLevel</c> of handler.</param>
		/// <returns><c>LogHandler</c> object</returns>
		public LogHandler(
			MessageLevel level)
		{
			this.Level = level;
		}


		/// <summary>
		/// Level property.
		/// </summary>
		/// <remarks>Gets/Sets the level of the handler.</remarks>
		/// <returns><c>MessageLevel</c> of handler</returns>
		public MessageLevel Level { get; set; }


		/// <summary>
		/// Pushes the message info to a data source/buffer
		/// </summary>
		/// <param name="message"><c>LogMessage</c> that is to be sent to the data source/buffer</param>
		/// <returns></returns>
		public abstract void Publish(
			LogMessage message);


		/// <summary>
		/// Flushes buffered messages to the data source
		/// </summary>
		/// <returns></returns>
		public abstract void Flush();


		/// <summary>
		/// Closes the data source
		/// </summary>
		/// <returns></returns>
		public abstract void Close();
	}
}