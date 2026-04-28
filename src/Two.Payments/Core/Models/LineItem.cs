using Newtonsoft.Json;
using Two.Payments.Infrastructure.Serialization;

namespace Two.Payments.Core.Models
{
    /// <summary>Represents a single line item within an order.</summary>
    public class LineItem
    {
        private string _text;

        /// <summary>Name of the product or service.</summary>
        [JsonProperty("name")]
        public string Name
        {
            get => _text;
            set => _text = value;
        }

        /// <summary>Description of the product or service.</summary>
        [JsonProperty("description")]
        public string Description
        {
            get => _text;
            set => _text = value;
        }

        /// <summary>Quantity ordered.</summary>
        [JsonProperty("quantity")]
        [JsonConverter(typeof(FlexibleIntJsonConverter))]
        public int Quantity { get; set; }

        /// <summary>Unit of measure for the quantity (for example, "pcs").</summary>
        [JsonProperty("quantity_unit")]
        public string QuantityUnit { get; set; }

        /// <summary>Unit price (excluding tax), as a string with two decimal places.</summary>
        [JsonProperty("unit_price")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string UnitPrice { get; set; }

        /// <summary>Gross amount for the full line, as a string with two decimal places.</summary>
        [JsonProperty("gross_amount")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string GrossAmount { get; set; }

        /// <summary>Net amount for the full line, as a string with two decimal places.</summary>
        [JsonProperty("net_amount")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string NetAmount { get; set; }

        /// <summary>Tax amount for the full line, as a string with two decimal places.</summary>
        [JsonProperty("tax_amount")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string TaxAmount { get; set; }

        /// <summary>Tax rate as a decimal string (e.g. "0.25" for 25%).</summary>
        [JsonProperty("tax_rate")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string TaxRate { get; set; }

        /// <summary>Tax class applied to the line item (e.g. "HIGH", "LOW", "NONE").</summary>
        [JsonProperty("tax_class_name")]
        public string TaxClassName { get; set; }

        /// <summary>Discount amount for this line item as a string.</summary>
        [JsonProperty("discount_amount")]
        [JsonConverter(typeof(StringNumberJsonConverter))]
        public string DiscountAmount { get; set; }

        /// <summary>Type of the line item (e.g. "PHYSICAL", "DIGITAL", "SHIPPING_FEE").</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>Merchant's own identifier for the product.</summary>
        [JsonProperty("product_id")]
        public string ProductId { get; set; }
    }
}
