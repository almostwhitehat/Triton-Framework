using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Triton.Model;
using Triton.NHibernate.Model.Dao;
using Triton.NHibernate.Model.Dao.Support;

namespace Triton.Membership.Model.Dao
{
	public class NhRoleDao : NHibernateBaseDao<Role>, IRoleDao
	{
		#region IRoleDao Members

		public Role Get(int id)
		{
			return base.Get(id);
		}


		public RoleFilter GetFilter()
		{
			return new RoleFilter();
		}


		public SearchResult<Role> Find(RoleFilter filter)
		{
			List<Role> roles = new List<Role>();

			int totalMatches = 0;

			if (filter.Ids != null && filter.Ids.Length == 1) {
				Role role = this.Get(filter.Ids[0]);
				if (role != null) {
					roles.Add(role);
					totalMatches = 1;
				}
			} else {
				ICriteria criteria = base.Session.CreateCriteria<Role>("role");

				if (filter.Ids != null && filter.Ids.Length > 0) {
					criteria = criteria.Add(Expression.In("Id", filter.Ids));
				}

				if (filter.Codes != null && filter.Codes.Length > 0) {
					criteria = criteria.Add(Expression.In("Code", filter.Codes));
				}

				roles = SessionUtilities.GetItems<Role>(base.Session, criteria, filter, out totalMatches);
			}

			SearchResult<Role> results = new SearchResult<Role>(
				roles.ToArray(),
				filter,
				filter.Page ?? 1,
				filter.PageSize ?? roles.Count,
				totalMatches);

			return results;
		}

		#endregion
	}
}