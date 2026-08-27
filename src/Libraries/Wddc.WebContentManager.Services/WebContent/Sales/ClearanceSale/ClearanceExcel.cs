using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Wddc.WebContentManager.Services.WebContent.Sales.ClearanceSale
{
    /// <summary>
    /// One data row read from the uploaded workbook
    /// </summary>
    public class ClearanceParsedRow
    {
        public int RowNumber { get; set; }
        public string ItemNumber { get; set; }
        public string CategoryRaw { get; set; }
        public short? CategoryID { get; set; }
        public decimal? RegularPrice { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class ClearanceParseResult
    {
        public string SheetName { get; set; }
        public int HeaderRow { get; set; }
        public Dictionary<string, string> ColumnsFound { get; set; } = new Dictionary<string, string>();
        public bool HasSalePriceColumn { get; set; }
        public List<ClearanceParsedRow> Rows { get; set; } = new List<ClearanceParsedRow>();
        /// <summary>Fatal problem with the file itself (no header row found, etc.)</summary>
        public string FileError { get; set; }
    }

    /// <summary>
    /// Reads and writes the purchasers' clearance sale workbooks.
    /// Header names are matched case-insensitively and extra columns are ignored,
    /// so the existing "Clearance sale data" sheet imports without changes.
    /// </summary>
    public static class ClearanceExcel
    {
        public static ClearanceParseResult Parse(Stream stream)
        {
            var result = new ClearanceParseResult();

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault(ws => ws.Visibility == XLWorksheetVisibility.Visible && ws.RangeUsed() != null)
                ?? workbook.Worksheets.FirstOrDefault();
            if (worksheet == null || worksheet.RangeUsed() == null)
            {
                result.FileError = "The workbook contains no data.";
                return result;
            }
            result.SheetName = worksheet.Name;

            var used = worksheet.RangeUsed();
            var firstRow = used.FirstRow().RowNumber();
            var lastRow = used.LastRow().RowNumber();
            var firstCol = used.FirstColumn().ColumnNumber();
            var lastCol = used.LastColumn().ColumnNumber();

            // Locate the header row: the first row (within the first 15) containing an "ItemNumber" header
            int headerRow = 0;
            var columns = new Dictionary<string, int>(); // field -> column number
            for (int r = firstRow; r <= Math.Min(firstRow + 14, lastRow) && headerRow == 0; r++)
            {
                for (int c = firstCol; c <= lastCol; c++)
                {
                    if (MapHeader(CellText(worksheet.Cell(r, c))) == "ItemNumber")
                    {
                        headerRow = r;
                        break;
                    }
                }
            }
            if (headerRow == 0)
            {
                result.FileError = "Could not find a header row. The sheet needs a column named \"ItemNumber\".";
                return result;
            }
            result.HeaderRow = headerRow;

            for (int c = firstCol; c <= lastCol; c++)
            {
                var field = MapHeader(CellText(worksheet.Cell(headerRow, c)));
                if (field == null)
                    continue;
                // Prefer the first (or the more specific "CategoryID") match when two headers map to the same field
                if (!columns.ContainsKey(field))
                    columns[field] = c;
            }
            foreach (var kv in columns)
                result.ColumnsFound[kv.Key] = XLHelper.GetColumnLetterFromNumber(kv.Value);
            result.HasSalePriceColumn = columns.ContainsKey("SalePrice");

            for (int r = headerRow + 1; r <= lastRow; r++)
            {
                var itemNumber = GetString(worksheet, r, columns, "ItemNumber");
                var categoryRaw = GetString(worksheet, r, columns, "Category");
                var regularRaw = GetString(worksheet, r, columns, "RegularPrice");

                // Skip rows that are entirely empty
                if (string.IsNullOrWhiteSpace(itemNumber) && string.IsNullOrWhiteSpace(categoryRaw)
                    && string.IsNullOrWhiteSpace(regularRaw))
                    continue;

                var row = new ClearanceParsedRow
                {
                    RowNumber = r,
                    ItemNumber = (itemNumber ?? "").Trim(),
                    CategoryRaw = (categoryRaw ?? "").Trim(),
                };

                if (row.ItemNumber.Length == 0)
                    row.Errors.Add("Missing item number.");
                else if (row.ItemNumber.Length > 31)
                    row.Errors.Add("Item number is longer than 31 characters.");

                row.CategoryID = ParseCategory(row.CategoryRaw);
                if (row.CategoryID == null)
                    row.Errors.Add(string.IsNullOrWhiteSpace(row.CategoryRaw)
                        ? "Missing category (use 1 = Retail Clearance or 2 = Equipment Clearance)."
                        : $"Unrecognized category \"{row.CategoryRaw}\" (use 1 = Retail Clearance or 2 = Equipment Clearance).");
                else if (row.CategoryID == 3)
                    row.Errors.Add("Category 3 (Hard to Find Items) is managed by IT and cannot be imported here.");

                row.RegularPrice = ParsePrice(regularRaw, out var regularBad);
                if (regularBad)
                    row.Errors.Add($"Regular price \"{regularRaw}\" is not a valid amount.");
                else if (row.RegularPrice == null && columns.ContainsKey("RegularPrice"))
                    row.Warnings.Add("No regular price — the website has no \"was\" price to show.");
                else if (row.RegularPrice < 0)
                    row.Errors.Add("Regular price cannot be negative.");

                result.Rows.Add(row);
            }

            return result;
        }

        /// <summary>
        /// Builds the export workbook. Exported files re-import directly, so this doubles as the import template.
        /// "Sale Price (live)" is the current catalog price — informational only, ignored on import.
        /// </summary>
        public static byte[] BuildWorkbook(IEnumerable<ClearanceGridItem> items)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Clearance Sale");

            var headers = new[] { "ItemNumber", "CategoryID", "Category", "Catalog Description", "regular_price", "Sale Price (live)" };
            for (int i = 0; i < headers.Length; i++)
                worksheet.Cell(1, i + 1).Value = headers[i];
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.SetBold();
            headerRange.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DDEBF7"));

            int rowNumber = 2;
            foreach (var item in items)
            {
                worksheet.Cell(rowNumber, 1).SetValue(item.ItemNumber);
                worksheet.Cell(rowNumber, 2).SetValue(item.CategoryID);
                worksheet.Cell(rowNumber, 3).SetValue(CategoryName(item.CategoryID));
                worksheet.Cell(rowNumber, 4).SetValue(item.CatalogDescription ?? "");
                if (item.regular_price.HasValue)
                    worksheet.Cell(rowNumber, 5).SetValue(item.regular_price.Value);
                if (item.CatalogPrice.HasValue)
                    worksheet.Cell(rowNumber, 6).SetValue(item.CatalogPrice.Value);
                rowNumber++;
            }

            worksheet.Column(5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(6).Style.NumberFormat.Format = "#,##0.00";
            worksheet.SheetView.FreezeRows(1);
            worksheet.RangeUsed()?.SetAutoFilter();
            worksheet.Columns(1, headers.Length).AdjustToContents(1, Math.Min(rowNumber, 200));

            using var outputStream = new MemoryStream();
            workbook.SaveAs(outputStream);
            return outputStream.ToArray();
        }

        public static string CategoryName(short? categoryId)
        {
            switch (categoryId)
            {
                case 1: return "Retail Clearance";
                case 2: return "Equipment Clearance";
                case 3: return "Hard to Find Items";
                default: return "";
            }
        }

        /// <summary>
        /// Maps a header cell to a known field name; null when the column should be ignored
        /// </summary>
        private static string MapHeader(string header)
        {
            var normalized = Normalize(header);
            switch (normalized)
            {
                case "itemnumber":
                case "itemno":
                case "itemnbr":
                case "item":
                    return "ItemNumber";
                case "categoryid":
                case "category":
                    return "Category";
                case "regularprice":
                case "regprice":
                case "regularcost":
                    return "RegularPrice";
                case "saleprice":
                case "salecost":
                    // Detected only so the review screen can say the column is ignored;
                    // the website's sale price is the live catalog price, not this column
                    return "SalePrice";
                default:
                    return null;
            }
        }

        private static string Normalize(string value)
            => new string((value ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        private static string GetString(IXLWorksheet worksheet, int row, Dictionary<string, int> columns, string field)
        {
            if (!columns.TryGetValue(field, out var col))
                return null;
            return CellText(worksheet.Cell(row, col));
        }

        private static string CellText(IXLCell cell)
        {
            try
            {
                // CachedValue: stored result for formula cells, plain value otherwise — never re-evaluates
                var value = cell.CachedValue;
                if (value.IsBlank) return "";
                if (value.IsText) return value.GetText().Trim();
                if (value.IsNumber)
                {
                    var number = value.GetNumber();
                    return number == Math.Truncate(number)
                        ? ((long)number).ToString(CultureInfo.InvariantCulture)
                        : number.ToString(CultureInfo.InvariantCulture);
                }
                if (value.IsDateTime) return value.GetDateTime().ToString("yyyy-MM-dd");
                if (value.IsBoolean) return value.GetBoolean().ToString();
                return value.ToString();
            }
            catch
            {
                return cell.GetString()?.Trim() ?? "";
            }
        }

        private static short? ParseCategory(string raw)
        {
            var normalized = Normalize(raw);
            if (normalized.Length == 0) return null;
            if (normalized == "1" || normalized.Contains("retail")) return 1;
            if (normalized == "2" || normalized.Contains("equipment")) return 2;
            if (normalized == "3" || normalized.Contains("hardtofind")) return 3;
            return null;
        }

        private static decimal? ParsePrice(string raw, out bool invalid)
        {
            invalid = false;
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            var cleaned = raw.Replace("$", "").Replace(",", "").Trim();
            if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                return Math.Round(value, 4);
            invalid = true;
            return null;
        }

    }
}
