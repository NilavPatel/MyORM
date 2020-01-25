using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyORM.Core.DataAccess;
using MyORM.Test.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MyORM.Test
{
    [TestClass]
    public class UnitTestForQuery
    {
        private string connectionString = "Data Source=DESKTOP-PBIS91N\\SQLEXPRESS;Initial Catalog=Test;Integrated Security=True";
        [TestMethod]
        public void CreateNewDbConnection_WithConnectionString_ReturnsConnection()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var sqlConnection = dbConnection.GetSqlConnection();
                Assert.IsInstanceOfType(sqlConnection, typeof(SqlConnection));
                Assert.IsTrue(sqlConnection.ConnectionString.Length > 0);

                sqlConnection.Open();
                Assert.AreEqual(sqlConnection.State, ConnectionState.Open);

                sqlConnection.Close();
                Assert.AreEqual(sqlConnection.State, ConnectionState.Closed);
            }
        }

        [TestMethod]
        public void InsertCustomer_ExecuteNoneQuery_InsertCustomerInDatabase()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, SqlDbHelper.GetSqlString("Nilav1")),
                    new SqlDbParameter("LastName", System.Data.ParameterDirection.Input, SqlDbHelper.GetSqlString("Patel")),
                    new SqlDbParameter("Identity ", System.Data.ParameterDirection.Output, SqlDbHelper.GetSqlInt32(0))
                };
                dbConnection.ExecuteNonQuery("insert into customer(FirstName, LastName) values(@FirstName, @LastName) SET @Identity = SCOPE_IDENTITY()", parameters);
                var outParameters = dbConnection.GetOutParameters();
                if (outParameters != null && outParameters.Count > 0)
                {
                    var id = outParameters[0].Value;
                    var customer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", id));
                    Assert.IsNotNull(customer);
                    Assert.AreEqual(customer.FirstName, "Nilav1");
                }
                else
                {
                    Assert.Fail("Error in inserting data, return values not found");
                }
            }
        }

        [TestMethod]
        public void InsertCustomer_ExecuteNoneQueryWithScope_InsertCustomerInDatabase()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, "NilavWithScope"),
                    new SqlDbParameter("LastName", System.Data.ParameterDirection.Input, "Patel")
                };
                var id = dbConnection.ExecuteNonQueryWithScope<int>("insert into customer(FirstName, LastName) values(@FirstName, @LastName)", parameters);
                var customer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", id));
                Assert.IsNotNull(customer);
                Assert.AreEqual(customer.FirstName, "NilavWithScope");
            }
        }

        [TestMethod]
        public void GetCustomerCount_ExecuteScalar_ReturnTotalCustomerCount()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer");
                Assert.IsTrue((int)count > 0);
            }
        }

        [TestMethod]
        public void CheckConnectionIsCloseAfterExecute_ExecuteScalar_ConnectionStateIsClosed()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer");
                var sqlConnection = dbConnection.GetSqlConnection();
                Assert.AreEqual(sqlConnection.State, ConnectionState.Closed);
            }
        }

        [TestMethod]
        public void GetCustomerCountByName_ExecuteScalarWithParameters_ReturnTotalCustomerCount()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, "%Nilav%")
                };
                var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer where FirstName like @FirstName", parameters);
                Assert.IsTrue((int)count > 0);
            }

        }

        [TestMethod]
        public void GetAllCustomer_ExecuteList_ReturnsCustomerList()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customers = dbConnection.ExecuteList<Customer>("Select * From Customer");
                Assert.IsNotNull(customers);
            }
        }

        [TestMethod]
        public void GetCustomer_ExecuteSingle_ReturnsSingleCustomer()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customer = dbConnection.ExecuteSingle<Customer>("Select Top 1 * From Customer");
                Assert.IsNotNull(customer);
            }
        }

        [TestMethod]
        public void UpdateCustomer_ExecuteNoneQuery_UpdateCustomerInDatabase()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customer = dbConnection.ExecuteSingle<Customer>("Select Top 1 * From Customer");
                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, "NilavUpdate"),
                    new SqlDbParameter("CustomerId ", System.Data.ParameterDirection.Input,customer.CustomerId)
                };
                dbConnection.ExecuteNonQuery("Update Customer set FirstName = @FirstName Where CustomerId = @CustomerId", parameters);

                var updatedCustomer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", customer.CustomerId));
                Assert.IsNotNull(updatedCustomer);
                Assert.AreEqual(updatedCustomer.FirstName, "NilavUpdate");
            }
        }

        [TestMethod]
        public void DeleteCustomer_ExecuteNoneQuery_DeleteCustomerInDatabase()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, "NilavUpdate")
                };
                dbConnection.ExecuteNonQuery("Delete From Customer Where FirstName = @FirstName", parameters);

                var customer = dbConnection.ExecuteSingle<Customer>("Select * From Customer where FirstName = @FirstName", parameters);
                Assert.IsNull(customer);
            }
        }

        [TestMethod]
        public void GetFirstOrDefaultCustomer_ExecuteSingleWithMapper_ReturnsDataReader()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customer = dbConnection.ExecuteSingle("Select * From Customer", null, CustomerMap.Map);

                Assert.IsNotNull(customer);
            }
        }

        [TestMethod]
        public void GetAllCustomer_ExecuteListWithMapper_ReturnsDataReader()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customers = dbConnection.ExecuteList("Select * From Customer", null, CustomerMap.Map);

                Assert.IsNotNull(customers);
            }
        }

        [TestMethod]
        public void GetAllCustomer_ExecuteListWithMapper_ThrowsTimeOutException()
        {
            Assert.ThrowsException<SqlException>(() =>
            {
                // create connection with timeout 1 second
                using (var dbConnection = ConnectionFactory.CreateConnection(connectionString, 1))
                {
                    var customers = dbConnection.ExecuteList("WAITFOR DELAY '00:02'; Select * From Customer", null, CustomerMap.Map);

                    Assert.IsNotNull(customers);
                }
            });
        }
    }
}
