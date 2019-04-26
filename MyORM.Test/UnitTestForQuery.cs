﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyORM.Core;
using MyORM.Test.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MyORM.Test
{
    [TestClass]
    public class UnitTestForQuery
    {
        [TestMethod]
        public void CreateNewDbConnection_WithConnectionString_ReturnsConnection()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
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
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, "Nilav1"),
                    new SqlDbParameter("LastName", System.Data.ParameterDirection.Input, "Patel"),
                    new SqlDbParameter("Identity ", System.Data.ParameterDirection.Output, 0)
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
        public void GetCustomerCount_ExecuteScalar_ReturnTotalCustomerCount()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var count = dbConnection.ExecuteScalar("Select Count(CustomerId) From Customer");
                Assert.IsTrue((int)count > 0);
            }
        }

        [TestMethod]
        public void CheckConnectionIsCloseAfterExecute_ExecuteScalar_ConnectionStateIsClosed()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
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
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
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
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customers = dbConnection.ExecuteList<Customer>("Select * From Customer");
                Assert.IsNotNull(customers);
            }
        }

        [TestMethod]
        public void GetCustomer_ExecuteSingle_ReturnsSingleCustomer()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customer = dbConnection.ExecuteSingle<Customer>("Select Top 1 * From Customer");
                Assert.IsNotNull(customer);
            }
        }

        [TestMethod]
        public void UpdateCustomer_ExecuteNoneQuery_UpdateCustomerInDatabase()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
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
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
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
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customer = dbConnection.ExecuteSingle("Select * From Customer", null, CustomerMap.Map);

                Assert.IsNotNull(customer);
            }
        }

        [TestMethod]
        public void GetAllCustomer_ExecuteListWithMapper_ReturnsDataReader()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customers = dbConnection.ExecuteList("Select * From Customer", null, CustomerMap.Map);

                Assert.IsNotNull(customers);
            }
        }        
    }
}
