namespace EmpowerId.ProductCatalog.ConsoleApp.Data.Queries
{
    public class GetProductsQuery
    {
        public long? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? CategoryName { get; set; }
        public decimal? PriceMinimum { get; set; }
        public decimal? PriceMaximum { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? DateAdded { get; set; }
    }
}
