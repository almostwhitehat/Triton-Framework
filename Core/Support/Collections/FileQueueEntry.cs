using System;
using System.Collections;
using System.Text;

namespace Triton.Support.Collections
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <c>FileQueueEntry</c> is used by the <c>FileQueue</c> to represent an entry
	/// in the queue.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class FileQueueEntry
	{
		internal const String Q_LABEL_ATTEMPT_CNT = "# of attempts";
		internal const String Q_LABEL_LAST_TIME = "Time of last attempt";
		internal const String Q_LABEL_QUEUE_TIME = "Time of queuing";
		internal const String QUEUE_FILE_DELIMITER = ": ";

		private readonly String coreFileName;
		private readonly Hashtable values = new Hashtable();
		private int attemptCnt;
		private String fileName;
		private String filePrefix = "";
		private DateTime timeOfLastAttempt;
		private DateTime timeOfRequest;


		/// <summary>
		/// Default constructor -- initializes entry properties.
		/// </summary>
		public FileQueueEntry()
		{
			DateTime now = DateTime.Now;

			this.coreFileName = String.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}{6:000}.txt",
			                                  now.Year,
			                                  now.Month,
			                                  now.Day,
			                                  now.Hour,
			                                  now.Minute,
			                                  now.Second,
			                                  now.Millisecond);

			this.attemptCnt = 0;
			this.timeOfRequest = now;
			this.timeOfLastAttempt = now;
			this.fileName = this.coreFileName;
		}


		/// <summary>
		/// Gets the name of the file the queue entry is stored in.  This contains
		/// no path information.
		/// </summary>
		public String FileName
		{
			get { return this.fileName; }
		}


		/// <summary>
		/// Gets the number of attempts that have been made to re-process the queue
		/// entry.
		/// </summary>
		public int Attempts
		{
			get { return this.attemptCnt; }
		}


		/// <summary>
		/// Gets the date/time the entry was added to the queue.
		/// </summary>
		public DateTime QueuedTime
		{
			get { return this.timeOfRequest; }
		}


		/// <summary>
		/// Gets the date/time of the last attempt to process the entry.
		/// </summary>
		public DateTime LastAttemptTime
		{
			get { return this.timeOfLastAttempt; }
		}


		/// <summary>
		/// Gets or sets the prefix to use for the file name for the queue entry.
		/// The default filename is simpley a timestamp, but a prefix can be added
		/// to the timestamp if desired.
		/// </summary>
		public String FilePrefix
		{
			get { return this.filePrefix; }
			set
			{
				this.filePrefix = value;
				this.fileName = value + this.coreFileName;
			}
		}


		/// <summary>
		/// Gets or sets a dynamic attribute for the queue entry.  Dynamic attributes
		/// are typically the information the client application needs to store and
		/// are not essential to the operation of the queue.
		/// </summary>
		public String this[String key]
		{
			get { return (String) this.values[key]; }
			set { this.values[key] = value; }
		}


		/// <summary>
		/// Converts the queue entry information to a string that represents the
		/// information stored in the queue entry file.
		/// </summary>
		/// <returns></returns>
		public override String ToString()
		{
			StringBuilder buffer = new StringBuilder();

			//  add the core attributes to the string
			buffer.Append(Q_LABEL_QUEUE_TIME + QUEUE_FILE_DELIMITER + this.timeOfRequest + Environment.NewLine);
			buffer.Append(Q_LABEL_ATTEMPT_CNT + QUEUE_FILE_DELIMITER + this.attemptCnt + Environment.NewLine);
			buffer.Append(Q_LABEL_LAST_TIME + QUEUE_FILE_DELIMITER + this.timeOfLastAttempt + Environment.NewLine);

			//  add the dynamic attributes to the string
			foreach (String key in this.values.Keys) {
				buffer.Append(key + QUEUE_FILE_DELIMITER + this.values[key] + Environment.NewLine);
			}

			return buffer.ToString();
		}


		/// <summary>
		/// Sets the filename of the entry.
		/// </summary>
		/// <param name="name">The filename of the entry.</param>
		internal void SetFileName(
			String name)
		{
			this.fileName = name;
		}


		/// <summary>
		/// Sets the number of attempts that have been made for the entry.
		/// </summary>
		/// <param name="attempts">The number of attempts that have been made 
		///			for the entry.</param>
		internal void SetAttempts(
			int attempts)
		{
			this.attemptCnt = attempts;
		}


		/// <summary>
		/// Sets the time the entry was added to the queue.
		/// </summary>
		/// <param name="queuedTime">The time the entry was added to the queue.</param>
		internal void SetQueuedTime(
			DateTime queuedTime)
		{
			this.timeOfRequest = queuedTime;
		}


		/// <summary>
		/// Sets the time of the last attempt at processing the entry.
		/// </summary>
		/// <param name="lastAttemptTime">The time of the last attempt at processing 
		///			the entry.</param>
		internal void SetLastAttemptTime(
			DateTime lastAttemptTime)
		{
			this.timeOfLastAttempt = lastAttemptTime;
		}
	}
}