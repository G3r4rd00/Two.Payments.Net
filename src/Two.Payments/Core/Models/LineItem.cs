using Newtonsoft.Json;

namespace Two.Payments.Core.Models
{
    /// <summary>Represents a single line item within an order.</summary>
    public class LineItem
    {
        /// <summary>Name or description of the product or service.</summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>Quantity ordered.</summary>
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        /// <summary>Unit price (excluding tax), as a string with two decimal places.</summary>
        [JsonProperty("unit_price")]
        public string UnitPrice { get; set; }

        /// <summary>Tax rate as a decimal string (e.g. "0.25" for 25%).</summary>
        [JsonProperty("tax_rate")]
        public string TaxRate { get; set; }

        /// <summary>Tax class applied to the line item (e.g. "HIGH", "LOW", "NONE").</summary>
        [JsonProperty("tax_class_name")]
        public string TaxClassName { get; set; }

        /// <summary>Discount amount for this line item as a string.</summary>
        [JsonProperty("discount_amount")]
        public string DiscountAmount { get; set; }

        /// <summary>Type of the line item (e.g. "PHYSICAL", "DIGITAL", "SHIPPING_FEE").</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>Merchant's own identifier for the product.</summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; }
    }
}
