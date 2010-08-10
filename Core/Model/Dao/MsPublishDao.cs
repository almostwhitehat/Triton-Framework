using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using System.IO;
using Common.Logging;
using Triton.Controller.Publish;


namespace Triton.Model.Dao
{
	public class MsPublishDao : MsDaoBase, IPublishDao 
	{
		private string name = "IPublishDao";
		private const string TABLE_NAME = "PUBLISH_INFO";

		#region PublishDAO Members

		public PublishedPageCollection GetPublishInfo()
		{
			PublishedPageCollection result;
			try{

				SqlConnection conn = (SqlConnection)GetConnection();
				SqlCommand cmd = new SqlCommand();

				cmd.Connection = conn;
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "select * from Publish_Info;";					// 0

				//  build data adapter object
				SqlDataAdapter adapter = new SqlDataAdapter();

				adapter.SelectCommand = cmd;

				//  build and fill dataset
				DataSet ds = new DataSet();
				adapter.Fill(ds);

				StringWriter sw = new StringWriter();
				ds.Tables[0].WriteXml(sw);
				string xml = sw.ToString();
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);
				result = new PublishedPageCollection(doc);
			}
			catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Warn(
					errorMessage => errorMessage("Unable to load Published Page Info."), ex);

				result = new PublishedPageCollection(new XmlDocument());
			}
			return result;
		}

		public void SavePublishInfo(PublishedPageCollection publishInfo)
		{

			try {
				SqlConnection conn = (SqlConnection)GetConnection();
				SqlCommand cmd = new SqlCommand();
				SqlCommand cmdInsert = new SqlCommand();
                
				cmd.Connection = conn;

				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "delete from " + TABLE_NAME;

				conn.Open();
				conn.BeginTransaction();
				//  build data adapter object
				if ( cmd.ExecuteNonQuery() >= 0 ) {

					foreach (PublishRecord var in publishInfo) {
						// save the data
						StringBuilder sql = new StringBuilder();
						sql.AppendFormat("Insert into {0} ",TABLE_NAME);
						sql.AppendFormat("({0},{1},{2},{3},{4},{5},{6},{7},{8},{9}) ","Id","StartStateId","PublishedStateId","Event","PublishedPath","LastPublishedTime","Hits","Publishing","PublisherName");
						sql.AppendFormat("values('{0}',{1},{2},'{3}','{4}',{5},'{6}',{7},{8},'{9}' )",var.Key,
						                 var.StartState, var.PublishedState, var.Event, var.PublishedPath, var.LastPublished,
						                 var.HitCount, var.Publishing, var.PublisherName);

						cmdInsert.CommandText = "";
					}
				}
			}
			catch ( Exception ex) {
				LogManager.GetCurrentClassLogger().Error(
							message => message("Error occured in SavePublishInfo."), ex);
			}
		}

		#endregion

		public override string Name
		{
			get { return this.name; }
		}
	}
}