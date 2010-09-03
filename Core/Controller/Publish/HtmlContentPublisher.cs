using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Common.Logging;
using Triton.Controller.Config;
using Triton.Controller.StateMachine;
using Triton.Support;
using Triton.Utilities;

namespace Triton.Controller.Publish {

#region History

// History:
//  6/2/2009	KP	Changed the logging to Common.logging
//  09/29/2009	KP	Renamed logging methods to use GetCurrentClassLogger method
//  9/3/2010	SD	Added getting of published page expiration time from config settings.

#endregion

/// <summary>
/// </summary>
///	<author>Scott Dyke</author>
public class HtmlContentPublisher : ContentPublisher
{
	/// <summary>
	/// The name of the configuration setting within controllerSettings/publishing/settings for the
	/// expiration time (in minutes) of published pages.
	/// </summary>
	private const string EXPIRATION_SETTING		= "pageExpireTime";

	private const int DEFAULT_MAX_PATH_LEN		= 259;
	private const int DEFAULT_PAGE_EXPIRE_TIME	= 120;

	/// <summary>
	/// The request parameters to exclude from the publisher key.
	/// </summary>
	private const string PARAMS_TO_IGNORE = "st,e,dyn,repub";

	//  the marker appended to the published file so that it can be
	//  identified as a publishied page.
	private const string PUBLISH_MARKER = "<!-- P -->";

	/// <summary>
	/// The path to the root of the application.
	/// </summary>
	private readonly string basePath;

	private readonly MatchEvaluator	fileNameEvaluator;
	private readonly Regex			fileNameRegEx;
	private readonly int			maxPathLen			= DEFAULT_MAX_PATH_LEN;
	private readonly int			pageExpireTime		= DEFAULT_PAGE_EXPIRE_TIME;

	/// <summary>
	/// The relative (to basePath) path to the publish directory.
	/// </summary>
	private readonly string publishPath;


	public HtmlContentPublisher()
	{
				//  get the regular expression from sites.config that identifies character allowed
				//  in a published file name and build a Regex object for it
		string regExStr = SitesConfig.GetInstance().GetValue("/siteConfiguration/general/fileNameRegEx");
		if (regExStr != null) {
					//  we assume the regex defined in the config file is for valid characters, so
					//  we want to remove anything that's NOT a valid character
			if ((regExStr[0] == '[') && (regExStr[1] != '^')) {
				regExStr = regExStr.Insert(1, "^");
			}
			this.fileNameRegEx = new Regex(regExStr, RegexOptions.Compiled);
		}

				// Set the match evaluator delegate.
		this.fileNameEvaluator = this.GetUnicodeSequence;

				//	Get the maximum length of file path/name from sites.config.
		try {
			this.maxPathLen = int.Parse(SitesConfig.GetInstance().GetValue("/siteConfiguration/general/maxPathLength"));
		} catch (Exception ex) {
			LogManager.GetCurrentClassLogger().Error(
				errorMessage => errorMessage("PagePublisher : maxPathLength not found in sites.config "), ex);
		}

		try {
			PublishConfigSection config = ConfigurationManager.GetSection(
					"controllerSettings/publishing") as PublishConfigSection;
			string expireTimeStr = config.Settings[EXPIRATION_SETTING].Value;
			pageExpireTime = int.Parse(expireTimeStr);
		} catch {
			// TODO: log it
		}

		this.basePath = ConfigurationSettings.AppSettings["rootPath"];
		this.publishPath = SitesConfig.GetInstance().GetValue("/siteConfiguration/general/publishPath");
	}


	#region ContentPublisher Members

	/// <summary>
	/// Gets the name of the Publisher.
	/// </summary>
	public string Name
	{
		get {
// TODO: set this when loading from config?
			return "html";
		}
	}


	/// <summary>
	/// Publishes the content contained in the publishParam.
	/// </summary>
	/// <param name="publishParam">Contains the content to be published.</param>
	/// <param name="context">The Context of the request the content is being published for.</param>
	/// <param name="publisher">The publisher </param>
	/// <returns></returns>
	public virtual string Publish(
		object publishParam,
		TransitionContext context,
		Publisher publisher)
	{
		string content = publishParam as string;
		string key = context.PublishKey;

		LogManager.GetCurrentClassLogger().Debug(
			traceMessage => traceMessage("PagePublisher.Publish page {0}: start page = {1}, event = {2}",
			                                           context.EndState.Id,
			                                           context.StartState.Id,
			                                           context.StartEvent));

		PublishRecord pubRec = publisher.GetPublishRecord(key, context, true);

		try {
			//  make sure some other thread is not already publishing the page
			if (!pubRec.Publishing) {
				//  flag the page as in the process of being published
				pubRec.Publishing = true;
				//  get the key for the PublishRecord
				string filePrefix = pubRec.Key;
				//  strip off the state ID
				//  (should find a better way to do this so it's not so tightly coupled with
				//  what MakeKey's implementation is
				filePrefix = filePrefix.Substring(filePrefix.IndexOf('_') + 1);

				// replace special characters with their unicode representations.
				filePrefix = this.fileNameRegEx.Replace(filePrefix, this.fileNameEvaluator);

				//  get the publish state -- the one we are publishing
				PublishableState publishState = (PublishableState) context.EndState;

//				try {
//					content = publishState.GetPublishContent(context);
//				} catch (Exception e) {
//					Logger.GetLogger(LOGGER).ReportInnerExceptions = true;
//					Logger.GetLogger(LOGGER).Error("PagePublisher.Publish: ", e);
//					throw e;
//				}

				string fileName = publishState.BaseFileName;
				string section = publishState.Section;

				//  build the path to the directory to publish to
				string targetPath = string.Format(@"{0}{1}/{2}", this.publishPath, publishState.Site, section);
				//  make sure the directory exists
				IO.CreateDirectory(this.basePath + targetPath);

				//	if the length of full path exceeds the maximum, 
				//	use hash code of filePrefix instead of filePrefix to construct tagetPath
				string tmpFullPath = string.Format(@"{0}{1}/{2}_{3}.html",
				                                   this.basePath,
				                                   targetPath,
				                                   filePrefix,
				                                   fileName);
				if (tmpFullPath.Length > this.maxPathLen) {
					filePrefix = filePrefix.GetHashCode().ToString();
				}
				//  add the file name to the path
				targetPath = string.Format(@"{0}/{1}_{2}", targetPath, filePrefix, fileName);
				string path = this.basePath + targetPath;

				StreamWriter writer = null;
				try {
					writer = new StreamWriter(path.ToLower(), false, Encoding.UTF8);

					//  write the content received from Execute to the publish file
					writer.Write(content + Environment.NewLine + PUBLISH_MARKER);
				} catch (Exception e) {
					LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("PagePublisher.Publish write: "), e);
					throw;
				} finally {
					if (writer != null) {
						writer.Close();
					}
				}

				//  make sure the relative URL starts with "/"
				if (!targetPath.StartsWith("/")) {
					targetPath = "/" + targetPath;
				}

				pubRec.PublishedPath = targetPath;
				pubRec.LastPublished = DateTime.Now;
			}
		} catch (Exception e) {
			LogManager.GetCurrentClassLogger().Error(
				errorMessage => errorMessage("PagePublisher.Publish: "), e);
			content = null;
		} finally {
			//  we never want to leave the page in a state of publishing
			pubRec.Publishing = false;
		}

		return content;
	}


	/// <summary>
	/// Gets the publish key for the given context.
	/// </summary>
	/// <param name="context">The context to get the publish key for.</param>
	/// <returns>The publish key for the given context.</returns>
	public virtual string GetPublishKey(
		TransitionContext context)
	{
		StringBuilder keyParams = new StringBuilder();
		PublishableState state = context.StartState as PublishableState;

		//MvcTimer t = new MvcTimer();
		//t.Start();
		ArrayList parms = new ArrayList();
		parms.AddRange(context.Request.Params.AllKeys);

				//  build combined list of parameters to always exclude from the key
				//  + any defined in the state
		string excludeParams = (state.PublishExcludeParams == null)
		                       	? PARAMS_TO_IGNORE
		                       	: PARAMS_TO_IGNORE + "," + state.PublishExcludeParams;
				//  make the list into an array, and remove them from the list to key parameters
		string[] excludes = excludeParams.Split(',');
		foreach (string p in excludes) {
			parms.Remove(p);
		}

				// sort the params in case different calls have different order
		parms.Sort();

		foreach (string parm in parms) {
			if (context.Request[parm] != null) {
				keyParams.Append(context.Request[parm] + "_");
			}
		}

				//  remove trailing "_"
		if (keyParams.Length > 1) {
			keyParams.Remove(keyParams.Length - 1, 1);
		}

		string key = string.Format("{0}_{1}_{2}", state.Id, context.StartEvent, keyParams);
		//  remove any invalid characters from the key
		//		fileNameRegEx.Replace(key, "");
		//t.Stop();
		//Logger.GetLogger(LOGGER).Config("MakeKey time: " + t.Time + " seconds");

		return key;
	}


	/// <summary>
	/// Determines if the published content referenced by the given PublishRecord is expired.
	/// </summary>
	/// <param name="publishRecord">The PublishRecord to determine the expiration of.</param>
	/// <returns><b>True</b> if the given PublishRecord is expired, <b>false</b> if not.</returns>
	public virtual bool IsExpired(
		PublishRecord publishRecord)
	{

		return ((!publishRecord.LastPublished.HasValue)
				|| (publishRecord.LastPublished.Value.AddMinutes(pageExpireTime) < DateTime.Now));
	}


	public virtual void Dispose()
	{
	}

	#endregion


	/// <summary>
	/// This is the delegate function that is called each time a regular expression match
	/// is found during a Replace operation.
	/// </summary>
	/// <param name="match">The <c>Match</c> resulting from a single regular expression match during a Replace.</param>
	/// <returns>A <c>string</c> consisting of unicodes (represented by 2-digit fixed-length hex numbers) of the input value.</returns>
	internal string GetUnicodeSequence(
		Match match)
	{
		UnicodeEncoding unicode = new UnicodeEncoding();
		Byte[] bytes = unicode.GetBytes(match.Value);
		StringBuilder result = new StringBuilder();

		foreach (Byte aByte in bytes) {
			result.Append(aByte.ToString("X2"));
		}

		return result.ToString();
	}
}
}