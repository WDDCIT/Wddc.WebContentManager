using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wddc.WebContentManager.Models;
using Wddc.WebContentManager.Services.Logging;
using Wddc.WebContentManager.Services.WebContent.Sales.ClearanceSale;
using WebOrderingLog = Wddc.Api.Core.Domain.Entities.WebOrder.WebOrderingLog;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    /// <summary>
    /// Clearance Sale (Retail = category 1, Equipment = category 2).
    /// Category 3 (Hard to Find Items) is managed by IT and never shown or touched here.
    /// </summary>
    public class ClearanceSaleController : BaseController
    {
        private readonly IClearanceSaleService _clearanceSaleService;
        private readonly Services.Logging.ILogger _logger;

        public ClearanceSaleController(IClearanceSaleService clearanceSaleService, Services.Logging.ILogger logger)
        {
            _clearanceSaleService = clearanceSaleService;
            _logger = logger;
        }

        private string CurrentUser
        {
            get
            {
                var name = User?.Identity?.Name ?? "";
                var slash = name.LastIndexOf('\\');
                return (slash >= 0 ? name.Substring(slash + 1) : name).ToLower();
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Grid read: clearance items enriched with catalog description and current catalog price
        /// </summary>
        public async Task<JsonResult> GetWebClearanceAsync()
        {
            var items = await _clearanceSaleService.GetAllWebClearanceAsync() ?? new List<WebClearanceDto>();
            var enriched = await EnrichWithCatalogInfo(items);
            return Json(enriched);
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> CreateWebClearanceAsync([DataSourceRequest] DataSourceRequest request, ClearanceGridItem model)
        {
            try
            {
                NormalizeDates(model);
                var created = await _clearanceSaleService.CreateWebClearance(model);
                _logger.Information($"Added clearance sale item {created.ItemNumber} ({ClearanceExcel.CategoryName(created.CategoryID)}), Reg Price: {created.regular_price:0.00}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error adding clearance sale item {model.ItemNumber}: {Truncate(ex.Message, 200)}", ex, User, "WebOrdering");
                Response.StatusCode = 500;
                return Json(new { message = FriendlyMessage(ex, $"Error adding item {model.ItemNumber}") });
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> UpdateWebClearanceAsync([DataSourceRequest] DataSourceRequest request, ClearanceGridItem model, string originalItemNumber)
        {
            var original = string.IsNullOrWhiteSpace(originalItemNumber) ? model.ItemNumber : originalItemNumber;
            try
            {
                NormalizeDates(model);
                var updated = await _clearanceSaleService.UpdateWebClearance(original, model);
                var renamed = !string.Equals(original, updated.ItemNumber, StringComparison.OrdinalIgnoreCase)
                    ? $" (renamed from {original})" : "";
                _logger.Information($"Updated clearance sale item {updated.ItemNumber}{renamed} ({ClearanceExcel.CategoryName(updated.CategoryID)}), Reg Price: {updated.regular_price:0.00}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating clearance sale item {original}: {Truncate(ex.Message, 200)}", ex, User, "WebOrdering");
                Response.StatusCode = 500;
                return Json(new { message = FriendlyMessage(ex, $"Error updating item {original}") });
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> DeleteWebClearanceAsync([DataSourceRequest] DataSourceRequest request, ClearanceGridItem model)
        {
            try
            {
                await _clearanceSaleService.DeleteWebClearance(model.ItemNumber);
                _logger.Information($"Deleted clearance sale item {model.ItemNumber} ({ClearanceExcel.CategoryName(model.CategoryID)})", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting clearance sale item {model.ItemNumber}: {Truncate(ex.Message, 200)}", ex, User, "WebOrdering");
                Response.StatusCode = 500;
                return Json(new { message = FriendlyMessage(ex, $"Error deleting item {model.ItemNumber}") });
            }
        }

        public async Task<JsonResult> GetItemInfoAsync(string itemNumber)
        {
            var info = await _clearanceSaleService.GetItemInfo(itemNumber ?? "");
            return Json(info);
        }

        /// <summary>
        /// Downloads the current clearance list as an Excel file (also serves as the import template)
        /// </summary>
        public async Task<ActionResult> ExportAsync()
        {
            var items = await _clearanceSaleService.GetAllWebClearanceAsync() ?? new List<WebClearanceDto>();
            var enriched = await EnrichWithCatalogInfo(items);
            var bytes = ClearanceExcel.BuildWorkbook(enriched);
            _logger.Information($"Exported {enriched.Count} clearance sale items to Excel", null, User, "WebOrdering");
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Clearance Sale Items_{DateTime.Today:yyyy-MM-dd}.xlsx");
        }

        /// <summary>
        /// Step 1 of the Excel import: parse the workbook, validate every row, and
        /// build a review payload (errors, duplicates, catalog check, and a diff against the current list).
        /// Nothing is written to the database here.
        /// </summary>
        [AcceptVerbs("Post")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult> UploadImportAsync(IFormFile importFile)
        {
            try
            {
                if (importFile == null || importFile.Length == 0)
                    return BadRequestJson("Please choose an Excel file to upload.");
                var extension = Path.GetExtension(importFile.FileName ?? "").ToLowerInvariant();
                if (extension != ".xlsx" && extension != ".xlsm")
                    return BadRequestJson("The file must be an Excel workbook (.xlsx). Older .xls files should be re-saved as .xlsx first.");

                ClearanceParseResult parsed;
                using (var stream = new MemoryStream())
                {
                    await importFile.CopyToAsync(stream);
                    stream.Position = 0;
                    parsed = ClearanceExcel.Parse(stream);
                }
                if (parsed.FileError != null)
                    return BadRequestJson(parsed.FileError);

                // Rows that can never be imported vs. rows that are candidates
                var errorRows = parsed.Rows.Where(r => r.Errors.Count > 0).ToList();
                var candidateRows = parsed.Rows.Where(r => r.Errors.Count == 0).ToList();

                // Duplicate item numbers within the sheet (among importable rows)
                var duplicateGroups = candidateRows
                    .GroupBy(r => r.ItemNumber, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.ToList())
                    .ToList();

                // Catalog + category 3 check for everything in the sheet
                var catalogInfo = await _clearanceSaleService.ValidateItems(
                    candidateRows.Select(r => r.ItemNumber).Distinct(StringComparer.OrdinalIgnoreCase).ToList());
                var catalogByItem = catalogInfo.ToDictionary(c => c.ItemNumber, StringComparer.OrdinalIgnoreCase);

                // Diff against what is on the website right now
                var currentItems = await _clearanceSaleService.GetAllWebClearanceAsync() ?? new List<WebClearanceDto>();
                var currentByItem = currentItems.ToDictionary(c => c.ItemNumber, StringComparer.OrdinalIgnoreCase);
                var sheetItemNumbers = new HashSet<string>(candidateRows.Select(r => r.ItemNumber), StringComparer.OrdinalIgnoreCase);

                var removedItems = currentItems
                    .Where(c => !sheetItemNumbers.Contains(c.ItemNumber))
                    .OrderBy(c => c.CategoryID).ThenBy(c => c.ItemNumber)
                    .ToList();
                var removedCatalog = await _clearanceSaleService.ValidateItems(removedItems.Select(r => r.ItemNumber).ToList());
                var removedCatalogByItem = removedCatalog.ToDictionary(c => c.ItemNumber, StringComparer.OrdinalIgnoreCase);

                var rows = candidateRows.Select(r =>
                {
                    catalogByItem.TryGetValue(r.ItemNumber, out var catalog);
                    currentByItem.TryGetValue(r.ItemNumber, out var current);
                    var changes = current == null ? new List<string>() : DescribeChanges(current, r);
                    return new
                    {
                        row = r.RowNumber,
                        itemNumber = r.ItemNumber,
                        categoryID = r.CategoryID,
                        categoryName = ClearanceExcel.CategoryName(r.CategoryID),
                        regular_price = r.RegularPrice,
                        warnings = r.Warnings,
                        inCatalog = catalog?.ExistsInCatalog ?? false,
                        inCategory3 = catalog?.ExistsInCategory3 ?? false,
                        catalogDescription = catalog?.Description,
                        catalogPrice = catalog?.Price,
                        status = current == null ? "new" : (changes.Count > 0 ? "changed" : "unchanged"),
                        changes,
                    };
                }).ToList();

                var preview = new
                {
                    fileName = importFile.FileName,
                    sheetName = parsed.SheetName,
                    headerRow = parsed.HeaderRow,
                    columnsFound = parsed.ColumnsFound,
                    hasSalePriceColumn = parsed.HasSalePriceColumn,
                    rows,
                    errors = errorRows.Select(r => new { row = r.RowNumber, itemNumber = r.ItemNumber, messages = r.Errors }).ToList(),
                    duplicates = duplicateGroups.Select(g => g.Select(r => r.RowNumber).ToList()).ToList(),
                    removedItems = removedItems.Select(c =>
                    {
                        removedCatalogByItem.TryGetValue(c.ItemNumber, out var catalog);
                        return new
                        {
                            itemNumber = c.ItemNumber,
                            categoryID = c.CategoryID,
                            categoryName = ClearanceExcel.CategoryName(c.CategoryID),
                            regular_price = c.regular_price,
                            catalogPrice = catalog?.Price,
                            catalogDescription = catalog?.Description,
                        };
                    }).ToList(),
                    currentCount = currentItems.Count,
                };

                _logger.Information($"Previewed clearance import \"{importFile.FileName}\": {rows.Count} rows, {errorRows.Count} errors, {duplicateGroups.Count} duplicate groups, {removedItems.Count} not in sheet", null, User, "WebOrdering");
                return Json(preview);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error reading clearance import file: {Truncate(ex.Message, 200)}", ex, User, "WebOrdering");
                Response.StatusCode = 500;
                return Json(new { message = "Could not read the Excel file. " + ex.Message });
            }
        }

        public class ConfirmImportModel
        {
            public string FileName { get; set; }
            public List<WebClearanceDto> Items { get; set; } = new List<WebClearanceDto>();
        }

        /// <summary>
        /// Step 2 of the Excel import: replace all category 1 and 2 items with the reviewed list.
        /// The API snapshots the whole table into Web_Clearance_Backup inside the same transaction.
        /// </summary>
        [AcceptVerbs("Post")]
        public async Task<ActionResult> ConfirmImportAsync([FromBody] ConfirmImportModel model)
        {
            try
            {
                if (model == null || model.Items == null || model.Items.Count == 0)
                    return BadRequestJson("There are no items to import.");

                var result = await _clearanceSaleService.ReplaceAll(new ClearanceReplaceRequestDto
                {
                    Items = model.Items,
                    RequestedBy = CurrentUser,
                });

                _logger.Information($"Imported clearance sale from \"{model.FileName}\": {result.RowsInserted} items now live, {result.RowsDeleted} replaced, {result.RowsBackedUp} rows backed up to {result.BackupTableName}", null, User, "WebOrdering");
                return Json(new
                {
                    backupTableName = result.BackupTableName,
                    rowsBackedUp = result.RowsBackedUp,
                    rowsDeleted = result.RowsDeleted,
                    rowsInserted = result.RowsInserted,
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Error importing clearance sale: {Truncate(ex.Message, 200)}", ex, User, "WebOrdering");
                Response.StatusCode = 500;
                return Json(new { message = FriendlyMessage(ex, "Import failed — no changes were made") });
            }
        }

        public async Task<JsonResult> GetBackupsAsync()
        {
            var backups = await _clearanceSaleService.GetBackups();
            return Json(backups.Select(b => new
            {
                tableName = b.TableName,
                backupDate = b.BackupDate.ToString("yyyy-MM-dd HH:mm:ss"),
                backupBy = b.BackupBy,
                backupReason = b.BackupReason,
                rowCount = b.RowCount,
            }));
        }

        public class RestoreModel
        {
            public string BackupTableName { get; set; }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> RestoreBackupAsync([FromBody] RestoreModel model)
        {
            try
            {
                var result = await _clearanceSaleService.Restore(new ClearanceRestoreRequestDto
                {
                    BackupTableName = model?.BackupTableName,
                    RequestedBy = CurrentUser,
                });
                _logger.Information($"Restored clearance sale from backup {result.RestoredFrom}: {result.RowsRestored} items restored", null, User, "WebOrdering");
                return Json(new
                {
                    restoredFrom = result.RestoredFrom,
                    backupTableName = result.BackupTableName,
                    rowsDeleted = result.RowsDeleted,
                    rowsRestored = result.RowsRestored,
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Error restoring clearance sale backup: {Truncate(ex.Message, 200)}", ex, User, "WebOrdering");
                Response.StatusCode = 500;
                return Json(new { message = FriendlyMessage(ex, "Restore failed — no changes were made") });
            }
        }

        public async Task<ActionResult> LogAsync(int? pageNumber, int pageSize = 100, string referrerUrl = null)
        {
            IPagedList<WebOrderingLog> results = await _logger.GetAllWebOrderingLogsAsync(referrerUrl: referrerUrl, pageSize: pageSize, pageIndex: pageNumber ?? 1);

            var ajaxResult = new AjaxResults()
            {
                ResultStatus = ResultStatus.Success,
                Results = results
                    .Select(l => new { Text = l.ShortMessage.Length > 100 ? l.ShortMessage.Substring(0, 100) : l.ShortMessage, l.User, Created = l.CreatedOnUtc.ToLocalTime().ToString() }),
                TotalItemCount = results.TotalItemCount,
            };
            return Json(ajaxResult);
        }

        private async Task<List<ClearanceGridItem>> EnrichWithCatalogInfo(List<WebClearanceDto> items)
        {
            var catalogInfo = items.Count == 0
                ? new List<ClearanceCatalogInfoDto>()
                : await _clearanceSaleService.ValidateItems(items.Select(i => i.ItemNumber).ToList());
            var catalogByItem = catalogInfo.ToDictionary(c => c.ItemNumber, StringComparer.OrdinalIgnoreCase);

            return items.Select(i =>
            {
                catalogByItem.TryGetValue(i.ItemNumber, out var catalog);
                return new ClearanceGridItem
                {
                    ItemNumber = i.ItemNumber,
                    CategoryID = i.CategoryID,
                    regular_price = i.regular_price,
                    sale_price = i.sale_price,
                    override_qty = i.override_qty,
                    description = i.description,
                    expiry_date = i.expiry_date,
                    CatalogDescription = catalog?.Description,
                    CatalogPrice = catalog?.Price,
                    InCatalog = catalog?.ExistsInCatalog ?? false,
                };
            }).ToList();
        }

        private static List<string> DescribeChanges(WebClearanceDto current, ClearanceParsedRow row)
        {
            var changes = new List<string>();
            if (current.CategoryID != row.CategoryID)
                changes.Add($"category {ClearanceExcel.CategoryName(current.CategoryID)} → {ClearanceExcel.CategoryName(row.CategoryID)}");
            if (current.regular_price != row.RegularPrice)
                changes.Add($"Reg Price {Money(current.regular_price)} → {Money(row.RegularPrice)}");
            return changes;
        }

        private static string Money(decimal? value) => value?.ToString("0.00") ?? "—";

        private static string Truncate(string value, int maxLength)
            => value != null && value.Length > maxLength ? value.Substring(0, maxLength) : value;

        private ActionResult BadRequestJson(string message)
        {
            Response.StatusCode = 400;
            return Json(new { message });
        }

        /// <summary>
        /// The apps API wraps validation problems in ArgumentException("BadRequest: ...") — show those to the user as-is
        /// </summary>
        private static string FriendlyMessage(Exception ex, string fallback)
        {
            if (ex is ArgumentException)
            {
                var message = ex.Message;
                if (message.StartsWith("BadRequest: ", StringComparison.OrdinalIgnoreCase))
                    message = message.Substring("BadRequest: ".Length);
                return message.Trim().Trim('"');
            }
            return fallback + ".";
        }

        private static void NormalizeDates(WebClearanceDto model)
        {
            // Kendo sends 1969-12-31/1970-01-01 when the date picker is cleared
            if (model.expiry_date.HasValue && model.expiry_date.Value.Year < 1971)
                model.expiry_date = null;
            if (model.expiry_date.HasValue)
                model.expiry_date = model.expiry_date.Value.Date;
        }
    }
}
