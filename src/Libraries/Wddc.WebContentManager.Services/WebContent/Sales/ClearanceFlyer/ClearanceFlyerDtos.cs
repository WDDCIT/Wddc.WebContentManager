using System;

namespace Wddc.WebContentManager.Services.WebContent.Sales.ClearanceFlyer
{
    /// <summary>
    /// One clearance flyer (the Apps API's api/Clearance/Flyers, AppData's Inventory.ClearanceFlyer): its look,
    /// the website section it lists, when it was last published, and how many items it would have now.
    /// FileName = the website's flyer: CS/Promos/&lt;FileName&gt;.pdf.
    /// </summary>
    public class ClearanceFlyerDto
    {
        public int ID { get; set; }
        public string FileName { get; set; }
        public int CategoryID { get; set; }
        public string Section { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Intro { get; set; }
        public string Note { get; set; }

        /// <summary>The footer of every page; parts separated by |.</summary>
        public string Footer { get; set; }
        public string AccentColor { get; set; }
        public bool HasCover { get; set; }
        public string CoverImageType { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string PublishedBy { get; set; }
        public DateTime? PublishedOn { get; set; }
        public int? PublishedItems { get; set; }
        public int Items { get; set; }
    }

    /// <summary>A flyer's cover picture.</summary>
    public class ClearanceFlyerCoverDto
    {
        public int ID { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }

    /// <summary>
    /// One item of a flyer (api/Clearance/FlyerItems): on the website's clearance list, on clearance, in stock.
    /// RegularPrice = the website's Reg Price; ClearancePrice = GP's REGULAR price today; GroupName = the product
    /// class, the flyer's heading.
    /// </summary>
    public class ClearanceFlyerItemDto
    {
        public string ItemNumber { get; set; }
        public string Description { get; set; }
        public decimal? RegularPrice { get; set; }
        public decimal? ClearancePrice { get; set; }
        public decimal? QtyAvailable { get; set; }
        public string GroupName { get; set; }
        public string SubGroupName { get; set; }
    }
}
