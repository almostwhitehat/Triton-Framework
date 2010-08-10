using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using Common.Logging;
using Triton.Utilities;
using Triton.Utilities.Xml;

namespace Triton.Support.Error
{

	#region History

	// History:
	// 6/5/2009		KP	Changed the logging to Common.Logging.
	// 09/29/2009	KP	Changed the logging method to GetCurrentClassLogger

	#endregion

	/// <summary>
	/// <c>ErrorList</c> maintains a collection of <c>Error</c>s.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class ErrorList : BaseList, Xmlable
	{
		private readonly IErrorDictionary dictionary;
		private readonly Set errFormFields = new Set();
		private readonly Set errNumbers = new Set();
		private Error.ErrorType maxSeverity = Error.ErrorType.NOERROR;


		public ErrorList() {}


		public ErrorList(
			IErrorDictionary dict)
		{
			this.dictionary = dict;
		}


		public Error.ErrorType MaxSeverity
		{
			get { return this.maxSeverity; }
		}

		#region Xmlable Members

		public void fill(
			XmlNode element) {}


		/**
	 * Converts the <code>ErrorList</code> XML. 
	 *
	 * @return	the <code>ErrorList</code> as XML.
	 */


		public String toXML()
		{
			StringBuilder xml = new StringBuilder();

			xml.Append("<Errors>");

			IEnumerator iter = theList.GetEnumerator();
			while (iter.MoveNext()) {
				Error err = (Error) iter.Current;
				xml.Append(err.toXML());
			}

			xml.Append("</Errors>");

			return xml.ToString();
		}

		#endregion

		/**
	 * Appends the specified Error to the end of this list.  
	 *
	 * @param err	Error to be inserted.
	 *
	 * @return		
	 */


		public int Add(
			Error err)
		{
			this.maxSeverity = err.Type > this.maxSeverity ? err.Type : this.maxSeverity;
			this.errNumbers.Add(err.Id);

			if (err.FormField != null) {
				this.errFormFields.Add(err.FormField);
			}

			return base.Add(err);
		}


		public void AddItems(
			ErrorList list)
		{
			if (list != null) {
				try {
					for (int i = 0; i < list.Count; i++) {
						Error err = (Error) list[i];

						this.maxSeverity = err.Type > this.maxSeverity ? err.Type : this.maxSeverity;
						this.errNumbers.Add(err.Id);
						this.errFormFields.Add(err.FormField);
						base.Add(err);
					}
				} catch (Exception ex) {
					LogManager.GetCurrentClassLogger().Error(
						errorMessage => errorMessage("ErrorList: AddItems: "), ex);
				}
			}
		}


		/**
	 * Appends the Error for the given ID to the list.  
	 *
	 * @param id	ID of the Error to be inserted.
	 *
	 * @return		.
	 */


		public int Add(
			long id)
		{
			Error err = null;
			int results = -1;

			try {
				err = this.getError(id);
				this.maxSeverity = (err.Type > this.maxSeverity) ? err.Type : this.maxSeverity;
				this.errNumbers.Add(id);
				this.errFormFields.Add(err.FormField);
				results = base.Add(err);
			} catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("ErrorList: Add (id): "), ex);
			}
			return results;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int Add(
			long id,
			params String[] options)
		{
			Error err = null;
			int results = -1;

			try {
				err = this.getError(id);
				this.maxSeverity = (err.Type > this.maxSeverity) ? err.Type : this.maxSeverity;
				err.Message = String.Format(err.Message, options);
				this.errNumbers.Add(id);
				this.errFormFields.Add(err.FormField);
				results = base.Add(err);
			} catch (Exception ex) {
				LogManager.GetCurrentClassLogger().Error(
					errorMessage => errorMessage("ErrorList: Add (id, options[]): "), ex);
			}
			return results;
		}


		/* Commented out - this needs to change since the get and set methods changed
	/**
	 * Populates the <code>GroupList</code> from the given XML Element.
	 *
	 * @param element	the XML <code>Element</code> to populate the
	 *					<code>GroupList</code> from.
	 * /
	public void fill(
		XmlNode	element)
	{
		XmlNodeList	grpNodeList = element.SelectNodes("Error");

		IEnumerator iter = grpNodeList.GetEnumerator();		  
		while (iter.MoveNext()) {   
			XmlElement errEl = (XmlElement)iter.Current;
			Error err = new Error();
			err.fill(errEl);
			this.Add(err);
		}
	}
*/


		public bool hasError(long id)
		{
			return this.errNumbers.Contains(id);
		}


		public bool hasError(String formFieldName)
		{
			return this.errFormFields.Contains(formFieldName);
		}


		public bool hasError(long id, string formFieldName)
		{
			return this.getOccurrences(id, formFieldName) > 0;
		}

		public int getOccurrences(long id, string formFieldName)
		{
			Error[] errors = (Error[])this.theList.ToArray(typeof(Error));

			var x = from error in errors
					where error.Id == id && error.FormField == formFieldName
					select error;

			return x.ToArray().Length;
		}


		public Error getError(
			long id)
		{
			Error retErr = null;

			if (this.dictionary == null) {
				throw new NoDictionaryException("No dictionary defined for this ErrorList.");
			}

			retErr = this.dictionary.GetError(id);

			return retErr;
		}
	}
}