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
	}
}
}