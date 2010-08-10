using System.Collections.Generic;

namespace Triton.Location.Model.Dao
{
	public interface IStateDao
	{
		State Get(int id);
		IList<State> Get(State example);
		void Save(State state);
		void Delete(State state);
	}
}