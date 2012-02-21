using System.Collections.Generic;
using Triton.Model;

namespace Triton.Media.Model.Dao
{
	public interface IMediaDao
	{
		Media Get(int id);
		MediaFilter GetFilter();
		SearchResult<Media> Find(MediaFilter filter);
		void Save(Media stageCode);
		void Delete(Media stageCode);
	}
}