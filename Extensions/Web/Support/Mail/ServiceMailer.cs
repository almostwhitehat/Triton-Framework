using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using Common.Logging;
using Triton.Support.Collections;
using Triton.Utilities;

namespace Triton.Support.Mail
{

	#region History

	// History:
	// 6/5/2009		KP	Changed the logging to Common.Logging.
	// 09/29/2009	KP	Changed the logging method to GetCurrentClassLogger

	#endregion

	/// <summary>
	/// Service Mailer does not contact the email server directly. Instead, it 
	/// uses the service of another application to send out emails. It passes 
	/// information to the service provider by issuing http requests and queues 
	/// the requests if any error occurs.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ServiceMailer : Mailer
	{
		private const string CONFIG_EMAIL_SERVER_URL = "emailServiceUrl";
		private const string CONFIG_MAX_ATTEMPTS = "emailMaxAttempts";
		private const string CONFIG_PRIMARY_SERVICE = "primaryEmailService";
		private const string CONFIG_QUEUE_CHECK_TIME = "emailQueueCheckTime";
		private const string CONFIG_QUEUE_DIR = "emailQueueDir";
		private const string CONFIG_QUEUE_FAILED_DIR = "emailFailedDir";
		private const string CONFIG_SECONDARY_SERVICE = "secondaryEmailService";
		private const string DEFAULT_IGNORE_PARAMS = "to,emailtemplate";

		/// <summary>
		/// The directory to move queued messages to that have been retried the max
		/// # of times.
		/// </summary>
		private static String failedDir;

		/// <summary>
		/// The <c>FileQueue</c> that manages the queue of failed sends.
		/// </summary>
		private static FileQueue queue;

		/// <summary>
		/// The interval, in minutes, to check and process the queue
		/// </summary>
		private static int queueCheckTime;

		/// <summary>
		/// The directory to use to queue message that fail to go thru.
		/// </summary>
		private static String queueDir;

		/// <summary>
		/// Indicates request parameters which are to ignored from being added to the service.  The initial 
		/// defaults (to, emailtemplate) are set because otherwise they would be duplicated in the service
		/// send.
		/// </summary>
		private readonly Set ignoreParams = new Set(DEFAULT_IGNORE_PARAMS.Split(','));

		private readonly NameValueCollection parameters = new NameValueCollection();
		private string emailTemplate = "";
		private Message message;


		/// <summary>
		/// Default constructor.
		/// </summary>
		public ServiceMailer()
		{
			//  get the path to the directory for queuing messages
			if (queueDir == null) {
				queueDir = HttpContext.Current.Server.MapPath("/") + ConfigurationSettings.AppSettings[CONFIG_QUEUE_DIR];
			}
			//  get the path to the directory for failed queue messages
			if (failedDir == null) {
				failedDir = HttpContext.Current.Server.MapPath("/") + ConfigurationSettings.AppSettings[CONFIG_QUEUE_FAILED_DIR];
			}

			//  get the time interval for checking the queue -- value
			//  in config file is minutes, so convert to milliseconds
			if (queueCheckTime == 0) {
				try {
					queueCheckTime = int.Parse(ConfigurationSettings.AppSettings[CONFIG_QUEUE_CHECK_TIME]);
					//* 60 * 1000;
				} catch (Exception e) {
					LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("Parse of queueCheckTime failed."), e);
				}
			}

			//  set up the FileQueue for queuing failed requests
			if (queue == null) {
				queue = new FileQueue(queueDir,
				                      queueCheckTime,
				                      this.ProcessQueueEntry);
				try {
					queue.MaxAttempts = int.Parse(ConfigurationSettings.AppSettings[CONFIG_MAX_ATTEMPTS]);
				} catch (Exception e) {
					queue.MaxAttempts = 0;
				}
				queue.MaxAttemptsHandler = this.MaxedOutQueueEntry;
			}
		}


		/// <summary>
		/// Specifies the email to be sent out (file name of the template file)
		/// </summary>
		public string EmailTemplate
		{
			get { return this.emailTemplate; }

			set { this.emailTemplate = value; }
		}


		/// <summary>
		/// Contains other information needed by the email.
		/// </summary>
		public NameValueCollection Parameters
		{
			get { return this.parameters; }
		}


		/// <summary>
		/// Allows the addition of parameter names to ignore from being added to the service request.
		/// </summary>
		public Set IgnoreParams
		{
			get { return this.ignoreParams; }
		}


		/// <summary>
		/// Sends out email
		/// </summary>
		public override void Send()
		{
			// make sure we have the To and EmailTemplate required fields.
			if ((To.Count == 0) || (this.EmailTemplate.Length <= 0)) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("To or EmailTemplate not provided."), new MissingParamException());
			} else {
				StringBuilder queryString = new StringBuilder();
				// Construct the querystring
				queryString.Append("email=" + this.EmailTemplate);

				foreach (string toAddr in To) {
					queryString.Append("&to=" + toAddr);
				}

				foreach (string key in this.Parameters.Keys) {
					string val = this.Parameters[key];

					// Only add params that are not in the ignore list
					if (!this.ignoreParams.Contains(key.ToLower())) {
						val = HttpContext.Current.Server.UrlEncode(val);
						queryString.Append("&" + key + "=" + val);
					}
				}

				//  try sending the request to the email service
				this.SendRequest(queryString.ToString(), false);
			}
		}


		/// <summary>
		/// Processes an entry from the queue.
		/// </summary>
		/// <param name="entry">A file queue entry.</param>
		/// <returns>A boolean value that specifies if the queue entry has been 
		///			processed successfully.</returns>
		public bool ProcessQueueEntry(
			FileQueueEntry entry)
		{
			string queryString = entry["QueryString"];

			this.message = new Message();
			this.message.FromQueue = true;
			this.message.QueryString = queryString;

			return this.SendToService();
		}


		/// <summary>
		/// Implements the <c>MaxAttemptsProcessor</c> delegate for the
		/// <c>FileQueue</c>.  This is called by the <c>FileQueue</c> when
		/// a queued message is retried the max number of times.
		/// This method just moves the queue entry to another directory.
		/// </summary>
		/// <param name="entry">The queue entry that has been attempted
		///			the max # of times.</param>
		public void MaxedOutQueueEntry(
			FileQueueEntry entry)
		{
			try {
				File.Move(queueDir + entry.FileName, failedDir + entry.FileName);
			} catch (Exception e) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("ServiceMailer.MaxedOutQueueEntry "), e);
			}
		}


		/// <summary>
		/// Attempts to send the request to the email service.  It first tries sending
		/// to the primary server, and if that fails and a secondary server is specified
		/// in the config file, attempts the secondary server.  If all attempts
		/// to send the request fail, the request is queued, and attempted again later.
		/// </summary>
		/// <param name="queryString">The query string containing the parameters to
		///			send to the email service.</param>
		/// <param name="queued"><c>True</c> if the request is from a queue entry,
		///			<c>false</c> if not.</param>
		/// <returns><c>True</c> if the request was successfully sent, <c>false</c>
		///			if the send failed.</returns>
		private bool SendRequest(
			string queryString,
			bool queued)
		{
			bool retVal = true;

			this.message = new Message();
			this.message.FromQueue = queued;
			this.message.QueryString = queryString;

			this.SendThreaded();

			return retVal;
		}


		/// <summary>
		/// Creates a separate thread to call the method to perform the connection
		/// to the email service and send the email.
		/// </summary>
		private void SendThreaded()
		{
			Thread t = new Thread(this.SendToServiceThread);
			t.Start();
		}


		/// <summary>
		/// Wrapper call to <c>SendToService</c> with <c>void</c> return type
		/// to allow for use as the delegate to be called from the thread in
		/// <c>SendThreaded</c>.
		/// </summary>
		private void SendToServiceThread()
		{
			this.SendToService();
		}


		/// <summary>
		/// Makes the connection to the email service and sends the request.
		/// </summary>
		private bool SendToService()
		{
			switch (this.message.Attempts) {
				case 0:
					this.message.Server = ConfigurationSettings.AppSettings[CONFIG_PRIMARY_SERVICE];
					break;

				case 1:
					this.message.Server = ConfigurationSettings.AppSettings[CONFIG_SECONDARY_SERVICE];
					break;

				default:
					if (!this.message.FromQueue) {
						FileQueueEntry queueEntry = new FileQueueEntry();
						queueEntry["QueryString"] = this.message.QueryString;
						queue.Enqueue(queueEntry);
					}

					//  NOTE:  we exit the method here
					return false;
			}

			//  increament the attempt counter
			this.message.Attempts++;

			string url = this.message.Server; // + "?" + this.message.QueryString;

			try {
				// Create http request
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);

				// Set proxy if needed
				NameValueCollection config = (NameValueCollection) ConfigurationSettings.GetConfig("proxySettings/proxyInfo");
				if (config != null) {
					WebProxy proxy = new WebProxy(config["proxyName"], int.Parse(config["port"]));
					proxy.BypassProxyOnLocal = true;
					proxy.Credentials = CredentialCache.DefaultCredentials;
					request.Proxy = proxy;
				}

				// ---------  write the email information out via "post"  ----------
				request.Method = "POST";

				ASCIIEncoding encoding = new ASCIIEncoding();
				byte[] bytes = encoding.GetBytes(this.message.QueryString);

				// set the content type of the data being posted.
				request.ContentType = "application/x-www-form-urlencoded";

				// set the content length of the string being posted.
				request.ContentLength = bytes.Length;

				Stream reqStream = request.GetRequestStream();

				reqStream.Write(bytes, 0, bytes.Length);

				// close the Stream object.
				reqStream.Close();
				// ---------------------------------------------


				WebResponse response = request.GetResponse();
//				StreamReader reader = new StreamReader(response.GetResponseStream());
				response.Close();
			} catch (Exception e) {
				//  log an error message
				string msg;
				if (this.message.Attempts == 1) {
					msg = "Primary server unreachable: ";
				} else if (this.message.Attempts == 2) {
					msg = "Secondary server failed: ";
				} else {
					msg = "SendToService2: attempts= " + this.message.Attempts + ": ";
				}
				msg += ((this.message.FromQueue) ? "(from queue) " : "") + url;

				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage(msg), e);

				//  if it's the first or second attempt, retry
				if ((this.message.Attempts == 1) || (this.message.Attempts == 2)) {
					return this.SendToService();
				}
			}

			return true;
		}

		#region Nested type: Message

		private class Message
		{
			internal int Attempts;
			internal bool FromQueue;
			internal string QueryString;
			internal string Server;
		}

		#endregion
	}
}