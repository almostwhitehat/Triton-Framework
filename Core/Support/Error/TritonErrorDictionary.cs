using System;
using System.Collections;
using System.Configuration;
using System.Xml;
using Common.Logging;
using Triton.Controller.Config;
using Triton.Utilities.Configuration;

namespace Triton.Support.Error
{
	public class TritonErrorDictionary : ErrorDictionary
	{
		//private const string DICIONARY_NAME = "TritonErrorDictionary";
		private readonly ILog logger = LogManager.GetCurrentClassLogger();

		private Hashtable errorMessages;
		private string site;


		private TritonErrorDictionary(
			String site)
		{
			XmlDocument xmlDoc;
			XmlNode currError;
			XmlNode rootNode;

			this.logger.Info("TritonErrorDictionary - starting load.");
			try {
				this.errorMessages = new Hashtable();
				xmlDoc = new XmlDocument();

				this.site = site;
				Name = this.site;

				//  get the config section for the current site
				XmlConfiguration siteConfig = SitesConfig.GetInstance().GetConfig("sites", this.site);
				String errorDictionaryConfigPath = siteConfig.GetValue("//validationErrorDictionaryPath");

				xmlDoc.Load(ConfigurationManager.AppSettings["rootPath"] + errorDictionaryConfigPath);
				rootNode = xmlDoc.DocumentElement;
				if (rootNode != null) {
					XmlNodeList errors = rootNode.SelectNodes("//Error");

					if (errors != null) {
						IEnumerator iter = errors.GetEnumerator();

						while (iter.MoveNext()) {
							currError = (XmlNode) iter.Current;
							try {
								long currId = Int64.Parse(currError.Attributes.GetNamedItem("id").Value);
								String currFormField = "";
								String currMsg = currError.InnerText;
								Error.ErrorType currType =
									(Error.ErrorType) Enum.Parse(typeof (Error.ErrorType),
									                             currError.Attributes["type"].Value.ToUpper(),
									                             true);

								if (currError.Attributes.GetNamedItem("formField") != null) {
									currFormField = currError.Attributes.GetNamedItem("formField").Value;
								}

								//  Add the new error message
								this.errorMessages.Add(currId, new Error(currId, currFormField, currMsg, currType));
							} catch (Exception ex) {
								this.logger.Error("Error when creating an error entry.", ex);
							}
						}
					} else {
						this.logger.Info("Instantiation: Error list is null");
					}
				} else {
					this.logger.Info("Instantiation: Root node in error dictionary not found.");
				}
			} catch (Exception ex) {
				this.logger.Info("Instantiation: ", ex);
			} finally {
				this.logger.Info("TritonErrorDictionary - loaded " + this.errorMessages.Count + " error messages.");
				this.logger.Info("TritonErrorDictionary - load complete.");
			}
		}


		public static TritonErrorDictionary GetDictionary(
			String site)
		{
			TritonErrorDictionary dict = (TritonErrorDictionary) DictionaryManager.GetDictionaryManager().GetDictionary(site);

			if (dict == null) {
				dict = new TritonErrorDictionary(site);
				DictionaryManager.GetDictionaryManager().AddDictionary(dict);
			}

			return dict;
		}


		public override Error GetError(
			long id)
		{
			if (!this.errorMessages.ContainsKey(id)) {
				NoSuchErrorException tmpException = new NoSuchErrorException(
					"No error exists in dicionary for ID " + id);
				this.logger.Error("GetError: ", tmpException);
				throw tmpException;
			}
			Error retError = new Error((Error) this.errorMessages[id]);

			return retError;
		}


		public void Reset()
		{
			if (this.errorMessages != null) {
				this.errorMessages.Clear();
				this.errorMessages = null;
				this.logger.Info("TritonErrorDictionary reset.");
			}
		}
	}
}