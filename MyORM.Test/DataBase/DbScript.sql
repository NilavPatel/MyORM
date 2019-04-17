/****** Object:  Database [Test]    Script Date: 17-Apr-19 5:10:36 PM ******/
CREATE DATABASE [Test]
USE [Test]
GO
/****** Object:  Table [dbo].[Customer]    Script Date: 17-Apr-19 5:10:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[CustomerId] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAllCustomers]    Script Date: 17-Apr-19 5:10:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetAllCustomers] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * FROM dbo.Customer c
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetCustomerCount]    Script Date: 17-Apr-19 5:10:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetCustomerCount] 	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT count(c.CustomerId) FROM dbo.Customer c
END
GO
/****** Object:  StoredProcedure [dbo].[sp_InsertCustomer]    Script Date: 17-Apr-19 5:10:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_InsertCustomer] 
	-- Add the parameters for the stored procedure here
	@CustomerName nvarchar(10),
	@Identity bigint OUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO dbo.Customer
    (
        --CustomerId - this column value is auto-generated
        CustomerName
    )
    VALUES
    (
        -- CustomerId - bigint
        @CustomerName -- CustomerName - nvarchar
    ) 
	SET @Identity = SCOPE_IDENTITY()
END
GO
USE [master]
GO
ALTER DATABASE [Test] SET  READ_WRITE 
GO
