CREATE TABLE [dbo].[Products] (
    [product_id]   BIGINT          NOT NULL,
    [product_name] NVARCHAR (255)  NOT NULL,
    [category_id]  INT             NOT NULL,
    [price]        DECIMAL (18, 2) NULL,
    [description]  NTEXT           NULL,
    [image_url]    NVARCHAR (MAX)  NULL,
    [date_added]   DATETIME        CONSTRAINT [DF_Products_date_added] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([product_id] ASC),
    CONSTRAINT [FK_Products_Categories] FOREIGN KEY ([category_id]) REFERENCES [dbo].[Categories] ([category_id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Product_Category]
    ON [dbo].[Products]([category_id] ASC);

