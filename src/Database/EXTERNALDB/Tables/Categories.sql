CREATE TABLE [EXTERNALDB].[Categories] (
    [category_id]      INT            NOT NULL,
    [category_name]    NVARCHAR (128) NOT NULL,
    [last_update_date] DATETIME       NOT NULL,
    CONSTRAINT [PK_EXT_Categories] PRIMARY KEY CLUSTERED ([category_id] ASC)
);

