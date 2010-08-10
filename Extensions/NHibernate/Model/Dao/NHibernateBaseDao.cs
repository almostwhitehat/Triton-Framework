using System.Collections.Generic;
using Common.Logging;
using NHibernate;
using NHibernate.Criterion;

namespace Triton.NHibernate.Model.Dao
{
	public abstract class NHibernateBaseDao<T>
	{
		private readonly ILog log = LogManager.GetLogger(typeof (NHibernateBaseDao<T>));
		private ISession session;

		public ISession Session
		{
			get
			{
				if (this.session == null) {
					//new up a session if the session has not been set
					this.session = NHibernateSessionProvider.Instance.GetSession();
				}
				return this.session;
			}

			set { this.session = value; }
		}


		/// <summary>
		/// Get a collection of objects that match the specified example.
		/// </summary>
		/// <param name="example">Example of the type of object to retrieve.</param>
		/// <returns>IList of matched objects</returns>
		public virtual IList<T> Get(T example)
		{
			return this.Session.CreateCriteria(typeof (T)).Add(Example.Create(example)).List<T>();
		}


		/// <summary>
		/// Get the object by id.
		/// </summary>
		/// <param name="id">id of the object to retrieve.</param>
		/// <returns>object of type T</returns>
		public virtual T Get(object id)
		{
			return this.Session.Get<T>(id);
		}


		/// <summary>
		/// Save the object using SaveOrUpdate.
		/// </summary>
		/// <param name="obj">Object to save.</param>
		public virtual void Save(T obj)
		{
			using (ITransaction tx = this.Session.BeginTransaction()) {
				try {
					this.Session.SaveOrUpdate(obj);
					tx.Commit();
				} catch (HibernateException ex) {
					tx.Rollback();
					this.log.Error(errorMessage => errorMessage("Problem with saving the object.", ex));
					throw;
				}
			}
		}


		/// <summary>
		/// Uses SaveOrUpdate to persist the objects to the database.
		/// </summary>
		/// <param name="objs">List of objects to persist.</param>
		public virtual void Save(IList<T> objs)
		{
			using (ITransaction tx = this.Session.BeginTransaction()) {
				try {
					foreach (T obj in objs) {
						this.Session.SaveOrUpdate(obj);
						tx.Commit();
					}
				} catch (HibernateException ex) {
					tx.Rollback();
					this.log.Error(errorMessage => errorMessage("Problem with saving the list of objects.", ex));
					throw;
				}
			}
		}


		/// <summary>
		/// Delete the object.
		/// </summary>
		/// <param name="obj">object to delete.</param>
		public virtual void Delete(T obj)
		{
			using (ITransaction tx = this.Session.BeginTransaction()) {
				try {
					this.Session.Delete(obj);
					tx.Commit();
				} catch (HibernateException ex) {
					tx.Rollback();
					this.log.Error(errorMessage => errorMessage("Problem with deleting an object.", ex));
					throw;
				}
			}
		}
	}
}