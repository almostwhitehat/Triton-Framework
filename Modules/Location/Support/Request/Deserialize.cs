using System;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller.Request;
using Triton.Location.Model;
using Triton.Location.Model.Dao;
using Triton.Model.Dao;

namespace Triton.Location.Support.Request {


/// <summary>
/// Helper class to take parameters from the request and generate objects to save/update.
/// For use with Triton.Location module.
/// </summary>
public static class Deserialize
{

	/// <summary>
	/// Creates a new <c>PersistedAddress</c> from the request.
	/// </summary>
	/// <param name="request">Request to create the PersistedAddress from.</param>
	/// <returns>A populated PersistedAddress object.</returns>
	public static PersistedAddress CreatePersistedAddress(MvcRequest request)
	{
		return new PersistedAddress() {
			//TODO {INIT_PARAMS}
		};
	}

	public static void Populate(
		MvcRequest request,
		PersistedAddress address)
	{

		if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.ID])) {
			long id;
			if (long.TryParse(request[ParameterNames.Address.Field.ID], out id)) {
				address.Id = id;
			}
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.LINE1])) {
			address.Line1 = request[ParameterNames.Address.Field.LINE1];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.LINE2])) {
			address.Line2 = request[ParameterNames.Address.Field.LINE2];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.LINE3])) {
			address.Line3 = request[ParameterNames.Address.Field.LINE3];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.CITY])) {
			address.CityName = request[ParameterNames.Address.Field.CITY];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.STATE])) {
			address.StateName = request[ParameterNames.Address.Field.STATE];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.POSTALCODE])) {
			address.PostalCodeName = request[ParameterNames.Address.Field.POSTALCODE];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.COUNTY])) {
			address.County = request[ParameterNames.Address.Field.COUNTY];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.COUNTRY])) {
			address.CountryName = request[ParameterNames.Address.Field.COUNTRY];
		}

		address.State = null;
        if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.STATE_ID]))
        {
            int stateId;
            if(int.TryParse(request[ParameterNames.Address.Field.STATE_ID], out stateId))
            {
                address.State = States.Instance[stateId];
	            address.StateName = address.State.Code;
            }
        }
		address.Country = null;
        if (!string.IsNullOrEmpty(request[ParameterNames.Address.Field.COUNTRY_ID]))
        {
            int countryId;
            if (int.TryParse(request[ParameterNames.Address.Field.COUNTRY_ID], out countryId))
            {
                address.Country = Countries.Instance[countryId];
	            address.CountryName = address.Country.Code;
            }
        }

		// PersistedAddress Address Request Parameters
		if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.ID])) {
			long id;
			if (long.TryParse(request[ParameterNames.PersistedAddress.Field.ID], out id)) {
				address.Id = id;
			}
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.LINE1])) {
			address.Line1 = request[ParameterNames.PersistedAddress.Field.LINE1];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.LINE2])) {
			address.Line2 = request[ParameterNames.PersistedAddress.Field.LINE2];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.LINE3])) {
			address.Line3 = request[ParameterNames.PersistedAddress.Field.LINE3];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.CITY])) {
			address.CityName = request[ParameterNames.PersistedAddress.Field.CITY];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.STATE])) {
			address.StateName = request[ParameterNames.PersistedAddress.Field.STATE];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.POSTALCODE])) {
			address.PostalCodeName = request[ParameterNames.PersistedAddress.Field.POSTALCODE];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.COUNTY])) {
			address.County = request[ParameterNames.PersistedAddress.Field.COUNTY];
		}

		if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.COUNTRY])) {
			address.CountryName = request[ParameterNames.PersistedAddress.Field.COUNTRY];
		}

        if (!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.STATE_ID]))
        {
            int stateId;
            if (int.TryParse(request[ParameterNames.PersistedAddress.Field.STATE_ID], out stateId))
            {
                address.State = States.Instance[stateId];
				address.StateName = address.State.ShortName;
			}
        }

        if(!string.IsNullOrEmpty(request[ParameterNames.PersistedAddress.Field.COUNTRY_ID]))
        {
            int countryId;
            if(int.TryParse(request[ParameterNames.PersistedAddress.Field.COUNTRY_ID], out countryId))
            {
                address.Country = Countries.Instance[countryId];
				address.CountryName = address.Country.ShortName;
			}
        }
	}
}
}