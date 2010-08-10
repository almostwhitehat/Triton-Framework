using Triton.NHibernate.Model.Dao;

namespace Triton.Location.Model.Dao
{
	public class NhStateDao : NHibernateBaseDao<State>, IStateDao
	{
		#region IStateDao Members

		public State Get(int id)
		{
			return base.Get(id);
		}

		#endregion
	}
}