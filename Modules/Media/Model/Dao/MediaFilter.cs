using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Triton.Model.Dao;

namespace Triton.Media.Model.Dao
{
	public class MediaFilter : BaseFilter
	{
		public int[] Ids { get; set; }

		public string Name { get; set; }
	}
}
