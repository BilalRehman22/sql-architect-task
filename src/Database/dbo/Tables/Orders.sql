CREATE TABLE [dbo].[Orders] (
    [order_id]      BIGINT        NOT NULL,
    [order_date]    DATETIME      NOT NULL,
    [customer_name] NVARCHAR (64) NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([order_id] ASC)
);

