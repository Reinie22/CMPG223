/* =============================================================================
   Stored procedures - Lookups
   CMPG213 Group 6

   The small tables that fill dropdown lists. They all return the same two
   columns, Id and Name, so that one C# class and one mapping method can handle
   all of them.
   ============================================================================= */

IF OBJECT_ID('dbo.sp_Lookup_Provinces','P')           IS NOT NULL DROP PROCEDURE dbo.sp_Lookup_Provinces;
IF OBJECT_ID('dbo.sp_Lookup_AttractionCategories','P') IS NOT NULL DROP PROCEDURE dbo.sp_Lookup_AttractionCategories;
IF OBJECT_ID('dbo.sp_Lookup_ExpenseCategories','P')   IS NOT NULL DROP PROCEDURE dbo.sp_Lookup_ExpenseCategories;
IF OBJECT_ID('dbo.sp_Lookup_Roles','P')               IS NOT NULL DROP PROCEDURE dbo.sp_Lookup_Roles;
GO

CREATE PROCEDURE dbo.sp_Lookup_Provinces
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProvinceId AS Id, ProvinceName AS Name
    FROM dbo.Province
    ORDER BY ProvinceName;
END
GO

CREATE PROCEDURE dbo.sp_Lookup_AttractionCategories
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CategoryId AS Id, CategoryName AS Name
    FROM dbo.AttractionCategory
    ORDER BY CategoryName;
END
GO

CREATE PROCEDURE dbo.sp_Lookup_ExpenseCategories
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ExpenseCategoryId AS Id, CategoryName AS Name
    FROM dbo.ExpenseCategory
    ORDER BY CategoryName;
END
GO

CREATE PROCEDURE dbo.sp_Lookup_Roles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId AS Id, RoleName AS Name
    FROM dbo.Role
    ORDER BY RoleId;
END
GO

PRINT 'Lookup procedures created.';
GO
