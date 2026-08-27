using System;
using System.Collections.Generic;

namespace Wddc.WebContentManager.Services.WebContent.Sales.ClearanceSale
{
    /// <summary>
    /// One row of WebOrdering.dbo.Web_Clearance, as exposed by the apps API.
    /// Category 1 = Retail Clearance, 2 = Equipment Clearance, 3 = Hard to Find Items (managed by IT, never touched here).
    /// </summary>
    public class WebClearanceDto
    {
        public string ItemNumber { get; set; }
        public short? CategoryID { get; set; }
        public decimal? regular_price { get; set; }
        public decimal? sale_price { get; set; }
        public short? override_qty { get; set; }
        public string description { get; set; }
        public DateTime? expiry_date { get; set; }
    }

    /// <summary>
    /// Grid row: a clearance item enriched with web catalog info
    /// </summary>
    public class ClearanceGridItem : WebClearanceDto
    {
        public string CatalogDescription { get; set; }
        public decimal? CatalogPrice { get; set; }
        public bool InCatalog { get; set; }
    }

    /// <summary>
    /// Catalog lookup result for one item number
    /// </summary>
    public class ClearanceCatalogInfoDto
    {
        public string ItemNumber { get; set; }
        public bool ExistsInCatalog { get; set; }
        public bool ExistsInCategory3 { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
    }

    public class ClearanceReplaceRequestDto
    {
        public List<WebClearanceDto> Items { get; set; } = new List<WebClearanceDto>();
        public string RequestedBy { get; set; }
    }

    public class ClearanceReplaceResultDto
    {
        public string BackupTableName { get; set; }
        public int RowsBackedUp { get; set; }
        public int RowsDeleted { get; set; }
        public int RowsInserted { get; set; }
    }

    /// <summary>
    /// One backup table — a structurally identical copy of Web_Clearance that can be renamed into place
    /// </summary>
    public class ClearanceBackupInfoDto
    {
        public string TableName { get; set; }
        public DateTime BackupDate { get; set; }
        public string BackupBy { get; set; }
        public string BackupReason { get; set; }
        public int RowCount { get; set; }
    }

    public class ClearanceRestoreRequestDto
    {
        public string BackupTableName { get; set; }
        public string RequestedBy { get; set; }
    }

    public class ClearanceRestoreResultDto
    {
        public string RestoredFrom { get; set; }
        public string BackupTableName { get; set; }
        public int RowsDeleted { get; set; }
        public int RowsRestored { get; set; }
    }
}
