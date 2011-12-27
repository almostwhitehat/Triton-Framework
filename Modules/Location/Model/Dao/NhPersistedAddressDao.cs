using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Triton.Model;
using Triton.NHibernate.Model.Dao;
using Triton.NHibernate.Model.Dao.Support;

namespace Triton.Location.Model.Dao
{
	public class NhPersistedAddressDao : NHibernateBaseDao<PersistedAddress>, IPersistedAddressDao
	{
		#region IPersistedAddressDao Members

		public PersistedAddressFilter GetFilter()
		{
			return new PersistedAddressFilter();
		}
        

		public PersistedAddress Get(long id)
		{
			return base.Get(id);
		}


		public SearchResult<PersistedAddress> Find(PersistedAddressFilter filter)
		{
			List<PersistedAddress> persistedAddresses = new List<PersistedAddress>();

			int totalMatches = 0;

			if (filter.Ids != null && filter.Ids.Length == 1) {

				PersistedAddress persistedAddress = this.Get(filter.Ids[0]);
				if (persistedAddress != null) {
					persistedAddresses.Add(persistedAddress);
					totalMatches = 1;
				}

			} else {
				ICriteria criteria = base.Session.CreateCriteria<PersistedAddress>("PersistedAddress");
				
				if(filter.Ids != null && filter.Ids.Length > 0) {
					criteria = criteria.Add(Expression.In("Id", filter.Ids));
				}

				if (!string.IsNullOrEmpty(filter.City)) {
					criteria = criteria.Add(Expression.Like("CityName", string.Format("{1}{0}{1}", filter.City, "%")));
				}

				if (!string.IsNullOrEmpty(filter.State)) {
					criteria = criteria.Add(Expression.Like("StateName", string.Format("{1}{0}{1}", filter.State, "%")));
				}

				if (!string.IsNullOrEmpty(filter.Country)) {
					criteria = criteria.Add(Expression.Like("CountryName", string.Format("{1}{0}{1}", filter.Country, "%")));
				}

				if (!string.IsNullOrEmpty(filter.Line1)) {
					criteria = criteria.Add(Expression.Like("Line1", string.Format("{1}{0}{1}", filter.Line1, "%")));
				}

				if (!string.IsNullOrEmpty(filter.PostalCode)) {
					criteria = criteria.Add(Expression.Like("PostalCodeName", string.Format("{1}{0}{1}", filter.PostalCode, "%")));
				}


				persistedAddresses = SessionUtilities.GetItems<PersistedAddress>(base.Session, criteria, filter, out totalMatches);
			}


			SearchResult<PersistedAddress> results = new SearchResult<PersistedAddress>(
				persistedAddresses.ToArray(),
				filter,
				filter.Page ?? 1,
				filter.PageSize ?? persistedAddresses.Count,
				totalMatches);

			return results;
		}

		#endregion
	}
}