using Wddc.Core.Domain;

namespace Wddc.WebContentManager.Core.Domain.Shipping
{
    public class ShipmentEstimateOptions
    {
        /// <summary>
        /// The total weight of the shipment in pounds
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Receiver city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Receiver two letter province abbreviation
        /// </summary>
        /// <example>AB</example>
        public string Province { get; set; }

        /// <summary>
        /// Receiver two letter country abbreviation
        /// </summary>
        /// <example>CA</example>
        public string Country { get; set; }

        /// <summary>
        /// Receiver postal code
        /// </summary>
        /// <example>A1A 1A1</example>
        public string PostalCode { get; set; }

        /// <summary>
        /// Length of largest product 
        /// </summary>
        public decimal? Length { get; set; }

        /// <summary>
        /// Width of largest product 
        /// </summary>
        public decimal? Width { get; set; }

        /// <summary>
        /// Height of largest product 
        /// </summary>
        public decimal? Height { get; set; }

        /// <summary>
        /// The warehouse the order will be shipped from
        /// </summary>
        public Warehouse? ShipFrom { get; set; }
    }
}