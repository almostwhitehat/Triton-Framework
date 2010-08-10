using System;
using System.IO;
using System.Text;
using System.Threading;
using Common.Logging;

namespace Triton.Support.Collections {

	#region History

	// History:
	// 6/5/2009		KP	Changed the logging to Common.Logging.
	// 09/29/2009	KP	Changed the logging methods to GetCurrentClassLogger

	#endregion

	/// <summary>
	/// The <c>QueueProcessor</c> delegate defines the method to be
	/// called for processing of items in the queue.
	/// </summary>
	public delegate bool QueueProcessor(FileQueueEntry entry);

	/// <summary>
	/// The <c>MaxAttemptsProcessor</c> delegate defines the method to be
	/// called when the # of attempts for a queue entry have been exceeded.
	/// </summary>
	public delegate void MaxAttemptsProcessor(FileQueueEntry entry);


	/// <summary>
	/// A <c>FileQueue</c> provides a means for queuing information to the
	/// file system.  
	/// </summary>
	///	<author>Scott Dyke</author>
	public class FileQueue
	{
		/// <summary>
		/// The state information for the queue.
		/// </summary>
		private QueueInfo queueInfo;

		/// <summary>
		/// The timer used to periodically call the queue processing method.
		/// </summary>
		private Timer queueTimer;

		//private int cnt = 0;
		//private static int counter = 0;


		/// <summary>
		/// Constructs a new <c>FileQueue</c> with the specified properties.
		/// </summary>
		/// <param name="location">The absolute path to the directory for the queue.</param>
		/// <param name="checkInterval">The interval, in minutes, at which to process
		///			entries in the queue.</param>
		/// <param name="processMethod">The method to call to process entries in
		///			the queue.</param>
		public FileQueue(
			String location,
			int checkInterval,
			QueueProcessor processMethod)
		{
			this.queueInfo = new QueueInfo();
			this.queueInfo.queueLocation = location;
			this.queueInfo.checkInterval = checkInterval;
			this.queueInfo.processingMethod = processMethod;

			//  make sure the path to the queue directory ends with \
			if (!this.queueInfo.queueLocation.EndsWith("\\")) {
				this.queueInfo.queueLocation += "\\";
			}

			this.SetTimer();
			//cnt = ++counter;
		}


		/// <summary>
		/// Constructs a new <c>FileQueue</c> with the properties of the given <c>QueueInfo</c>.
		/// </summary>
		/// <param name="queueInfo"></param>
		private FileQueue(
			QueueInfo queueInfo)
		{
			this.queueInfo = queueInfo;
			//cnt = ++counter;
		}


		//	~FileQueue()
		//	{

		//	}


		/// <summary>
		/// Gets a count of the number of entires currently in the queue.
		/// </summary>
		public int Count
		{
			get {
				DirectoryInfo dirInfo = new DirectoryInfo(this.queueInfo.queueLocation);
				//  get the list of files in the queue directory
				FileInfo[] files = dirInfo.GetFiles("*.txt");

				return files.Length;
			}
		}


		/// <summary>
		/// Gets the path of the queue directory.
		/// </summary>
		public String Location
		{
			get {
				return this.queueInfo.queueLocation;
			}
		}


		/// <summary>
		/// Gets the interval at which the queue is processed, in minutes.
		/// </summary>
		public int CheckInterval
		{
			get {
				return this.queueInfo.checkInterval;
			}
		}


		/// <summary>
		/// Gets or sets the maximum number of attempts an entry is allowed.  After
		/// this number of attempts have been attempted, the <c>MaxAttemptsHandler</c>
		/// method is called.
		/// <p>A value 0 (zero) means there is no limit on the number of retries.
		/// <p><b>Note:</b> the <c>MaxAttemptsHandler</c> property must be set in 
		/// conjunction with this value or <c>MaxAttempts</c> will be ignored.
		/// </summary>
		public int MaxAttempts
		{
			get {
				return this.queueInfo.maxAttempts;
			}
			set {
				this.queueInfo.maxAttempts = value;
			}
		}



		/// <summary>
		/// Gets or sets the method called to handle entries that have been attempted
		/// the maximum number of times.
		/// <p><b>Note:</b> This method is only called if <c>MaxAttempts</c> is set to
		/// a value > 0.
		/// </summary>
		public MaxAttemptsProcessor MaxAttemptsHandler
		{
			get {
				return this.queueInfo.maxAttemptsMethod;
			}
			set {
				this.queueInfo.maxAttemptsMethod = value;
				this.SetTimer();
			}
		}


		/// <summary>
		/// Adds the given entry to the queue.
		/// </summary>
		/// <param name="entry">The entry to add to the queue.</param>
		public void Enqueue(
			FileQueueEntry entry)
		{
			this.WriteQueueEntry(entry);
		}


		/// <summary>
		/// Removes the given entry from the queue.
		/// </summary>
		/// <param name="entry">The entry to remove from the queue.</param>
		public void Dequeue(
			FileQueueEntry entry)
		{
			File.Delete(this.queueInfo.queueLocation + entry.FileName);
		}


		/// <summary>
		/// Checks the queue directory for any entries and if there are entries attempts
		/// to process them.  This is the method called by the <c>Timer</c> to
		/// periodically process the queue.
		/// </summary>
		/// <param name="stateInfo">The <c>QueueInfo</c> containing the state
		///			information of the <c>FileQueue</c> from which the Timer that
		///			is calling this method originated.</param>
		private static void CheckQueue(
			Object stateInfo)
		{
			QueueInfo queueInfo = (QueueInfo)stateInfo;
			FileQueue fileQueue = new FileQueue(queueInfo);
			bool hadError = false;

			LogManager.GetCurrentClassLogger().Info(
				infoMessage => infoMessage("===== Checking Queue ====="));

			DirectoryInfo dirInfo = new DirectoryInfo(queueInfo.queueLocation);

			//  get the list of files in the queue directory
			FileInfo[] files = dirInfo.GetFiles("*.txt");

			//  for each queued file...
			foreach (FileInfo info in files) {
				try {
					StreamReader reader = info.OpenText();
					FileQueueEntry entry = new FileQueueEntry();
					entry.SetFileName(info.Name);

					int keyLineCnt = 0;
					//  continue while there is more stuff in the file
					while (reader.Peek() >= 0) {
						try {
							//  read the next line of the file
							String line = reader.ReadLine();
							//  find the position of the key/value delimiter
							int delimPos = line.IndexOf(FileQueueEntry.QUEUE_FILE_DELIMITER);
							String key = line.Substring(0, delimPos);
							String val = line.Substring(delimPos + FileQueueEntry.QUEUE_FILE_DELIMITER.Length);

							//  set the attributes of the queue entry from the
							//  queued file
							if (key == FileQueueEntry.Q_LABEL_QUEUE_TIME) {
								entry.SetQueuedTime(DateTime.Parse(val));
								keyLineCnt++;
							} else if (key == FileQueueEntry.Q_LABEL_ATTEMPT_CNT) {
								entry.SetAttempts(int.Parse(val));
								keyLineCnt++;
							} else if (key == FileQueueEntry.Q_LABEL_LAST_TIME) {
								entry.SetLastAttemptTime(DateTime.Parse(val));
								keyLineCnt++;
							} else {
								entry[key] = val;
							}
						} catch (Exception e) {
							hadError = true;
							LogManager.GetCurrentClassLogger().Error(
								errorMessage => errorMessage("FileQueue.CheckQueue 1: "), e);
						}
					}
					reader.Close();

					bool processed = false;
					//  if the entry was successfully processed, removed it 
					//  from the queue
					try {
						if (!hadError && queueInfo.processingMethod(entry)) {
							fileQueue.Dequeue(entry);
							processed = true;
						}
					} catch (Exception e) {
						LogManager.GetCurrentClassLogger().Error(
							errorMessage => errorMessage("Error processing queue entry: "), e);
					}

					//  if the entry was not successfully processed, first
					//  see if we have reached the max # of attempts and it
					//  so, call the MaxAttemptsHandler, otherwise update
					//  the entry info
					//  also, make sure we at least read the key 
					//  lines required to maintain the queue entry (beyond 3 
					//  is application data)
					if (!processed && (keyLineCnt >= 3)) {
						entry.SetAttempts(entry.Attempts + 1);
						entry.SetLastAttemptTime(DateTime.Now);
						fileQueue.WriteQueueEntry(entry);
						//  if there is a max attempts set and we've reached it
						//  call the handler method (if one is defined) for max attempts 
						//  and remove the entry from the queue
						if ((queueInfo.maxAttempts > 0) && (entry.Attempts >= queueInfo.maxAttempts)) {
							if (queueInfo.maxAttemptsMethod != null) {
								queueInfo.maxAttemptsMethod(entry);
							}
							fileQueue.Dequeue(entry);
						}
					}
				} catch (Exception e) {
					LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("FileQueue.CheckQueue 2: "), e);
				}
			}

			LogManager.GetCurrentClassLogger().Info(
				infoMessage => infoMessage("===== End Queue Check ====="));
		}


		/// <summary>
		/// Writes an entry to the queue.  If the entry already exists the info in the
		/// file is updated.  If it does not exist it is created.
		/// </summary>
		/// <param name="entry">The <c>FileQueueEntry</c> to write to the queue.</param>
		private void WriteQueueEntry(
			FileQueueEntry entry)
		{
			//  get the output to be written to the queue file
			String output = entry.ToString();

			//  open and truncate the file if it already exists, otherwise
			//  create it
			FileStream file;
			String path = this.queueInfo.queueLocation + entry.FileName;
			if (File.Exists(path)) {
				file = File.Open(path, FileMode.Truncate, FileAccess.Write);
			} else {
				file = File.Create(path);
			}

			//  write the output to the file
			Byte[] content = new UTF8Encoding(true).GetBytes(output);
			file.Write(content, 0, content.Length);
			file.Close();
		}


		/// <summary>
		/// Sets up the <c>Timer</c> to periodically call the method to process the
		/// queue.  Because the timer makes a copy of the given <c>QueueInfo</c>
		/// changes to the properties of the queue are not reflected in the timer's
		/// state information.  So, whenever a property of the <c>FileQueue</c> is
		/// changed, the old timer must be disposed and a new one created with the
		/// current state info.
		/// </summary>
		private void SetTimer()
		{
			if (this.queueTimer != null) {
				this.queueTimer.Dispose();
				this.queueTimer = null;
			}

			//  convert the checkInterval from minutes to milliseconds
			long queueCheckTime = this.queueInfo.checkInterval * 60 * 1000;
			//  set up the timer to periodically call the CheckQueue method
			TimerCallback timerDelegate = CheckQueue;
			this.queueTimer = new Timer(timerDelegate, this.queueInfo, queueCheckTime, queueCheckTime);
		}


		#region Nested type: QueueInfo

		/// <summary>
		/// <c>QueueInfo</c> encapsulates the properties of a <c>FileQueue</c>.
		/// These properties represent the state of the <c>FileQueue</c> and
		/// are encapsulated in this struct so the state can be given to the 
		/// <c>Timer</c> that periodically processes the queue, and then given
		/// to the processing method.  The processing method can use this
		/// state information to create a new <c>FileQueue</c> to perform the
		/// queue processing.
		/// </summary>
		private struct QueueInfo
		{
			internal int checkInterval;
			internal int maxAttempts;
			internal MaxAttemptsProcessor maxAttemptsMethod;
			internal QueueProcessor processingMethod;
			internal String queueLocation;
		}

		#endregion
	}
}