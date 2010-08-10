using Triton.NHibernate.Model.Dao;

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
			return base.Get(new MediaType{
			                             	Code = code
			                             })[0];
		}

		#endregion
	}
}