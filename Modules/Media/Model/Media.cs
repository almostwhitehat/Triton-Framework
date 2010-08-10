using System;
using System.Collections.Generic;

namespace Triton.Media.Model
{
	public class Media
	{
		public virtual FileRecord File { get; set; }

		public virtual string Name { get; set; }

		public virtual DateTime? CreatedDate { get; set; }

		public virtual DateTime? UpdatedDate { get; set; }

		public virtual string Comments { get; set; }

		public virtual float SortOrder { get; set; }

		public virtual int Id { get; set; }

		public virtual int Version { get; set; }

		public virtual MediaType Type { get; set; }

		public virtual IList<Media> RelatedMedia { get; set; }
	}
}