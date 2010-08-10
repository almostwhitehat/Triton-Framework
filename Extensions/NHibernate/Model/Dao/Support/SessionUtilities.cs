using System.Collections;
using System.Collections.Generic;
using Common.Logging;
using NHibernate;
using Triton.Model.Dao;
using Triton.Utilities;

namespace Triton.NHibernate.Model.Dao.Support
{
	public class SessionUtilities
	{
		public static List<T> GetItems<T>(
			ISession session,
			ICriteria criteria,
			BaseFilter filter,
			out int count)
		{
			ICriteria rowCount = CriteriaTransformer.TransformToRowCount(criteria);

			criteria = AddPagingCriteria(criteria, filter);

			IList retValues = session.CreateMultiCriteria()
				.Add(criteria)
				.Add(rowCount)
				.List();

			count = (int) ((IList) retValues[1])[0];

			return ListUtilities.ConvertToGenericList<T>((IList) retValues[0]);
		}


		public static ICriteria AddPagingCriteria(
			ICriteria criteria,
			BaseFilter filter)
		{
			ICriteria pagingCriteria = CriteriaTransformer.Clone(criteria);

			if (filter.PageSize.HasValue && filter.Page.HasValue) {
				int page = filter.Page.Value;
				if(page == 0) {
					LogManager.GetCurrentClassLogger().Warn(warn => warn("Your paging is starting at 0, reseting to start at 1, the Pagination control will not render correctly."));
					page = 1;
				}

				pagingCriteria =
					pagingCriteria.SetFirstResult((page - 1) * filter.PageSize.Value).SetMaxResults(filter.PageSize.Value);
			} else {
				LogManager.GetCurrentClassLogger().Warn(warn => warn("The AddPagingCriteria was called with either Page or PageSize not set. Page is set to: {0}. PageSize is set to: {1} ", filter.Page, filter.PageSize));
			}

			return pagingCriteria;
		}
	}
}