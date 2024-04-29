CREATE VIEW dbo.ProductsWithCategories
AS
SELECT dbo.Products.product_id, dbo.Products.product_name, dbo.Products.description, dbo.Categories.category_name
FROM   dbo.Products INNER JOIN
             dbo.Categories ON dbo.Products.category_id = dbo.Categories.category_id

GO
