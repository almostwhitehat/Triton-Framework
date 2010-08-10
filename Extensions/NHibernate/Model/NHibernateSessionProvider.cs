using System;
using System.Web;
using Common.Logging;
using NHibernate;
using NHibernateConfiguration = NHibernate.Cfg;

namespace Triton.NHibernate.Model
{

	public class NHibernateSessionProvider
	{
		private const string SESSION_NHIBERNATE_SESSION = "NHibernateSupport_Session";
		private static readonly object threadLock = new object();
		private static NHibernateSessionProvider instance;
		private readonly ISessionFactory sessionFactory;
		private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
		//private ISession session;

		/// <summary>
		/// Initializes a new instance of the NHibernateSessionProvider class.
		/// </summary>
		private NHibernateSessionProvider()
		{
			Logger.Debug(debugMessage => debugMessage("Creating a new NHibernateSessionProvider."));
				
			this.sessionFactory = this.GetSessionFactory();
		}


		public static NHibernateSessionProvider Instance
		{
			get
			{
				if (instance == null) {
					lock (threadLock) {
						if (instance == null) {
							instance = new NHibernateSessionProvider();
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
			if (HttpContext.Current.Session[SESSION_NHIBERNATE_SESSION] as ISession == null) {
				Logger.Debug(debugMessage => debugMessage("ISession was not found in the HttpSession, creating a new one."));
				
				this.CreateSession();
			}
			return HttpContext.Current.Session[SESSION_NHIBERNATE_SESSION] as ISession;
		}


		internal void DestroySession()
		{
			try {
				Logger.Debug(debugMessage => debugMessage("Call to destroy session."));

				if (HttpContext.Current != null && HttpContext.Current.Session != null) {
					ISession session = HttpContext.Current.Session[SESSION_NHIBERNATE_SESSION] as ISession;
					if (session != null) {
						session.Close();
						session.Dispose();
						Logger.Debug(debugMessage => debugMessage("Session closed and disposed."));
					}
					HttpContext.Current.Session[SESSION_NHIBERNATE_SESSION] = null;
				}
			} catch (Exception ex) {
				Logger.Error(errorMessage => errorMessage("Error occured when trying to destroy the session.", ex));
			}
		}


		internal void CreateSession()
		{
			HttpContext.Current.Session[SESSION_NHIBERNATE_SESSION] = this.sessionFactory.OpenSession();
		}


		private ISessionFactory GetSessionFactory()
		{
			return (new NHibernateConfiguration.Configuration()).Configure().BuildSessionFactory();
		}
	}
}