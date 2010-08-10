using System;
using Triton.Controller.Request;
using Triton.Utilities;

namespace Triton.Validator.Model.Rules
{

	#region History

	// History:

	#endregion

	/// <summary>
	/// <b>FileRule</b> is a leaf <b>IValidationRule</b> that validates
	/// a file by size or type.
	/// </summary>
	///	<author>Scott Dyke</author>
	public class FileRule : BaseLeafRule
	{
		/// <summary>
		/// The file size limitation.
		/// </summary>
		private long? fileSize = null;


		/// <summary>
		/// Gets or sets the file length limitation.
		/// </summary>
		public string FileSize
		{
			get { 
				return this.fileSize.HasValue ? this.fileSize.ToString() : null; 
			}
			set { 
				this.fileSize = long.Parse(value); 
			}
		}


		/// <summary>
		/// Gets or sets the acceptable file types.
		/// </summary>
		public string FileType { 
			get; 
			set; 
		}


		/// <summary>
		/// Evaluates the rule for the given request.  The <b>RegexRule</b> applies its
		/// regular expression pattern to its specified field in the given request.
		/// </summary>
		/// <param name="request">The <b>MvcRequest</b> to evaluate the rule for.</param>
		/// <returns>A <b>ValidationResult</b> indicating the result of evaluating the
		///			rule and any validation errors that occurred.</returns>
		public override ValidationResult Evaluate(
			MvcRequest request)
		{
			ValidationResult result = new ValidationResult { Passed = true };

			if (request.Files.Count == 0) {
				result.Passed = false;
				result.AddError(Field, ErrorId);
			} else {
				//  get the uploaded file reference
				MvcPostedFile theFile = request.Files[Field];

				if (this.FileType != null) {
					result.Passed = false;
					//  get the list of acceptable types
					string[] types = this.FileType.Split(',');
					//  get the extension of the file
					string ext = this.GetExtension(theFile.Name).ToLower();

					//  see if the file extension is in the acceptable list
					foreach (string t in types) {
						if (ext == t.ToLower()) {
							result.Passed = true;
							break;
						}
					}

					//  if the type check failed and an error ID was specified, add the error
					if (!result.Passed && (ErrorId > 0)) {
						result.AddError(Field, ErrorId);
					}
				}

				//  if a file size limit is specified, check the size
// TODO: should we check result.Passed and not do this if we've already failed?? 
				if (this.fileSize.HasValue) {
					if (theFile.Length > this.fileSize) {
						result.Passed = false;
						//  add the error message, if specified
						if (ErrorId > 0) {
							result.AddError(Field, ErrorId);
						}
					}
				}
			}

			return result;
		}


		/// <summary>
		/// Returns the file extension of the given file.
		/// </summary>
		/// <param name="filePath">The file name to get the extension for.</param>
		/// <returns>The file extension of the given file.</returns>
		private string GetExtension(
			string filePath)
		{
			string retVal = "";
			int pos = Math.Max(filePath.LastIndexOf('\\'), 0);

			pos = filePath.LastIndexOf('.', filePath.Length - 1, filePath.Length - pos);

			if (pos >= 0) {
				retVal = filePath.Substring(pos + 1);
			}

			return retVal;
		}
	}
}