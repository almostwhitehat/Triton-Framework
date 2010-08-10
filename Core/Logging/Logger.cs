using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Threading;
using Triton.Utilities.Configuration;

namespace Triton.Logging
{
	public enum MessageLevel
	{
		CONFIG,
		INFO,
		WARNING,
		ERROR,
		SEVERE_ERROR
	}

	public delegate void EnterQueueEventHandler(object sender,
	                                            EventArgs e);

	/// <summary>
	/// Logger objects are responsible for publishing messages to a data store.  There can only be one Logger
	/// of a given name, all of which are stored in a Singleton instance of LogManager.  Logger objects are
	/// instantiated through the static method Logger.GetLogger(loggerName) which will check LogManager first
	/// to see if it exists.  If not, it will attempt to create the logger dynamically from the Loggers.config
	/// file.  If this fails (logger does not have predefined settings), a custom Logger is created.  Once a 
	/// Logger is available, call any of the Log methods (or shortcut methods) to have the message processed.  When 
	/// Once these methods are called, the message is validated by checking it's level against the level of the 
	/// Logger object as well as applying a filter, if available.  If it passes, it is added to the message 
	/// queue for later processing, else nothing.  There is a TimerCallback delegate used, one for each unique Logger,
	/// to process the queue on a separate thread.  This delegate has a timer set so that it processes at the 
	/// given interval.  The process method loops all messages currently in the queue and for each, loops all 
	/// handlers for the Logger used to publish the message.  Any number of Handler objects can be added to a 
	/// Logger for message processing.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class Logger
	{
		private const int DEFAULT_QUEUE_PROCESS_TIME = 100;
		private const string QUEUE_SETTINGS_CHECK_TIME_ATTR = "queCheckTime";
		private const string QUEUE_SETTINGS_CONFIG_NAME = "queSettings";

		private readonly ArrayList handlers = new ArrayList();
		private readonly Queue messageQueue = new Queue();
		private readonly Timer queTimer;
		private readonly TimerCallback timerDelegate;
		private bool addInnerExceptions;
		private ILogFilter filter;
		private MessageLevel level = MessageLevel.CONFIG;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <remarks>A TimerCallback delegate is used for each instance of Logger so we have one thread that processes the Queue.
		/// A timer is set on the deleagate to start the thread and execute it at the specified time interval.</remarks>
		/// <param name="name"><c>String</c> that specifies the name of the <c>Logger</c></param>
		/// <returns><c>Logger</c> object</returns>
		protected Logger(
			String name)
		{
			this.Name = name;
			this.timerDelegate = this.ProcessQueue;
			this.queTimer = new Timer(this.timerDelegate, null, DEFAULT_QUEUE_PROCESS_TIME, DEFAULT_QUEUE_PROCESS_TIME);
		}


		/// <summary>
		/// Name Property.
		/// </summary>
		/// <remarks>Gets/Sets the name of the Logger.</remarks>
		/// <param name="value"><c>String</c> that specifies the name of the <c>Logger</c></param>
		/// <returns><c>Logger</c> name</returns>
		public String Name { get; set; }


		/// <summary>
		/// Level Property.
		/// </summary>
		/// <remarks>Gets/Sets the minimun level of messages that the Logger will publish.</remarks>
		/// <param name="value"><c>String</c> that specifies the level of the <c>Logger</c></param>
		/// <returns><c>Logger</c> level</returns>
		public MessageLevel Level
		{
			get { return this.level; }
			set { this.level = value; }
		}


		/// <summary>
		/// Filter Property.
		/// </summary>
		/// <remarks>Gets/Sets Filter object for the Logger.</remarks>
		/// <param name="value"><c>ILogFilter</c> that specifies the Filter of the <c>Logger</c></param>
		/// <returns><c>Logger</c> filter</returns>
		public ILogFilter Filter
		{
			get { return this.filter; }
			set { this.filter = value; }
		}


		/// <summary>
		/// Specifies whether or not <c>ERROR</c> and <c>SEVERE_ERROR</c> logging add the inner exceptions to
		/// the output
		/// </summary>
		/// <param name="value"><c>True</c> if you want the inner exceptions to write out to the logs,
		/// <c>False</c> otherwise.</param>
		/// <returns><c>Boolean</c> specifying whether or not inner exceptions are written to the logs</returns>
		public bool ReportInnerExceptions
		{
			get { return this.addInnerExceptions; }
			set { this.addInnerExceptions = value; }
		}


		/// <summary>
		/// Returns a <c>Logger</c> object of the desired name and level.
		/// </summary>
		/// <param name="name"><c>String</c> that specifies the name of the <c>Logger</c></param>
		/// <returns><c>Logger</c> object</returns>
		public static Logger GetLogger(
			String name)
		{
			// Retrieve Logger from LogManager
			Logger log = LogManager.GetLogManager().GetLogger(name);

			// Create new Logger and add it to LogManager if LogManager does not have a reference to the requested Logger
			if (log == null) {
				log = FillLogger(name);

				if (log != null) {
					LogManager.GetLogManager().AddLogger(log);
				}
			}

			return log;
		}


		/// <summary>
		/// Synchronously clears the message queue.
		/// </summary>
		public void Flush()
		{
			this.ProcessQueue(null);
		}


		/// <summary>
		/// Performs level validation for message and if it passes, adds it to the message Queue.
		/// </summary>
		/// <param name="level"><c>MessageLevel</c> that specifies the level of the message</param>
		/// <param name="message"><c>String</c> that specifies the message text</param>
		/// <returns></returns>
		public void Log(
			MessageLevel level,
			String message)
		{
			this.Log(level, message, null);
		}


		/// <summary>
		/// Performs level validation for message and if it passes, adds it to the message Queue.
		/// </summary>
		/// <param name="level"><c>MessageLevel</c> that specifies the level of the message</param>
		/// <param name="message"><c>String</c> that specifies the message text</param>
		/// <param name="exception"><c>Exception</c> that specifies the error that occured</param>
		/// <returns></returns>
		public void Log(
			MessageLevel level,
			String message,
			Exception exception)
		{
			this.Log(level, message, exception, null);
		}


		/// <summary>
		/// Performs level validation for message and if it passes, adds it to the message Queue.
		/// </summary>
		/// <param name="level"><c>MessageLevel</c> that specifies the level of the message.</param>
		/// <param name="message"><c>String</c> that specifies the message text.</param>
		/// <param name="exception"><c>Exception</c> that specifies the error that occured.</param>
		/// <param name="eventArg">An optional reference to context information about the
		///			origin of the message to log.  Pass null if no context information needed.</param>
		/// <param name="parms">Additional content to be included in the message.</param>
		public void Log(
			MessageLevel level,
			string message,
			Exception exception,
			object eventArg,
			params object[] parms)
		{
			LogMessage logMessage = new LogMessage(message, level);

			// Continue if the message passes logging criteria
			if (this.IsLoggable(logMessage)) {
				// Log exception info if applicable
				if (exception != null) {
					logMessage.Message += " : " + exception.Message;
					logMessage.Message += Environment.NewLine + "  " + exception.StackTrace;
					if (this.addInnerExceptions && exception.InnerException != null) {
						logMessage.Message += Environment.NewLine + "The following inner exceptions occurred:" +
						                      Environment.NewLine;
						while (exception.InnerException != null) {
							logMessage.Message += exception.InnerException.ToString();
							exception = exception.InnerException;
						}
					}
				}

				//  if an eventArg value was supplied, set it in the message
				if (eventArg != null) {
					logMessage.EventArg = eventArg;
				}

				// Log all param elements
				if (parms != null) {
					if (parms.Length > 0) {
						logMessage.Message += Environment.NewLine + "  Params= ";
						foreach (object item in parms) {
							if (item != null) {
								logMessage.Message += item + " ";
							}
						}
					}
				}

				this.QueueMessage(logMessage);
			}

			logMessage = null;
		}


		/// <summary>
		/// Shortcut to Log for an ERROR message.
		/// </summary>
		/// <param name="message"><c>String</c> that specifies the message text</param>
		/// <param name="exception"><c>Exception</c> that specifies the error that occured</param>
		/// <returns></returns>
		public void Error(
			String message,
			Exception exception)
		{
			this.Log(MessageLevel.ERROR, message, exception);
		}


		/// <summary>
		/// Shortcut to Log for a CONFIG message.
		/// </summary>
		/// <param name="message"><c>String</c> that specifies the message text</param>
		/// <returns></returns>
		public void Config(
			String message)
		{
			this.Log(MessageLevel.CONFIG, message);
		}


		/// <summary>
		/// Shortcut to Log for a INFO message.
		/// </summary>
		/// <param name="message"><c>String</c> that specifies the message text</param>
		/// <returns></returns>
		public void Status(
			String message)
		{
			this.Log(MessageLevel.INFO, message);
		}


		/// <summary>
		/// Shortcut to Log for a WARNING message.
		/// </summary>
		/// <param name="message"><c>String</c> that specifies the message text</param>
		/// <returns></returns>
		public void Warning(
			String message)
		{
			this.Log(MessageLevel.WARNING, message);
		}


		/// <summary>
		/// Shortcut to Log for a SEVERE_ERROR message.
		/// </summary>
		/// <param name="message"><c>String</c> that specifies the message text</param>
		/// <param name="exception"><c>Exception</c> that specifies the error that occured</param>
		/// <returns></returns>
		public void SevereError(
			String message,
			Exception exception)
		{
			this.Log(MessageLevel.SEVERE_ERROR, message, exception);
		}


		/// <summary>
		/// Adds a LogHandler object to the handlers collection which indicates how the messages for this Logger should be handled.
		/// </summary>
		/// <returns></returns>
		public void AddHandler(
			LogHandler handler)
		{
			this.handlers.Add(handler);
		}


		/// <summary>
		/// Performs level validation testing message level against Logger level, and applies the filter.
		/// </summary>
		/// <param name="message"><c>LogMessage</c> object to validate</param>
		/// <returns><b>bool</b> indicating if the <c>LogMessage</c> can be published according to its level</returns>
		private bool IsLoggable(
			LogMessage message)
		{
			bool returnVal = false;

			try {
				returnVal = ((message.Level >= this.level) && (this.handlers.Count > 0));

				// Continue check if return value is currently true
				if (returnVal) {
					// Check filter object
					if (this.filter != null) {
						// Test if filter allows message to pass
						if (!this.filter.IsLoggable(message)) {
							returnVal = false;
						}
					}
				}
			} catch {}

			return returnVal;
		}


		/// <summary>
		/// Adds the message to the <c>Queue</c> and triggers an event to process the queue.
		/// </summary>
		/// <param name="message"><c>LogMessage</c> object to validate</param>
		/// <returns></returns>
		private void QueueMessage(
			LogMessage message)
		{
			// Lock queue for thread-safety
			lock (this.messageQueue) {
				try {
					this.messageQueue.Enqueue(message);
				} catch {
					// do we want to do something with this??
				}
			}
		}


		/// <summary>
		/// Processes the messages in the <c>Queue</c>.
		/// </summary>
		/// <returns></returns>
		private void ProcessQueue(
			object state)
		{
			LogMessage message = null;

			// Loop all message in the queue
			try {
				lock (this.messageQueue) {
					while (this.messageQueue.Count > 0) {
						message = (LogMessage) this.messageQueue.Dequeue();

						if (message != null) {
							// Loop all handlers
							foreach (LogHandler handler in this.handlers) {
								// publish each message using each handler if the message level surpasses the handler level
								if (message.Level >= handler.Level) {
									handler.Publish(message);
								}
							}
						}
					}
				}
			} catch (Exception ex) {
				int k = 0;
				// Queue is empty
			}
		}


		/// <summary>
		/// Creates the Logger object from settings in the Loggers.config.
		/// </summary>
		/// <param name="name"><c>String</c> that specifies the name of the <c>Logger</c> to lookup.</param>
		/// <returns><c>Logger</c> name</returns>
		private static Logger FillLogger(
			string name)
		{
			Logger log = null;
			XmlConfiguration config = new XmlConfiguration();

			try {
				//  get the path to the loggers.config file from web.config
				string loggersConfigPath = ConfigurationSettings.AppSettings["rootPath"];
				// Load XmlConfiguration to get settings for this logger
				config.Load(loggersConfigPath + ConfigurationSettings.AppSettings["loggersConfigPath"]);
				//config.Load(new XmlTextReader(loggersConfigPath + ConfigurationSettings.AppSettings["loggersConfigPath"]));
				//config.Load(new XmlTextReader("http://fpwssdev1/PandaDen/XSLT%20Templates/loggers.config"));

				log = new Logger(name);

				// If there is configuration information, proceed, else return a new empty Logger
				if (config != null) {
					// this code will read the queue process time from a "settings" section of the config file:
					//	<settings>
					//		<!-- the time interval at which the log queue-->
					//		<setting name="queSettings" queCheckTime="10000" />
					//	</settings>
					//XmlConfiguration queSettings = config.GetConfig("settings", QUEUE_SETTINGS_CONFIG_NAME);

					//if (queSettings != null) {
					//    try {
					//        NameValueCollection queAttr = queSettings.GetAttributes("./*[1]");

					//        int period = int.Parse(queAttr[QUEUE_SETTINGS_CHECK_TIME_ATTR]);

					//        log.queTimer.Change(period, period);
					//    } catch {}
					//}

					//  get the logger's config settings
					config = config.GetConfig("loggers", name);

					NameValueCollection loggerAttrs = config.GetAttributes("./*[1]");

					// Loop log collection attributes to dynamically set the log object's properties
					for (int i = 0; i < loggerAttrs.Count; i++) {
						string attrName = loggerAttrs.GetKey(i);
						string attrValue = loggerAttrs.Get(i);

						//  if the attribute is the queue check time attribute, get the value
						//  and update the queTimer with the new value (in milliseconds)
						if (attrName == QUEUE_SETTINGS_CHECK_TIME_ATTR) {
							try {
								int period = int.Parse(attrValue);
								log.queTimer.Change(period, period);
							} catch {}
						} else {
							//  set the corresponding attribute of the object
							SetObjectAttribute(log, attrName, attrValue);
						}
					}

					// Add Filter and all Handler objects
					log.Filter = FillFilter(config);
					log.FillHandlers(config);

					// Logger objects must have at least one handler
					if (log.handlers.Count == 0) {
						log = null;
					}
				}
			} catch (Exception e) {
				log = null;
			}

			return log;
		}


		/// <summary>
		/// Creates a filter to apply the the Logger from the corresponding section in Loggers.config.
		/// </summary>
		/// <param name="name"><c>String</c> that specifies the name of the <c>Logger</c> to lookup.</param>
		/// <returns><c>ILogFilter</c> set for Logger from config file.</returns>
		private static ILogFilter FillFilter(
			XmlConfiguration config)
		{
			ILogFilter filter = null;
			NameValueCollection filterColl = null;
			string filterClass = null;

			config = config.GetConfig("logger", "filter");

			// Create the Filter object
			try {
				filterColl = config.GetAttributes("filter");
				filterClass = filterColl["class"];
				filter = (ILogFilter) Activator.CreateInstance(Type.GetType(filterClass));
			} catch (Exception e) {
				// System.MissingMethodException
			}

			return filter;
		}


		/// <summary>
		/// Creates the Handler objects from the corresponding section in Loggers.config.
		/// </summary>
		/// <param name="config"><c>XmlConfiguration</c> section of Loggers.config to pull Handler object settings.</param>
		/// <returns></returns>
		private void FillHandlers(
			XmlConfiguration config)
		{
			NameValueCollection handlerColl = null;
			LogHandler handler = null;
			string handlerClass = null;
			String attrName = null;
			String attrValue = null;

			// Loop handler nodes
			for (int i = 1; i <= config.SelectNodes("//handlers/handler").Count; i++) {
				handlerColl = config.GetAttributes("//handlers/handler[" + i + "]");

				try {
					// Create handler
					handlerClass = handlerColl["class"];
					handler = (LogHandler) Activator.CreateInstance(Type.GetType(handlerClass));

					// Loop handler attributes and dynamically create the objects properties
					for (int j = 0; j < handlerColl.Count; j++) {
						attrName = handlerColl.GetKey(j);
						attrValue = handlerColl.Get(j);
						//  set the corresponding attribute of the object
						SetObjectAttribute(handler, attrName, attrValue);
					}
					// Add handler to collection
					this.AddHandler(handler);
				} catch (Exception e) {
					string m = e.Message;
				}

				handler = null;
				handlerClass = null;
				handlerColl = null;
			}
		}


// TODO:  May need to remove to XmlUtilities.cs
		/// <summary>
		/// Dynamically sets public properties of the Logger using reflection and Loggers.config.  Attributes will map to 
		/// object public properties that need to be set.
		/// </summary>
		/// <param name="obj"><c>Object</c> (Logger object).</param>
		/// <param name="attrName"><c>String</c> name of Logger property.</param>
		/// <param name="attrValue"><c>String</c> value to set Logger property to.</param>
		/// <returns></returns>
		public static void SetObjectAttribute(
			object obj,
			string attrName,
			string attrValue)
		{
			// Get a reference to the desired propery of the Logger if it exists
			Type theType = obj.GetType();
			MemberInfo[] mbrInfoArray = theType.FindMembers(MemberTypes.Property,
			                                                BindingFlags.Instance | BindingFlags.Public,
			                                                Type.FilterNameIgnoreCase,
			                                                attrName);

			if (mbrInfoArray.Length == 1) {
				PropertyInfo property = (PropertyInfo) mbrInfoArray[0];
				Object param = null;

				// Get Parse method for obj's Type and execute
				// Enum type, Parse has different method signature
				if (property.PropertyType.IsEnum) {
					MethodInfo parseMethod = property.PropertyType.BaseType.GetMethod("Parse",
					                                                                  new Type[3]{
					                                                                             	Type.GetType("System.Type"), "".GetType(),
					                                                                             	true.GetType()
					                                                                             });

					if ((parseMethod != null) && (parseMethod.IsStatic)) {
						param = parseMethod.Invoke(null, new object[3]{property.PropertyType, attrValue, true});
					}

					// String type, can't Parse a string to a string
				} else if (Type.GetTypeCode(property.PropertyType) == Type.GetTypeCode("".GetType())) {
					param = attrValue;

					// Other types
				} else {
					MethodInfo parseMethod = property.PropertyType.GetMethod("Parse", new Type[1]{"".GetType()});

					if ((parseMethod != null) && (parseMethod.IsStatic)) {
						param = parseMethod.Invoke(null, new object[1]{attrValue});
					}
				}

				try {
					if (param != null) {
						//  Get the property setter method for the attribute and call it
						MethodInfo propertySetter = property.GetSetMethod();
						Object returnVal = propertySetter.Invoke(obj, new[]{param});
					}
				} catch (MissingMethodException e) {}
			}
		}
	}
}