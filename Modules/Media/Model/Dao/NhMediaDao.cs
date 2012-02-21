using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Triton.Model;
using Triton.NHibernate.Model.Dao;
using Triton.NHibernate.Model.Dao.Support;
using Triton.Model.Dao;


namespace Triton.Media.Model.Dao
{
	/// <summary>
	/// Media DAO Class
	/// </summary>
	public class NhMediaDao : NHibernateBaseDao<Media>, IMediaDao
	{
		#region IMediaDao Members

		/// <summary>
		/// summary
		/// </summary>
		/// <param name="id">integer</param>
		/// <returns></returns>
		public Media Get(int id)
		{
			return base.Get(id);
		}


		/// <summary>
		/// summary
		/// </summary>
		/// <param name="filter">byexample</param>
		/// <returns></returns>
		public SearchResult<Media> Find(MediaFilter filter)
		{
			List<Media> medias = new List<Media>();

			int totalMatches = 0;

			if (filter.Ids != null && filter.Ids.Length == 1 ) {

				Media media = this.Get(filter.Ids[0]);
				if (media != null) {
					medias.Add(media);
					totalMatches = 1;
				}
			}
			else {
				ICriteria criteria = base.Session.CreateCriteria<Media>("Media");

				if (filter.Ids != null && filter.Ids.Length > 0) {
					criteria = criteria.Add(Expression.In("Id", filter.Ids));
				}

				if (filter.Name != null) {
					criteria = criteria.Add(Expression.Eq("Name", filter.Name));
				}

				criteria = criteria.AddOrder(Order.Asc("SortOrder"));
				
				medias = SessionUtilities.GetItems<Media>(base.Session, criteria, new BaseFilter(), out totalMatches);
			}


			SearchResult<Media> results = new SearchResult<Media>(
				medias.ToArray(),
				filter,
				1,
				medias.Count,
				totalMatches);

			return results;
		}

		public void Save(Media stageCode)
		{
			throw new NotImplementedException();
		}

		public void Delete(Media stageCode)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IMediaDao Members


		public MediaFilter GetFilter()
		{
			return new MediaFilter();
		}

		#endregion
	}
}
