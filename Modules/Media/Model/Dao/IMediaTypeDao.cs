using System.Collections.Generic;

namespace Triton.Media.Model.Dao
{
	public interface IMediaTypeDao
	{
		MediaType Get(int id);
		MediaType Get(string code);
		IList<MediaType> Get(MediaType example);
		void Save(MediaType mediaType);
		void Delete(MediaType mediaType);
	}
}