using System;
using System.IO;

namespace Triton.Logging
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Defines an LogHandler object that is used to write to a file.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class FileLogHandler : LogHandler
	{
		private string fileName;
		private TextWriter stream;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <remarks>Call the base constructor.</remarks>
		/// <returns><c>FileLogHandler</c> object</returns>
		public FileLogHandler() {}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <remarks>Call the base constructor.</remarks>
		/// <param name="level"><c>MessageLevel</c> of <c>LogHandler</c>.</param>
		/// <returns><c>FileLogHandler</c> object</returns>
		public FileLogHandler(
			MessageLevel level) : base(level) {}


		/// <summary>
		/// FileName property.
		/// </summary>
		/// <remarks>Sets the filename used to publish to.</remarks>
		/// <param name="value"><c>String</c> of filename.</param>
		/// <returns></returns>
		public string FileName
		{
			set { this.fileName = value; }
		}


		/// <summary>
		/// Deconstructor calls the local Close() method to clean up the TextWriter stream.
		/// </summary>
		~FileLogHandler()
		{
			this.Close();
		}


		/// <summary>
		/// Pushes the message info to a file/buffer
		/// </summary>
		/// <param name="message"><c>LogMessage</c> that is to be sent to the data source/buffer</param>
		/// <returns></returns>
		public override void Publish(
			LogMessage message)
		{
			try {
				// Uses thread-safe wrapper
				this.stream = TextWriter.Synchronized(File.AppendText(this.fileName));
				this.stream.Write("{0} [{1}] : ", message.MessageTime, message.Level);
				this.stream.WriteLine(message.Message);
			} catch (Exception) {
				// Possible errors: System.IO.DirectoryNotFoundException, System.IO.IOException
			} finally {
				this.Flush();
				this.Close();
			}
		}


		/// <summary>
		/// Flushes buffered messages to the file
		/// </summary>
		/// <returns></returns>
		public override void Flush()
		{
			try {
				if (this.stream != null) {
					this.stream.Flush();
				}
			} catch {}
		}


		/// <summary>
		/// Closes the file
		/// </summary>
		/// <returns></returns>
		public override void Close()
		{
			try {
				if (this.stream != null) {
					this.stream.Close();
					this.stream = null;
				}
			} catch {}
		}
	}
}