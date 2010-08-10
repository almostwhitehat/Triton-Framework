using System.Collections.Generic;

namespace Triton.Media.Model
{
	public class MediaType
	{
		public virtual int Id { get; set; }

		public virtual string Code { get; set; }

		public virtual string Description { get; set; }

		public virtual int Version { get; set; }

		public virtual IList<string> FileTypes { get; set; }
	}
}