namespace Triton.Location.Model
{
	public class Address
	{
		public virtual string CityName { get; set; }

		public virtual City City { get; set; }

		public virtual string CountryName { get; set; }

		public virtual Country Country { get; set; }

		public virtual string County { get; set; }

		public virtual string Line1 { get; set; }

		public virtual string Line2 { get; set; }

		public virtual string Line3 { get; set; }

		public virtual string PostalCodeName { get; set; }

		public virtual PostalCode PostalCode { get; set; }

		public virtual string StateName { get; set; }

		public virtual State State { get; set; }

		public virtual GeoLocation GeoLocation { get; set; }

		#region Formatting Methods

		/// <summary>
		/// Formats the Name.
		/// the default format is prefix{p} first{f} middle{m} last{l} suffix{s}.
		/// </summary>
		/// <param name="format">
		/// the following formatting directives can be used to create a different result
		/// example ToString("{l}, {f} {m}") ==&gt; Smith, Jim R.
		/// <b>{l1}</b> the Address.Line1 value
		/// <b>{l2}</b> the Address.Line2 value
		/// <b>{l3}</b> the Address.Line3 value
		/// <b>{c}</b> the Address.CityName value
		/// <b>{s}</b> the Address.StateName value
		/// <b>{p}</b> the Address.PostalCodeName value
		/// <b>{la}</b> the Address.GeoLocation.Latitude value
		/// <b>{lo}</b> the Address.GeoLocation.Longitude value
		/// <b>{gps}</b> the Address.GeoLocation.ToString() value
		/// </param>
		/// <returns>
		/// The formatted string of the name.
		/// </returns>
		public virtual string ToString(string format)
		{
			string result = format;
			if (string.IsNullOrEmpty(format)) {
				result = "{l1} {c} {s} {p}";
			}

			result = result.Replace("{l1}", this.Line1);
			result = result.Replace("{l2}", this.Line2);
			result = result.Replace("{l3}", this.Line3);
			result = result.Replace("{c}", this.CityName);
			result = this.State == null ? result.Replace("{s}", this.StateName) : result.Replace("{s}", this.State.ShortName);
			result = this.Country == null ? result.Replace("{ctry}", this.CountryName) : result.Replace("{ctry}", this.Country.ShortName);
			result = result.Replace("{cty}", this.County);

			result = result.Replace("{p}", this.PostalCodeName);

			if (this.GeoLocation != null) {
				result = result.Replace("{ls}", this.GeoLocation.Latitude.ToString());
				result = result.Replace("{lo}", this.GeoLocation.Longitude.ToString());
				result = result.Replace("{gps}", this.GeoLocation.ToString());
			}
			else {
				result = result.Replace("{ls}", string.Empty);
				result = result.Replace("{lo}", string.Empty);
				result = result.Replace("{gps}", string.Empty);
			}

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