using System.Collections.Generic;
using Triton.Model;

namespace Triton.Membership.Model.Dao
{
	public interface IRoleDao
	{
		Role Get(int id);

		IList<Role> Get(Role example);

		void Save(Role role);

		void Save(IList<Role> role);

		void Delete(Role role);

		RoleFilter GetFilter();

		SearchResult<Role> Find(RoleFilter filter);
	}
}