using System.Collections.Generic;

namespace Triton.Media.Model.Dao
{
	public interface IMediaDao
	{
		Media Get(int id);
		Media Get(string code);
		IList<Media> Get(Media example);
		void Save(Media stageCode);
		void Delete(Media stageCode);
	}
}