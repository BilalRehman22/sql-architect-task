namespace EmpowerId.ProductCatalog.ConsoleApp.Data.Models
{
    public class Product
    {
        public required long ProductId { get; set; }
        public required string ProductName { get; set; }
        public required string CategoryName { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
