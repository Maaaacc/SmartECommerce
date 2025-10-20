using System.Collections.Generic;

namespace SmartECommerce.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueChangePercentage { get; set; }

        public int TotalOrders { get; set; }
        public decimal OrdersChangePercentage { get; set; }

        public int ActiveCustomers { get; set; }
        public decimal CustomersChangePercentage { get; set; }

        public int ProductsInStock { get; set; }
        public int LowStockCount { get; set; }

        public List<SalesTrendPoint> SalesTrend { get; set; } = new();

        public List<CategoryRevenue> RevenueByCategory { get; set; } = new();

        public List<ProductSales> TopSellingProducts { get; set; } = new();

        public List<RecentOrder> RecentOrders { get; set; } = new();

        public List<LowStockAlert> LowStockAlerts { get; set; } = new();
    }

    public class SalesTrendPoint
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CategoryRevenue
    {
        public string CategoryName { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ProductSales
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
    }

    public class RecentOrder
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Date { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
    }

    public class LowStockAlert
    {
        public string ProductName { get; set; }
        public int QuantityLeft { get; set; }
        public int ReorderThreshold { get; set; }
    }
}
