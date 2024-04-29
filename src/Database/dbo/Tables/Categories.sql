CREATE TABLE [dbo].[Categories] (
    [category_id]   INT            NOT NULL,
    [category_name] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([category_id] ASC)
);

