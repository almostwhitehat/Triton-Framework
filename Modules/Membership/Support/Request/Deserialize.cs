using System;
using System.Collections.Generic;
using Common.Logging;
using Triton.Controller.Request;
using Triton.Membership.Model;
using Triton.Membership.Model.Dao;
using Triton.Model.Dao;

namespace Triton.Membership.Support.Request
{
	/// <summary>
	/// Helper class to take parameters from the request and generate objects to save/update.
	/// For use with Triton.Membership module.
	/// </summary>
	public static class Deserialize
	{
		public static Account CreateAccount(MvcRequest request)
		{
			Account account = new Account
			                  {
			                  	Id = Guid.Empty,
			                  	CreateDate = DateTime.Now,
			                  	Person = new Person(),
			                  	Attributes = new Dictionary<AttributeType, string>(),
			                  	Usernames = new List<Username>()
			                  };
			return account;
		}


		public static Account Populate(MvcRequest request,
		                               Account account)
		{
			//if the username has been defined in the request
			#region DEPRCIATED
			if (!string.IsNullOrEmpty(request[ParameterNames.Account.USERNAME])) {
				// This will only update the first username in the collection,
				// needs to be updated to update any.
				if (account.Usernames.Count > 0) {
					if (account.Usernames[0] != null) {
						account.Usernames[0].Value = request[ParameterNames.Account.USERNAME];
					} else {
						account.Usernames[0] = new Username
						                       {
						                       	Value = request[ParameterNames.Account.USERNAME]
						                       };
					}
				} else {
					account.Usernames.Add(new Username
					                      {
					                      	Value = request[ParameterNames.Account.USERNAME]
					                      });
				}
			}
			#endregion

			//if the username has been defined in the request
			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Field.USERNAME])) {
				// This will only update the first username in the collection,
				// needs to be updated to update any.
				if (account.Usernames.Count > 0) {
					if (account.Usernames[0] != null) {
						account.Usernames[0].Value = request[ParameterNames.Account.Field.USERNAME];
					}
					else {
						account.Usernames[0] = new Username {
							Value = request[ParameterNames.Account.Field.USERNAME]
						};
					}
				}
				else {
					account.Usernames.Add(new Username {
						Value = request[ParameterNames.Account.Field.USERNAME]
					});
				}
			}

			if (account.Person != null) {
				Populate(request, account.Person);
			}

			#region DEPRECIATED
			if (!string.IsNullOrEmpty(request[ParameterNames.Account.PASSWORD])) {
				account.Password = request[ParameterNames.Account.PASSWORD];
			}
			#endregion

			if (!string.IsNullOrEmpty(request[ParameterNames.Account.Field.PASSWORD])) {
				account.Password = request[ParameterNames.Account.Field.PASSWORD];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.AccountStatus.CODE])) {
				AccountStatus accountStatus = DaoFactory.GetDao<IAccountStatusDao>().Get(request[ParameterNames.AccountStatus.CODE]);
				account.Status = accountStatus;
			}
	
			return account;
		}


		public static Person CreatePerson(MvcRequest request)
		{
			return Populate(request, new Person());
		}


		public static Person Populate(MvcRequest request,
		                              Person person)
		{
			if (request[ParameterNames.Person.EMAIL] != null) {
				person.Email = request[ParameterNames.Person.EMAIL];
			}

			if (request[ParameterNames.Person.PHONE] != null) {
				person.Phone = request[ParameterNames.Person.PHONE];
			}

			person.Name = person.Name == null ? CreateName(request) : Populate(request, person.Name);

			return person;
		}


		/// <summary>
		/// Creates a new <c>Name</c> from the request.
		/// </summary>
		/// <param name="request">Request to create the Name from.</param>
		/// <returns>A populated Name object.</returns>
		public static Name CreateName(MvcRequest request)
		{
			return Populate(request, new Name());
		}


		/// <summary>
		/// Populates the <c>Name</c> with the values from the request.
		/// </summary>
		/// <param name="request">Request to populate the Name from.</param>
		/// <param name="name">Name to populate</param>
		/// <returns>Populated from the request Name object.</returns>
		public static Name Populate(MvcRequest request,
		                            Name name)
		{
			if (request[ParameterNames.Person.FIRST_NAME] != null) {
				name.First = request[ParameterNames.Person.FIRST_NAME];
			}

			if (request[ParameterNames.Person.LAST_NAME] != null) {
				name.Last = request[ParameterNames.Person.LAST_NAME];
			}

			if (request[ParameterNames.Person.MIDDLE_NAME] != null) {
				name.Middle = request[ParameterNames.Person.MIDDLE_NAME];
			}

			if (request[ParameterNames.Person.SUFFIX_CODE] != null) {
				name.SuffixCode = request[ParameterNames.Person.SUFFIX_CODE];
			}

			if (request[ParameterNames.Person.PREFIX_CODE] != null) {
				name.PrefixCode = request[ParameterNames.Person.PREFIX_CODE];
			}

			if (!string.IsNullOrEmpty(request[ParameterNames.NameSuffix.ID])) {
				int suffixId;
				if (int.TryParse(request[ParameterNames.NameSuffix.ID], out suffixId)) {
					NameSuffix nameSuffix = DaoFactory.GetDao<INameSuffixDao>().Get(suffixId);

					if (nameSuffix == null) {
						LogManager.GetCurrentClassLogger().Error(error => error("No suffix found in the database with id: {0}", suffixId));
					} else {
						name.Suffix = nameSuffix;
					}
				}
			}

			return name;
		}


		/// <summary>
		/// Creates a new <c>Role</c> from the request.
		/// </summary>
		/// <param name="request">Request to create the Role from.</param>
		/// <returns>A populated Role object.</returns>
		public static Role CreateRole(MvcRequest request)
		{
			return Populate(request, new Role()
			                         {
			                         	Id = 0
			                         });
		}


		/// <summary>
		/// Populates the <c>Role</c> with the values from the request.
		/// </summary>
		/// <param name="request">Request to populate the Role from.</param>
		/// <param name="role">Role to populate</param>
		/// <returns>Populated from the request Role object.</returns>
		public static Role Populate(MvcRequest request,
		                            Role role)
		{
			if (request[ParameterNames.Role.CODE] != null) {
				role.Code = request[ParameterNames.Role.CODE];
			}

			if (request[ParameterNames.Role.DESCRIPTION] != null) {
				role.Description = request[ParameterNames.Role.DESCRIPTION];
			}

			return role;
		}


		/// <summary>
		/// Creates a new <c>AttributeType</c> from the request.
		/// </summary>
		/// <param name="request">Request to create the AttributeType from.</param>
		/// <returns>A populated AttributeType object.</returns>
		public static AttributeType CreateAttributeType(MvcRequest request)
		{
			return Populate(request, new AttributeType()
			                         {
			                         	Id = 0
			                         });
		}


		/// <summary>
		/// Populates the <c>AttributeType</c> with the values from the request.
		/// </summary>
		/// <param name="request">Request to populate the AttributeType from.</param>
		/// <param name="attributeType">AttributeType to populate</param>
		/// <returns>Populated from the request AttributeType object.</returns>
		public static AttributeType Populate(MvcRequest request,
		                                     AttributeType attributeType)
		{
			if (request[ParameterNames.AttributeType.CODE] != null) {
				attributeType.Code = request[ParameterNames.AttributeType.CODE];
			}

			if (request[ParameterNames.AttributeType.NAME] != null) {
				attributeType.Name = request[ParameterNames.AttributeType.NAME];
			}

			if (request[ParameterNames.AttributeType.DESCRIPTION] != null) {
				attributeType.Description = request[ParameterNames.AttributeType.DESCRIPTION];
			}

			if (request[ParameterNames.AttributeType.WEIGHT] != null) {
				float weight;
				attributeType.Weight = float.TryParse(request[ParameterNames.AttributeType.WEIGHT], out weight) ? weight : 0;
			}

			return attributeType;
		}
	}
}