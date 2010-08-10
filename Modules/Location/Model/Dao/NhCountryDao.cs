using Triton.NHibernate.Model.Dao;

namespace Triton.Location.Model.Dao
{
	public class NhCountryDao : NHibernateBaseDao<Country>, ICountryDao
	{
		#region ICountryDao Members

		public Country Get(int id)
		{
			return base.Get(id);
		}

		#endregion
	}
}