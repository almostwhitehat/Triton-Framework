using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Triton.Model;
using Triton.NHibernate.Model.Dao;
using Triton.NHibernate.Model.Dao.Support;

namespace Triton.Membership.Model.Dao
{
	public class NhAccountDao : NHibernateBaseDao<Account>, IAccountDao
	{
		#region IAccountDao Members

		public AccountFilter GetFilter()
		{
			return new AccountFilter();
		}
        

		public Account Get(Guid id)
		{
			return base.Get(id);
		}


		public SearchResult<Account> Find(AccountFilter filter)
		{
			List<Account> accounts = new List<Account>();

			int totalMatches = 0;

			if (filter.Ids != null && filter.Ids.Length == 1) {

				Account account = this.Get(filter.Ids[0]);
				if (account != null) {
					accounts.Add(account);
					totalMatches = 1;
				}

			} else {
				ICriteria criteria = base.Session.CreateCriteria<Account>("account");
				
				if(filter.Ids != null && filter.Ids.Length > 0) {
					criteria = criteria.Add(Expression.In("Id", filter.Ids));
				}

				if(filter.Usernames != null && filter.Usernames.Length > 0) {
					//figure out what the criteria is for searching in a list of values.
					criteria = criteria.CreateAlias("Usernames", "usernames")
						.Add(Expression.In("usernames.Value", filter.Usernames));

				}

				if (!string.IsNullOrEmpty(filter.Name)) {
                    criteria = criteria.CreateAlias("Person", "person")
						.Add(Expression.Or(Expression.Like("person.Name.Last", filter.Name, MatchMode.Start), 
						Expression.Like("person.Name.First", filter.Name, MatchMode.Start)));
				}

				accounts = SessionUtilities.GetItems<Account>(base.Session, criteria, filter, out totalMatches);
			}


			SearchResult<Account> results = new SearchResult<Account>(
				accounts.ToArray(),
				filter,
				filter.Page ?? 1,
				filter.PageSize ?? accounts.Count,
				totalMatches);

			return results;
		}

		#endregion
	}
}