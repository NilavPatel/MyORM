using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyORM.Core.DataAccess;
using MyORM.Test.Models;
using System.Collections.Generic;

namespace MyORM.Test
{
    [TestClass]
    public class UnitTestForProc
    {
        private string connectionString = "Data Source=DESKTOP-PBIS91N\\SQLEXPRESS;Initial Catalog=Test;Integrated Security=True";
        [TestMethod]
        public void InsertCustomer_ExecuteProc_InsertNewCustomer()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, SqlDbHelper.GetSqlString("Nilav1")),
                    new SqlDbParameter("LastName", System.Data.ParameterDirection.Input, SqlDbHelper.GetSqlString("Patel")),
                    new SqlDbParameter("Identity ", System.Data.ParameterDirection.Output, SqlDbHelper.GetSqlInt32(0))
                };
                dbConnection.ExecuteNonQueryProc("sp_InsertCustomer", parameters);
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
        public void GetAllCustomer_ExecuteProc_ReturnsAllCustomer()
        {

            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customers = dbConnection.ExecuteListProc<Customer>("sp_GetAllCustomers");
                Assert.IsInstanceOfType(customers, typeof(List<Customer>));
                Assert.IsNotNull(customers);
                Assert.IsTrue(customers.Count > 0);
            }
        }

        [TestMethod]
        public void GetCustomerCount_ExecuteScalarProc_ReturnTotalCustomerCount()
        {            
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var count = dbConnection.ExecuteScalarProc("sp_GetCustomerCount");
                Assert.IsTrue((int)count > 0);
            }
        }

        [TestMethod]
        public void GetFirstOrDefaultCustomer_ExecuteSingleProcWithMapper_ReturnsDataReader()
        {            
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customer = dbConnection.ExecuteSingleProc("sp_GetAllCustomers", null, CustomerMap.MapProc);

                Assert.IsNotNull(customer);
                Assert.IsNotNull(customer.CustomerName);
            }
        }

        [TestMethod]
        public void GetAllCustomer_ExecuteListProcWithMapper_ReturnsDataReader()
        {            
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var customers = dbConnection.ExecuteListProc("sp_GetAllCustomers", null, CustomerMap.MapProc);

                Assert.IsNotNull(customers);
            }
        }

        [TestMethod]
        public void GetLastCustomerId_ExecuteNonQueryProcWithReturn_ReturnsDataReader()
        {            
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                var result = dbConnection.ExecuteNonQueryProcWithReturn<int>("sp_ReturnStaticParameter", null);

                Assert.AreEqual(result, 501);
            }
        }
    }
}
