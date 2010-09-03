using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using Common.Logging;
using Triton.Controller.Publish;
using Triton.Utilities.Db;

namespace Triton.Model.Dao {


/// <summary>
/// <b>MsPublishDao</b> implements the <c>IPublishDao</c> for a SQLServer database.
/// It manages database interactions for the page publishing system.
/// </summary>
public class MsPublishDao : IPublishDao
{
	/// <summary>
	/// The name of the configuration setting within controllerSettings/publishing/settings for the
	/// name of the database connection.
	/// </summary>
	private const string				CONNECTION_SETTING	= "publishDaoConnection";
	/// <summary>
	/// The name of the table the page publish information is stored to/retrieved from.
	/// </summary>
	private const string				TABLE_NAME			= "publish_info";


	/// <summary>
	/// Reads the published page data from the database.
	/// </summary>
	/// <remarks>
	/// GetPublishInfo returns the published page data in an <c>XmlDocument</c> in the
	/// following format:
	/// <pre>
	///		<PublishedPages>
	///			<Page startState="" publishedState="" key="" event="" path="" lastPublished="" hits="" />
	///			...
	///		</PublishedPages>
	/// </pre>
	/// </remarks>
	/// <returns>An <b>XmlDocument</b> containing the published page data
	///			retrieved from the database.</returns>
	public PublishedPageCollection GetPublishInfo()
	{
		SqlConnection	conn	= null;
		SqlCommand		cmd		= null;
		SqlDataReader	dr		= null;
		XmlDocument		xml		= new XmlDocument();
//#if (PUBLISH_TRACE)
		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();
//#endif

		try {
					//  set up the connection and command
//			conn = (SqlConnection)ConnectionManager.GetConnection("PublishDAO", connType);
			PublishConfigSection config = ConfigurationManager.GetSection(
					"controllerSettings/publishing") as PublishConfigSection;
			string connName = config.Settings[CONNECTION_SETTING].Value;
			conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connName].ConnectionString);

			cmd = new SqlCommand();
			cmd.Connection = conn;
			conn.Open();
			cmd.CommandText = string.Format("select start_state_id, published_state_id, [key],"
					+ " event, published_path, last_published_time, hits, server from {0} (nolock)"
					+ " where server = '{1}'", TABLE_NAME, Environment.MachineName);
					//  execute the command
			dr = cmd.ExecuteReader();

					//  create the outer "PublishedPages" node
			XmlElement publishedPagesNode = xml.CreateElement("PublishedPages");
			xml.AppendChild(publishedPagesNode);

					//  read each page record and and create a "Page" node for each
			while (dr.Read()) {
				XmlElement pg = xml.CreateElement("Page");

				XmlAttribute attr = xml.CreateAttribute("startState");
				attr.Value = dr["start_state_id"].ToString();
				pg.Attributes.Append(attr);

				attr = xml.CreateAttribute("publishedState");
				attr.Value = dr["published_state_id"].ToString();
				pg.Attributes.Append(attr);

				attr = xml.CreateAttribute("event");
				attr.Value = dr["event"].ToString();
				pg.Attributes.Append(attr);

				attr = xml.CreateAttribute("key");
				attr.Value = dr["key"].ToString();
				pg.Attributes.Append(attr);

				attr = xml.CreateAttribute("path");
				attr.Value = dr["published_path"].ToString();
				pg.Attributes.Append(attr);

				attr = xml.CreateAttribute("lastPublished");
				attr.Value = dr["last_published_time"].ToString();
				pg.Attributes.Append(attr);

				attr = xml.CreateAttribute("hits");
				attr.Value = dr["hits"].ToString();
				pg.Attributes.Append(attr);

				publishedPagesNode.AppendChild(pg);
			}

		} catch (Exception e) {
			LogManager.GetCurrentClassLogger().Error("GetPublishInfo", e);

		} finally {
//#if (PUBLISH_TRACE)
			stopWatch.Stop();
			LogManager.GetCurrentClassLogger().Trace(string.Format("GetPublishInfo time (for {0}) = {1}.", Environment.MachineName, stopWatch.Elapsed));
//#endif
					//  close everything
			DbUtilities.Close(conn, cmd, dr);
		}

		return new PublishedPageCollection(xml);
	}


	/// <summary>
	/// Saves the published page collection, represented as XML, to the database.
	/// </summary>
	/// <param name="doc">An XML document containing the published page collection
	///			to save to the database.</param>
	public void SavePublishInfo(
		PublishedPageCollection	pageInfo)
	{
		SqlConnection	conn	= null;
		SqlCommand		cmd		= null;
		SqlTransaction	trans	= null;
		XmlDocument		doc		= pageInfo.ToXml();

//#if (PUBLISH_TRACE)
		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();
//#endif
		try {
//			conn = (SqlConnection)ConnectionManager.GetConnection("PublishDAO", connType);
			PublishConfigSection config = ConfigurationManager.GetSection(
					"controllerSettings/publishing") as PublishConfigSection;
			string connName = config.Settings[CONNECTION_SETTING].Value;
			conn = new SqlConnection(ConfigurationManager.ConnectionStrings[connName].ConnectionString);

			cmd = new SqlCommand();
			cmd.Connection = conn;
			conn.Open();

//#if (PUBLISH_TRACE)
			cmd.CommandText = string.Format("select count(1) from {0} where server = '{1}'",
					TABLE_NAME, Environment.MachineName);
			int beforeCnt = (int)cmd.ExecuteScalar();
//#endif
					//  set up the transaction
			trans = conn.BeginTransaction("SavePublishInfo");
			cmd.Transaction = trans;

					//  ======  remove entries for this server from the publish table  =====
			cmd.CommandText = string.Format("delete from {0} where server = '{1}'", TABLE_NAME, Environment.MachineName);
			int rows = cmd.ExecuteNonQuery();

					//  get the list of pages to be written
			XmlNodeList pageNodes = doc.DocumentElement.SelectNodes("Page");

					//  don't bother preparing the command if there is nothing
					//  to save
			if (pageNodes.Count > 0) {
				cmd.CommandText = "insert into " + TABLE_NAME
						+ " (start_state_id, published_state_id, [key], event, published_path, last_published_time, hits, server)"
						+ " values (@start_state_id, @published_state_id, @key, @event, @published_path, @last_published_time, @hits, @server)";
				cmd.Parameters.Add("@start_state_id", SqlDbType.Int);
				cmd.Parameters.Add("@published_state_id", SqlDbType.Int);
						// TODO: find better way than hard-coded field lengths
				cmd.Parameters.Add("@key", SqlDbType.NVarChar, 200);
				cmd.Parameters.Add("@event", SqlDbType.NVarChar, 200);
				cmd.Parameters.Add("@published_path", SqlDbType.NVarChar, 400);
				cmd.Parameters.Add("@last_published_time", SqlDbType.DateTime);
				cmd.Parameters.Add("@hits", SqlDbType.Int);
				cmd.Parameters.Add("@server", SqlDbType.NVarChar, 30);

				cmd.Prepare();

						//  server doesn't change per page, so set before we loop
				cmd.Parameters["@server"].Value = Environment.MachineName;

						//  write each published page record to the DB
				foreach (XmlNode page in pageNodes) {
					try {
						cmd.Parameters["@start_state_id"].Value = int.Parse(page.Attributes["startState"].Value);
						cmd.Parameters["@published_state_id"].Value = int.Parse(page.Attributes["publishedState"].Value);
						cmd.Parameters["@key"].Value = page.Attributes["key"].Value;
						cmd.Parameters["@event"].Value = page.Attributes["event"].Value;
						cmd.Parameters["@published_path"].Value = page.Attributes["path"].Value;
						cmd.Parameters["@last_published_time"].Value = page.Attributes["lastPublished"].Value;
						cmd.Parameters["@hits"].Value = int.Parse(page.Attributes["hits"].Value);

						cmd.ExecuteNonQuery();
					} catch (Exception e) {
						LogManager.GetCurrentClassLogger().Error(string.Format(
								"SavePublishInfo - error saving page key='{0}' path='{1}' event='{2}': ",
								page.Attributes["key"].Value,
								page.Attributes["path"].Value,
								page.Attributes["event"].Value), e);
					}
				}
			}

					//  if we got here everything went OK, so commit the changes
			trans.Commit();
			cmd.Transaction = null;

//#if (PUBLISH_TRACE)
			cmd.CommandText = string.Format("select count(1) from {0} where server = '{1}'", TABLE_NAME, Environment.MachineName);
			int afterCnt = (int)cmd.ExecuteScalar();
			LogManager.GetCurrentClassLogger().Trace(string.Format("SavePublishInfo - {0}:  before: {1}, after {2}.", Environment.MachineName, beforeCnt, afterCnt));
//#endif

		} catch (Exception e) {
			if (trans != null) {
				trans.Rollback();
			}
			LogManager.GetCurrentClassLogger().Error("SavePublishInfo", e);

		} finally {
//#if (PUBLISH_TRACE)
			stopWatch.Stop();
			LogManager.GetCurrentClassLogger().Trace(string.Format("SavePublishInfo time (for {0}) = {1}.", Environment.MachineName, stopWatch.Elapsed));
//#endif
					//  close everything
			DbUtilities.Close(conn, cmd, null);
		}
	}
}
}
