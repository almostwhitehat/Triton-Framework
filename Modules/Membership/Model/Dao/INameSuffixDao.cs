using System.Collections.Generic;

namespace Triton.Membership.Model.Dao
{
	public interface INameSuffixDao
	{
		NameSuffix Get(int id);
		IList<NameSuffix> Get(NameSuffix example);
		void Save(NameSuffix nameSuffix);
		void Delete(NameSuffix nameSuffix);
		NameSuffix Get(string code);
	}
}