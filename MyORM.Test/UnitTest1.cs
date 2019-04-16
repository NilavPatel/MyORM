using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyORM.Core;
using MyORM.Test.Models;
using System.Collections.Generic;

namespace MyORM.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CreateNewDbConnection_WithConnectionString_ReturnsConnection()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new DbConnection(connectionString))
            {
                var sqlConnection = dbConnection.GetSqlConnection();
                Assert.IsTrue(sqlConnection.GetType().ToString() == "System.Data.SqlClient.SqlConnection");
                Assert.IsTrue(sqlConnection.ConnectionString.Length > 0);
            }
        }

        [TestMethod]
        public void GetCustomerCount_ExecuteScalar_ReturnTotalCustomerCount()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new DbConnection(connectionString))
            {
                var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer");
                Assert.IsTrue((int)count > 0);
            }

        }

        [TestMethod]
        public void CheckConnectionIsCloseAfterExecute_ExecuteScalar_ConnectionStateIsClosed()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new DbConnection(connectionString))
            {
                var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer");
                var sqlConnection = dbConnection.GetSqlConnection();
                Assert.IsTrue(sqlConnection.State == System.Data.ConnectionState.Closed);
            }
        }

        [TestMethod]
        public void GetCustomerCountByName_ExecuteScalarWithParameters_ReturnTotalCustomerCount()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new DbConnection(connectionString))
            {
                var parameters = new List<DbParameter>
                {
                    new DbParameter("CustomerName", System.Data.ParameterDirection.Input, "%Nilav2%")
                };
                var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer where CustomerName like @CustomerName", parameters);
                Assert.IsTrue((int)count > 0);
            }

        }

        [TestMethod]
        public void GetAllCustomer_ExecuteList_ReturnsCustomerList()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new DbConnection(connectionString))
            {                
                var customers = dbConnection.ExecuteList<Customer>("Select * From Customer");
                Assert.IsNotNull(customers);
            }
        }

        [TestMethod]
        public void GetCustomerById_ExecuteSingle_ReturnsSingleCustomer()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new DbConnection(connectionString))
            {
                var parameters = new List<DbParameter>();
                var customer = dbConnection.ExecuteSingle<Customer>("Select * From Customer where CustomerId = 1");
                Assert.IsNotNull(customer);
            }
        }

        [TestMethod]
        public void InsertCustomer_ExecuteNoneQuery_InsertCustomerInDatabase()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new DbConnection(connectionString))
            {
                var parameters = new List<DbParameter>
                {
                    new DbParameter("CustomerName", System.Data.ParameterDirection.Input, "NilavPatel"),
                    new DbParameter("Identity ", System.Data.ParameterDirection.Output, 0)
                };
                dbConnection.ExecuteNonQuery("insert into customer(customerName) values(@CustomerName) SET @Identity = SCOPE_IDENTITY()", parameters);
                var outParameters = dbConnection.GetOutParameters();
                if (outParameters != null && outParameters.Count > 0)
                {
                    var id = outParameters[0].Value;
                    var customer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", id));
                    Assert.IsNotNull(customer);
                    Assert.IsTrue(customer.CustomerName == "NilavPatel");
                }
                else
                {
                    Assert.IsNotNull(outParameters);
                }
            }
        }

        [TestMethod]
        public void UpdateCustomer_ExecuteNoneQuery_UpdateCustomerInDatabase()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new DbConnection(connectionString))
            {
                // first time update
                var parameters = new List<DbParameter>
                {
                    new DbParameter("CustomerName", System.Data.ParameterDirection.Input, "NilavPatel"),
                    new DbParameter("CustomerId ", System.Data.ParameterDirection.Input, 1)
                };
                dbConnection.ExecuteNonQuery("Update Customer set CustomerName = @CustomerName Where CustomerId = @CustomerId", parameters);
                
                var customer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", 1));
                Assert.IsNotNull(customer);
                Assert.IsTrue(customer.CustomerName == "NilavPatel");

                // second time update
                var newParameters = new List<DbParameter>
                {
                    new DbParameter("CustomerName", System.Data.ParameterDirection.Input, "NilavPatelTest"),
                    new DbParameter("CustomerId ", System.Data.ParameterDirection.Input, 1)
                };
                dbConnection.ExecuteNonQuery("Update Customer set CustomerName = @CustomerName Where CustomerId = @CustomerId", newParameters);

                var newCustomer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", 1));
                Assert.IsNotNull(newCustomer);
                Assert.IsTrue(newCustomer.CustomerName == "NilavPatelTest");
            }
        }

        [TestMethod]
        public void DeleteCustomer_ExecuteNoneQuery_DeleteCustomerInDatabase()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new DbConnection(connectionString))
            {
                var parameters = new List<DbParameter>
                {                    
                    new DbParameter("CustomerId ", System.Data.ParameterDirection.Input, 3)
                };
                dbConnection.ExecuteNonQuery("Delete From Customer Where CustomerId = @CustomerId", parameters);

                var customer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", 10002));
                Assert.IsNull(customer);
            }
        }
    }
}
