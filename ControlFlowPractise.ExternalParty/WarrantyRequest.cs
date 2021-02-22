using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlowPractise.ExternalParty
{
    // combinations  of (WarrantyRequestType, WarrantyRequestAction) are
    // (Verify, null) and WarrantyCaseId == null => create warranty case
    // (Verify, null)
    // (Verify, Cancel)
    // (Commit, null)
    public enum WarrantyRequestType
    {
        Verify,
        Commit
    }

    public enum WarrantyRequestAction
    {
        Cancel
    }

    public class ProductDetail
    {
        public ProductDetail(string productId, string specification)
        {
            ProductId = productId;
            Specification = specification;
        }
        public string ProductId { get; set; }
        public string Specification { get; set; }
    }

    public class PurchaserDetail
    {
        public PurchaserDetail(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class VendorDetail
    {
        public VendorDetail(
            string firstName,
            string lastName,
            string email,
            string phoneNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class OrderDetail
    {
        public OrderDetail(
            string orderId,
            PurchaserDetail purchaserDetail,
            VendorDetail vendorDetail)
        {
            OrderId = orderId;
            PurchaserDetail = purchaserDetail;
            VendorDetail = vendorDetail;
        }
        public string OrderId { get; set; }
        public List<ProductDetail> ProductDetails { get; set; } =
            new List<ProductDetail>();
        public PurchaserDetail PurchaserDetail { get; set; }
        public VendorDetail VendorDetail { get; set; }
    }

    public class WarrantyRequest
    {
        public WarrantyRequest(string transactionDate)
        {
            TransactionDate = transactionDate;
        }
        public WarrantyRequestType RequestType { get; set; }
        public WarrantyRequestAction? Action { get; set; }
        public string? WarrantyCaseId { get; set; }
        public string TransactionDate { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } =
            new List<OrderDetail>();
    }
}
