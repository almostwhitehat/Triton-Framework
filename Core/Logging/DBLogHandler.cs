using System;
using System.Data;

namespace Triton.Logging
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// Summary description for DBLogHandler.
	/// </summary>
	///	<author>Scott Dyke</author>
	public abstract class DBLogHandler : LogHandler
	{
		protected IDbConnection conn;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <remarks>Call the base constructor.</remarks>
		/// <returns><c>DBLogHandler</c> object</returns>
		public DBLogHandler() {}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <remarks>Call the base constructor.</remarks>
		/// <param name="level"><c>MessageLevel</c> of <c>LogHandler</c>.</param>
		/// <returns><c>DBLogHandler</c> object</returns>
		public DBLogHandler(
			MessageLevel level) : base(level) {}


		/// <summary>
		/// Pushes the message info to a database table entry
		/// </summary>
		/// <param name="message"><c>LogMessage</c> that is to be sent to the database</param>
		public override void Publish(
			LogMessage message)
		{
			IDbCommand cmd = null;

			try {
				// Call the method to create the database connection
				this.GetDBConnection();
				// Open the database connection
				this.OpenDBConnection();
				// Create the command for saving the log entry
				cmd = this.CreateDBCommand(message);
				// Write the log entry to the database
				this.WriteLog(cmd);
				// Close the database connection
				this.Close();
			} catch (Exception) {
				// Possible errors: 
			}
		}


		/// <summary>
		/// Abstract method to return a database connection
		/// </summary>
		/// <remarks>Store the connection as the <c>conn</c> class variable. This will allow the other
		/// abstract classes to use it directly.</remarks>
		protected abstract void GetDBConnection();


		/// <summary>
		/// Abstract method to open the database connection
		/// </summary>
		/// <remarks>Use the connection stored in the <c>conn</c> class variable. Make sure to check
		/// for null instances before opening.</remarks>
		protected abstract void OpenDBConnection();


		/// <summary>
		/// Abstract method to create the database command for storing the log entry in the database
		/// </summary>
		/// <param name="message"><c>LogMessage</c> to be stored</param>
		/// <returns><c>IDbCommand</c> populated with the necessary information to save the log entry
		/// to the database</returns>
		/// <remarks>Use the connection stored in the <c>conn</c> class variable. Make sure to check
		/// for null instances before creating the command.</remarks>
		protected abstract IDbCommand CreateDBCommand(LogMessage message);


		/// <summary>
		/// Abstract method to store the log entry in the database
		/// </summary>
		/// <param name="cmd"><c>IDbCommand</c> previously populated by a call to
		/// <c>CreateDBCommand</c></param>
		protected abstract void WriteLog(IDbCommand cmd);


		/// <summary>
		/// Flushes buffered messages to the database. Override this method if you need to process the
		/// message storage using transactions.
		/// </summary>
		public override void Flush() {}


		/// <summary>
		/// Close the database connection
		/// </summary>
		public override void Close()
		{
			if ((this.conn != null) && (this.conn.State == ConnectionState.Open)) {
				this.conn.Close();
			}

			this.conn = null;
		}
	}
}