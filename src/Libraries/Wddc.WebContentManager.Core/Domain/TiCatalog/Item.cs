namespace Wddc.WebContentManager.Core.Domain.TiCatalog
{
    public class Item
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public decimal Cost { get; set; }
        public string PriceGroup { get; set; }
        public string PriceLevel { get; set; }
        public string Location { get; set; }
        public bool BackOrdersEnabled { get; set; }
    }
}
