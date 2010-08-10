using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Triton.Model;
using Triton.NHibernate.Model.Dao;
using Triton.NHibernate.Model.Dao.Support;

namespace Triton.Membership.Model.Dao
{
	public class NhAttributeTypeDao : NHibernateBaseDao<AttributeType>, IAttributeTypeDao
	{
		#region IAttributeTypeDao Members

		public AttributeType Get(int id)
		{
			return base.Get(id);
		}


		public AttributeType Get(string code)
		{
			IList<AttributeType> types = base.Get(new AttributeType
			                                      {
			                                      	Code = code
			                                      });

			if (types.Count == 0) {
				throw new ApplicationException(string.Format("Could not find the AttributeType by the Code of: {0}.", code));
			}

			return types[0];
		}


		public AttributeTypeFilter GetFilter()
		{
			return new AttributeTypeFilter();
		}


		public SearchResult<AttributeType> Find(AttributeTypeFilter filter)
		{
			List<AttributeType> attributeTypes = new List<AttributeType>();

			int totalMatches = 0;

			if (filter.Ids != null && filter.Ids.Length == 1) {
				AttributeType role = this.Get(filter.Ids[0]);
				if (role != null) {
					attributeTypes.Add(role);
					totalMatches = 1;
				}
			} else {
				ICriteria criteria = base.Session.CreateCriteria<AttributeType>("attribute_type");

				if (filter.Ids != null && filter.Ids.Length > 0) {
					criteria = criteria.Add(Expression.In("Id", filter.Ids));
				}

				if (filter.Codes != null && filter.Codes.Length > 0) {
					criteria = criteria.Add(Expression.In("Code", filter.Codes));
				}

				attributeTypes = SessionUtilities.GetItems<AttributeType>(base.Session, criteria, filter, out totalMatches);
			}

			SearchResult<AttributeType> results = new SearchResult<AttributeType>(
				attributeTypes.ToArray(),
				filter,
				filter.Page ?? 1,
				filter.PageSize ?? attributeTypes.Count,
				totalMatches);

			return results;
		}

		#endregion
	}
}