using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyORM.Core;
using MyORM.Test.Models;
using System.Collections.Generic;

namespace MyORM.Test
{
    [TestClass]
    public class UnitTestForTransaction
    {
        [TestMethod]
        public void SetTransaction_WithCommitTransaction_SaveData()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                dbConnection.BeginTransaction();

                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("CustomerName", System.Data.ParameterDirection.Input, "NilavPatel"),
                    new SqlDbParameter("Identity ", System.Data.ParameterDirection.Output, 0)
                };
                dbConnection.ExecuteNonQuery("insert into customer(customerName) values(@CustomerName) SET @Identity = SCOPE_IDENTITY()", parameters);
                
                var outParameters = dbConnection.GetOutParameters();
                if (outParameters != null && outParameters.Count > 0)
                {
                    var id = outParameters[0].Value;
                    var updateParameters = new List<SqlDbParameter>
                    {
                        new SqlDbParameter("CustomerName", System.Data.ParameterDirection.Input, "NilavPatelUpdate"),
                        new SqlDbParameter("CustomerId ", System.Data.ParameterDirection.Input,id)
                    };
                    dbConnection.ExecuteNonQuery("Update Customer set CustomerName = @CustomerName Where CustomerId = @CustomerId", updateParameters);
                    var updatedCustomer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", id));
                    Assert.IsNotNull(updatedCustomer);
                    Assert.AreEqual(updatedCustomer.CustomerName, "NilavPatelUpdate");
                }

                dbConnection.CommitTransaction();

                var sqlConnection = dbConnection.GetSqlConnection();
                Assert.AreEqual(sqlConnection.State, System.Data.ConnectionState.Closed);
            }
        }

        [TestMethod]
        public void SetTransaction_WithRollbackTransaction_SaveData()
        {
            var connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Test;Integrated Security=True";
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                dbConnection.BeginTransaction();

                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("CustomerName", System.Data.ParameterDirection.Input, "NilavPatel"),
                    new SqlDbParameter("Identity ", System.Data.ParameterDirection.Output, 0)
                };
                dbConnection.ExecuteNonQuery("insert into customer(customerName) values(@CustomerName) SET @Identity = SCOPE_IDENTITY()", parameters);

                dbConnection.RollbackTransaction();

                var outParameters = dbConnection.GetOutParameters();
                if (outParameters != null && outParameters.Count > 0)
                {
                    var id = outParameters[0].Value;                    
                    var customer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", id));
                    Assert.IsNull(customer);
                }

                var sqlConnection = dbConnection.GetSqlConnection();
                Assert.AreEqual(sqlConnection.State, System.Data.ConnectionState.Closed);
            }
        }
    }
}
