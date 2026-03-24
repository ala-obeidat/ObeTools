using System.Collections.Generic;

namespace ObeTools.Model
{
	/// <summary>
	/// Excel Sheet Model.
	/// </summary>
	public class ExcelSheetModel
	{
		/// <summary>
		/// Excel sheet name
		/// </summary>
		public string SheetName { get; set; }

		/// <summary>
		/// Rows Data for each sheet
		/// </summary>
		public List<string[]> Data { get; set; }

		/// <summary>
		/// Title first row
		/// </summary>
		public List<string> Headers { get; set; }
	}
}
