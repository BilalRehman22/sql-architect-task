namespace EmpowerId.ProductCatalog.ConsoleApp.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string? str, string? value)
        {
            if (str == null)
            {
                return false;
            }

            return str.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
