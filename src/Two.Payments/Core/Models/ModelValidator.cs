using System;
using System.Collections.Generic;
using System.Linq;

namespace Two.Payments.Core.Models
{
    public static class ModelValidator
    {
        public static void ValidateOrder(CreateOrderRequest order)
        {
            var errors = new List<string>();

            // Validaciones de campos obligatorios de CreateOrderRequest
            if (string.IsNullOrWhiteSpace(order.Currency))
                errors.Add("Currency es obligatorio.");
            if (string.IsNullOrWhiteSpace(order.InvoiceType))
                errors.Add("InvoiceType es obligatorio.");
            if (string.IsNullOrWhiteSpace(order.GrossAmount))
                errors.Add("GrossAmount es obligatorio.");
            if (string.IsNullOrWhiteSpace(order.NetAmount))
                errors.Add("NetAmount es obligatorio.");
            if (string.IsNullOrWhiteSpace(order.TaxAmount))
                errors.Add("TaxAmount es obligatorio.");
            if (order.Buyer == null)
                errors.Add("Buyer es obligatorio.");
            if (order.LineItems == null || !order.LineItems.Any())
                errors.Add("Debe haber al menos un LineItem.");
            if (order.MerchantUrls == null)
                errors.Add("MerchantUrls es obligatorio.");
            else
            {
                if (string.IsNullOrWhiteSpace(order.MerchantUrls.MerchantConfirmationUrl))
                    errors.Add("MerchantConfirmationUrl es obligatorio.");
                if (string.IsNullOrWhiteSpace(order.MerchantUrls.MerchantCancelOrderUrl))
                    errors.Add("MerchantCancelOrderUrl es obligatorio.");
            }

            // Validaciones de Buyer
            if (order.Buyer != null)
            {
                if (order.Buyer.Representative == null)
                    errors.Add("Buyer.Representative es obligatorio.");
                else
                {
                    if (string.IsNullOrWhiteSpace(order.Buyer.Representative.FirstName))
                        errors.Add("Buyer.Representative.FirstName es obligatorio.");
                    if (string.IsNullOrWhiteSpace(order.Buyer.Representative.LastName))
                        errors.Add("Buyer.Representative.LastName es obligatorio.");
                    if (string.IsNullOrWhiteSpace(order.Buyer.Representative.PhoneNumber))
                        errors.Add("Buyer.Representative.PhoneNumber es obligatorio.");
                    if (string.IsNullOrWhiteSpace(order.Buyer.Representative.Email))
                        errors.Add("Buyer.Representative.Email es obligatorio.");
                }
                if (order.Buyer.Company == null)
                    errors.Add("Buyer.Company es obligatorio.");
                else
                {
                    if (string.IsNullOrWhiteSpace(order.Buyer.Company.CountryPrefix))
                        errors.Add("Buyer.Company.CountryPrefix es obligatorio.");
                    if (string.IsNullOrWhiteSpace(order.Buyer.Company.OrganizationNumber))
                        errors.Add("Buyer.Company.OrganizationNumber es obligatorio.");
                    if (string.IsNullOrWhiteSpace(order.Buyer.Company.CompanyName))
                        errors.Add("Buyer.Company.CompanyName es obligatorio.");
                }
            }

            // Validaciones de LineItems
            if (order.LineItems != null)
            {
                for (int i = 0; i < order.LineItems.Count; i++)
                {
                    var item = order.LineItems[i];
                    if (string.IsNullOrWhiteSpace(item.Name))
                        errors.Add($"LineItem[{i}].Name es obligatorio.");
                    if (string.IsNullOrWhiteSpace(item.Description))
                        errors.Add($"LineItem[{i}].Description es obligatorio.");
                    if (item.Quantity <= 0)
                        errors.Add($"LineItem[{i}].Quantity debe ser mayor que 0.");
                    if (string.IsNullOrWhiteSpace(item.UnitPrice))
                        errors.Add($"LineItem[{i}].UnitPrice es obligatorio.");
                    if (string.IsNullOrWhiteSpace(item.TaxRate))
                        errors.Add($"LineItem[{i}].TaxRate es obligatorio.");
                    if (string.IsNullOrWhiteSpace(item.TaxClassName))
                        errors.Add($"LineItem[{i}].TaxClassName es obligatorio.");
                    if (string.IsNullOrWhiteSpace(item.Type))
                        errors.Add($"LineItem[{i}].Type es obligatorio.");
                    if (string.IsNullOrWhiteSpace(item.ProductId))
                        errors.Add($"LineItem[{i}].ProductId es obligatorio.");
                }
            }

            if (errors.Any())
                throw new ArgumentException("Errores de validación: " + string.Join(" | ", errors));
        }
    }
}
