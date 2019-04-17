using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyORM.Core;
using MyORM.Test.Models;
using System.Collections.Generic;

namespace MyORM.Test
{
    [TestClass]
    public class UnitTestForProc
    {
        [TestMethod]
        public void InsertCustomer_ExecuteProc_InsertNewCustomer()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new SqlDbConnection(connectionString))
            {
                var parameters = new List<DbParameter>
                {
                    new DbParameter("CustomerName", System.Data.ParameterDirection.Input, "NilavPatel"),
                    new DbParameter("Identity ", System.Data.ParameterDirection.Output, 0)
                };
                dbConnection.ExecuteNonQueryProc("sp_InsertCustomer", parameters);
                var outParameters = dbConnection.GetOutParameters();
                if (outParameters != null && outParameters.Count > 0)
                {
                    var id = outParameters[0].Value;
                    var customer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", id));
                    Assert.IsNotNull(customer);
                    Assert.AreEqual(customer.CustomerName, "NilavPatel");
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
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = new SqlDbConnection(connectionString))
            {
                var customers = dbConnection.ExecuteListProc<Customer>("sp_GetAllCustomers");
                Assert.IsInstanceOfType(customers, typeof(List<Customer>));
                Assert.IsNotNull(customers);
                Assert.IsTrue(customers.Count > 0);
            }
        }
    }
}
