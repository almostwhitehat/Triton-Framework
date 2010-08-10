

namespace web.security.membership.model.dao
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using web.model;
    using web.model.dao;
    using web.support.logging;

    /// <summary>
    /// 
    /// </summary>
    public class AddressFilter
    {

        private Logger logger = Logger.GetLogger("DaoLogger");

        protected List<int> _memberaddressIds = new List<int>();
        protected int _memberaddresstypeid = int.MinValue;
        protected Guid _accountid = Guid.Empty;
        protected string _addressline1 = String.Empty;
        protected string _addressline2 = String.Empty;
        protected string _addressline3 = String.Empty;
        protected string _addresscity = String.Empty;
        protected string _addressstate = String.Empty;
        protected string _addresspostalcode = String.Empty;
        protected string _addresscountry = String.Empty;
        protected DateTime? _createdate = null;
        protected string _createdby = String.Empty;
        protected DateTime? _updatedate = null;
        protected string _updatedby = String.Empty;
        private string _sortField = "Id";
        private DAOUtil.DBSortDirection _sortOrder = DAOUtil.DBSortDirection.ASC;
        private int _page = -1;
        private int _pageSize = 10;

        /// <summary>
        /// 
        /// </summary>
        public int MemberAddressTypeId {
            get { return _memberaddresstypeid; }
            set { _memberaddresstypeid = value;}
        }

        /// <summary>
        /// 
        /// </summary>
        public Guid AccountId {
            get { return _accountid;}
            set { _accountid = value;}
        }
        /// <summary>
        /// 
        /// </summary>
        public string AddressLine1
        {
            get { return _addressline1; }
            set { _addressline1 = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string AddressLine2
        {
            get { return _addressline2; }
            set { _addressline2 = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string AddressLine3
        {
            get { return _addressline3; }
            set { _addressline3 = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string AddressCity
        {
            get { return _addresscity; }
            set { _addresscity = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string AddressState
        {
            get { return _addressstate; }
            set { _addressstate = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string AddressPostalCode
        {
            get { return _addresspostalcode; }
            set { _addresspostalcode = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string AddressCountry
        {
            get { return _addresscountry; }
            set { _addresscountry = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreatedDate
        {
            get { return _createdate; }
            set { _createdate = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy
        {
            get { return _createdby; }
            set { _createdby = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UpdatedDate
        {
            get { return _updatedate; }
            set { _updatedate = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string UpdateBy
        {
            get { return _updatedby; }
            set { _updatedby = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Page
        {
            get { return _page; }
            set { _page = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<int> MemberaddressIds
        {
            get { return _memberaddressIds; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberaddressId"></param>
        public void AddMemberaddressId(int memberaddressId)
        {
            _memberaddressIds.Add(memberaddressId);
        }
        /// <summary>
        /// 
        /// </summary>
        public string SortFields
        {
            get { return _sortField; }
            set { _sortField = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DAOUtil.DBSortDirection SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        public void Fill(web.controller.request.WebRequest request)
        {
            if (request[MembershipConstants.PARAM_ADDRESS_ID] != null && request[MembershipConstants.PARAM_ADDRESS_ID].Length > 0)
            {
                string[] memberaddressids = request[MembershipConstants.PARAM_ADDRESS_ID].Split(',');
                foreach (string memberaddressId in memberaddressids)
                {
                    try
                    {
                        AddMemberaddressId(int.Parse(memberaddressId));
                    }
                    catch (FormatException e)
                    {
                        logger.Warning(string.Format("MemberaddressFilter.Fill: parse of 'memberaddressid' failed [{0}].", memberaddressId));
                    }
                }
            }
            // change request name to no underscores and casing
            if (request[MembershipConstants.PARAM_ADDRESS_LINE_1] != null && request[MembershipConstants.PARAM_ADDRESS_LINE_1].Length > 0)
            {
                try
                {
                    string addressline1 = request[MembershipConstants.PARAM_ADDRESS_LINE_1];
                    AddressLine1 = addressline1;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'ADDRESSLINE1' failed [{0}].", AddressLine1));
                }
            }

            if (request[MembershipConstants.PARAM_ADDRESS_TYPE_ID] != null && request[MembershipConstants.PARAM_ADDRESS_TYPE_ID].Length > 0)
            {
                try
                {
                    string addresstypeid = request[MembershipConstants.PARAM_ADDRESS_TYPE_ID];
                    MemberAddressTypeId = int.Parse(addresstypeid);


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'ADDRESSLINE1' failed [{0}].", AddressLine1));
                }
            }

            if (request[MembershipConstants.PARAM_UID] != null && request[MembershipConstants.PARAM_UID].Length > 0)
            {
                try
                {
                    
                    Guid accountid = new Guid(request[MembershipConstants.PARAM_UID]);
                    AccountId = accountid;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'ADDRESSLINE2' failed [{0}].", AddressLine2));
                }
            }
            if (request[MembershipConstants.PARAM_ADDRESS_LINE_2] != null && request[MembershipConstants.PARAM_ADDRESS_LINE_2].Length > 0)
            {
                try
                {
                    string addressline2 = request[MembershipConstants.PARAM_ADDRESS_LINE_2];
                    AddressLine2 = addressline2;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'ADDRESSLINE2' failed [{0}].", AddressLine2));
                }
            }
            if (request[MembershipConstants.PARAM_ADDRESS_LINE_3] != null && request[MembershipConstants.PARAM_ADDRESS_LINE_3].Length > 0)
            {
                try
                {
                    string addressline3 = request[MembershipConstants.PARAM_ADDRESS_LINE_3];
                    AddressLine3 = addressline3;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'ADDRESSLINE3' failed [{0}].", AddressLine3));
                }
            }
            if (request[MembershipConstants.PARAM_ADDRESS_CITY] != null && request[MembershipConstants.PARAM_ADDRESS_CITY].Length > 0)
            {
                try
                {
                    string addresscity = request[MembershipConstants.PARAM_ADDRESS_CITY];
                    AddressCity = addresscity;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'ADDRESSCITY' failed [{0}].", AddressCity));
                }
            }
            if (request[MembershipConstants.PARAM_ADDRESS_STATE] != null && request[MembershipConstants.PARAM_ADDRESS_STATE].Length > 0)
            {
                try
                {
                    string addressstate = request[MembershipConstants.PARAM_ADDRESS_STATE];
                    AddressState = addressstate;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'ADDRESSSTATE' failed [{0}].", AddressPostalCode));
                }
            }
            if (request[MembershipConstants.PARAM_ADDRESS_POSTALCODE] != null && request[MembershipConstants.PARAM_ADDRESS_POSTALCODE].Length > 0)
            {
                try
                {
                    string addresspostalcode = request[MembershipConstants.PARAM_ADDRESS_POSTALCODE];
                    AddressPostalCode = addresspostalcode;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'ADDRESSPOSTALCODE' failed [{0}].", AddressPostalCode));
                }
            }
            if (request[MembershipConstants.PARAM_ADDRESS_COUNTRY] != null && request[MembershipConstants.PARAM_ADDRESS_COUNTRY].Length > 0)
            {
                try
                {
                    string addresscountry = request[MembershipConstants.PARAM_ADDRESS_COUNTRY];
                    AddressCountry = addresscountry;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'ADDRESSCOUNTRY' failed [{0}].", AddressCountry));
                }
            }
            if (request["createdate"] != null && request["createdate"].Length > 0)
            {
                try
                {
                    string createdate = request["createdate"];
                    CreatedDate = DateTime.Parse(createdate);


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'CREATEDATE' failed [{0}].", CreatedDate));
                }
            }
            if (request["createdby"] != null && request["createdby"].Length > 0)
            {
                try
                {
                    string createdby = request["createdby"];
                    CreatedBy = createdby;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'CREATEDBY' failed [{0}].", CreatedBy));
                }
            }
            if (request["updatedate"] != null && request["updatedate"].Length > 0)
            {
                try
                {
                    string updatedate = request["updatedate"];
                    UpdatedDate = DateTime.Parse(updatedate);


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'UPDATEDATE' failed [{0}].", UpdatedDate));
                }
            }
            if (request["updatedby"] != null && request["updatedby"].Length > 0)
            {
                try
                {
                    string updatedby = request["updatedby"];
                    UpdateBy = updatedby;


                }
                catch (Exception e)
                {
                    logger.Warning(string.Format("MemberaddressFilter.Fill: Setting Property 'UPDATEDBY' failed [{0}].", UpdateBy));
                }
            }
        }

    }
}

