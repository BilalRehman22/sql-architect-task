CREATE TABLE [dbo].[OrderLineItems] (
    [lineitem_id] BIGINT          NOT NULL,
    [order_id]    BIGINT          NOT NULL,
    [product_id]  BIGINT          NOT NULL,
    [quantity]    INT             NOT NULL,
    [price]       DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_OrderLineItems_1] PRIMARY KEY CLUSTERED ([lineitem_id] ASC),
    CONSTRAINT [FK_OrderLineItems_Orders] FOREIGN KEY ([order_id]) REFERENCES [dbo].[Orders] ([order_id]),
    CONSTRAINT [FK_OrderLineItems_Products] FOREIGN KEY ([product_id]) REFERENCES [dbo].[Products] ([product_id])
);

