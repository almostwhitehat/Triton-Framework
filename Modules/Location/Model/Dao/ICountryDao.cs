using System.Collections.Generic;

namespace Triton.Location.Model.Dao
{
	public interface ICountryDao
	{
		Country Get(int id);
		IList<Country> Get(Country example);
		void Save(Country country);
		void Delete(Country country);
	}
}