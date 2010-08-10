namespace Triton.Membership.Model
{
	public class Name
	{
		public virtual long? Id { get; set; }

		public virtual long? Version { get; set; }

		public virtual string First { get; set; }

		public virtual string Last { get; set; }

		public virtual string Middle { get; set; }

		public virtual string SuffixCode { get; set; }

		public virtual NameSuffix Suffix { get; set; }

		public virtual string PrefixCode { get; set; }

		#region Formatting Methods

		/// <summary>
		/// Formats the Name.
		/// the default format is prefix{p} first{f} middle{m} last{l} suffix{s}.
		/// </summary>
		/// <param name="format">
		/// the following formatting directives can be used to create a different result
		/// example ToString("{l}, {f} {m}") ==&gt; Smith, Jim R.
		/// <b>{p}</b> the Name.PrefixCode value
		/// <b>{f}</b> the Name.First value
		/// <b>{m}</b> the Name.Middle value
		/// <b>{l}</b> the Name.Last value
		/// <b>{s}</b> the Name.SuffixCode value
		/// </param>
		/// <returns>
		/// The formatted string of the name.
		/// </returns>
		public string ToString(string format)
		{
			string result = format;
			if (string.IsNullOrEmpty(format))
			{
				result = "{p} {f} {m} {l} {s}";
			}			
			
			result = result.Replace("{p}", this.PrefixCode);
			result = result.Replace("{f}", this.First);
			result = result.Replace("{m}", this.Middle);
			result = result.Replace("{l}", this.Last);
			result = result.Replace("{s}", this.SuffixCode);

			return result;
		}
		
		/// <summary> Formats the Name in the default format (prefix first middle last suffix).
		/// </summary>
		/// <returns> The formatted string of the name.
		/// </returns>
		public override string ToString()
		{
			return ToString(string.Empty);
		}


		#endregion
	}
}