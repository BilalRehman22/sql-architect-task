CREATE TABLE [EXTERNALDB].[Products] (
    [product_id]       BIGINT          NOT NULL,
    [product_name]     NVARCHAR (255)  NOT NULL,
    [category_id]      INT             NOT NULL,
    [price]            DECIMAL (18, 2) NULL,
    [last_update_date] DATETIME        NOT NULL,
    CONSTRAINT [PK_EXT_Products] PRIMARY KEY CLUSTERED ([product_id] ASC),
    CONSTRAINT [FK_EXT_Products_Categories] FOREIGN KEY ([category_id]) REFERENCES [EXTERNALDB].[Categories] ([category_id])
);

