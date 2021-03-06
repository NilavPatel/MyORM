GO
CREATE TABLE [dbo].[Customer](
	[CustomerId] [bigint] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAllCustomers]    Script Date: 26-Apr-19 1:29:11 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_GetCustomerCount]    Script Date: 26-Apr-19 1:29:11 PM ******/
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
CREATE PROCEDURE sp_ReturnStaticParameter
AS
BEGIN
	
	SET NOCOUNT ON;

    Return 501;
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_InsertCustomer]    Script Date: 26-Apr-19 1:29:11 PM ******/
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
@FirstName NVARCHAR(10), 
@LastName  NVARCHAR(10), 
@Identity  BIGINT OUT
AS
    BEGIN
        -- SET NOCOUNT ON added to prevent extra result sets from
        -- interfering with SELECT statements.
        SET NOCOUNT ON;
        INSERT INTO dbo.Customer
        (
        --CustomerId - this column value is auto-generated
        dbo.Customer.FirstName, 
        dbo.Customer.LastName
        )
        VALUES
        (
        -- CustomerId - bigint
        @FirstName, 
        @LastName
        );
        SET @Identity = SCOPE_IDENTITY();
    END;
GO
USE [master]
GO
ALTER DATABASE [Test] SET  READ_WRITE 
GO
