using System;
using Common.Logging;
using NHibernate;
using NHibernateConfiguration = NHibernate.Cfg;

namespace Triton.NHibernate.Model
{

	public class MEMNHibernateSessionProvider
	{
		private const string SESSION_NHIBERNATE_SESSION = "NHibernateSupport_Session";
		private static readonly object threadLock = new object();
		private static MEMNHibernateSessionProvider instance;
		private readonly ISessionFactory sessionFactory;
		private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
		private ISession session = null;
		//private ISession session;

		/// <summary>
		/// Initializes a new instance of the NHibernateSessionProvider class.
		/// </summary>
		private MEMNHibernateSessionProvider()
		{
			Logger.Debug(debugMessage => debugMessage("Creating a new NHibernateSessionProvider."));
				
			this.sessionFactory = this.GetSessionFactory();
		}


		public static MEMNHibernateSessionProvider Instance
		{
			get
			{
				if (instance == null) {
					lock (threadLock) {
						if (instance == null) {
							instance = new MEMNHibernateSessionProvider();
						}
					}
				}
				return instance;
			}
		}


		public static void Reset()
		{
			lock (threadLock) {
				if (instance != null) {
					Logger.Debug(debugMessage => debugMessage("Call to destroy the session factory."));

					try {
						instance.sessionFactory.Close();
						instance.sessionFactory.Dispose();

						Logger.Debug(debugMessage => debugMessage("Session factory closed and disposed."));
					} catch (Exception ex) {
						Logger.Error(errorMessage => errorMessage("Error occured when trying to destroy the NHibernateSessionProvider singleton.", ex));
					}
				}
				instance = null;
			}
		}


		public ISession GetSession()
		{
			if (this.session == null) {
				Logger.Debug(debugMessage => debugMessage("ISession was not found in the HttpSession, creating a new one."));
				
				this.CreateSession();
			}
			return this.session;
		}


		internal void DestroySession()
		{
			try {
				Logger.Debug(debugMessage => debugMessage("Call to destroy session."));

				this.session= null;
			} catch (Exception ex) {
				Logger.Error(errorMessage => errorMessage("Error occured when trying to destroy the session.", ex));
			}
		}


		internal void CreateSession()
		{
			this.session = this.sessionFactory.OpenSession();
		}


		private ISessionFactory GetSessionFactory()
		{
			return (new NHibernateConfiguration.Configuration()).Configure().BuildSessionFactory();
		}
	}
}