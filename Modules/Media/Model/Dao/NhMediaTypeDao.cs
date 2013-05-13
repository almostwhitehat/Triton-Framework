using Triton.NHibernate.Model.Dao;
using System.Collections.Generic;

namespace Triton.Media.Model.Dao
{
	public class NhMediaTypeDao : NHibernateBaseDao<MediaType>, IMediaTypeDao
	{
		#region IMediaTypeDao Members

		public MediaType Get(int id)
		{
			return base.Get(id);
		}


		public MediaType Get(string code)
		{
			MediaType result = null;
			
			IList<MediaType> results = base.Get(new MediaType{ Code = code });

			if (results.Count == 1){
				result = results[0];
			}

			return result;
		}

		#endregion
	}
}