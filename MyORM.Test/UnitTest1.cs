using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyORM.Core;
using System.Collections.Generic;

namespace MyORM.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CreateConnection_WithConnectionString_ReturnsConnection()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            var dbConnection = new DbConnection(connectionString);
            var sqlConnection = dbConnection.GetSqlConnection();
            Assert.IsTrue(sqlConnection.GetType().ToString() == "System.Data.SqlClient.SqlConnection");
            Assert.IsTrue(sqlConnection.ConnectionString.Length > 0);
        }

        [TestMethod]
        public void GetCustomerCount_ExecuteScalar_ReturnTotalCustomerCount()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            var dbConnection = new DbConnection(connectionString);
            var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer");
            Assert.IsTrue((int)count > 0);
        }

        [TestMethod]
        public void GetCustomerCount_ExecuteScalar_CheckConnectionIsCloseAfterExecute()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            var dbConnection = new DbConnection(connectionString);
            var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer");
            var sqlConnection = dbConnection.GetSqlConnection();
            Assert.IsTrue(sqlConnection.State == System.Data.ConnectionState.Closed);
        }

        [TestMethod]
        public void GetCustomerCount_ExecuteScalarWithParameters_ReturnTotalCustomerCount()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            var dbConnection = new DbConnection(connectionString);
            var parameters = new List<DbParameter>();
            parameters.Add(new DbParameter("CustomerName",System.Data.ParameterDirection.Input, "%Nilav2%"));
            var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer where CustomerName like @CustomerName", parameters);
            Assert.IsTrue((int)count > 0);
        }
    }
}
