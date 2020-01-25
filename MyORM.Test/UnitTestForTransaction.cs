using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyORM.Core.DataAccess;
using MyORM.Test.Models;
using System.Collections.Generic;

namespace MyORM.Test
{
    [TestClass]
    public class UnitTestForTransaction
    {
        private string connectionString = "Data Source=DESKTOP-PBIS91N\\SQLEXPRESS;Initial Catalog=Test;Integrated Security=True";
        [TestMethod]
        public void SetTransaction_WithCommitTransaction_SaveData()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                dbConnection.BeginTransaction();

                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, "Nilav"),
                    new SqlDbParameter("LastName", System.Data.ParameterDirection.Input, "Patel"),
                    new SqlDbParameter("Identity ", System.Data.ParameterDirection.Output, 0)
                };
                dbConnection.ExecuteNonQuery("insert into customer(FirstName, LastName) values(@FirstName, @LastName) SET @Identity = SCOPE_IDENTITY()", parameters);

                var outParameters = dbConnection.GetOutParameters();
                if (outParameters != null && outParameters.Count > 0)
                {
                    var id = outParameters[0].Value;
                    var updateParameters = new List<SqlDbParameter>
                    {
                        new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, "NilavUpdate"),
                        new SqlDbParameter("CustomerId ", System.Data.ParameterDirection.Input,id)
                    };
                    dbConnection.ExecuteNonQuery("Update Customer set FirstName = @FirstName Where CustomerId = @CustomerId", updateParameters);
                    var updatedCustomer = dbConnection.ExecuteSingle<Customer>(string.Format("Select * From Customer where CustomerId = {0}", id));
                    Assert.IsNotNull(updatedCustomer);
                    Assert.AreEqual(updatedCustomer.FirstName, "NilavUpdate");
                }

                dbConnection.CommitTransaction();

                var sqlConnection = dbConnection.GetSqlConnection();
                Assert.AreEqual(sqlConnection.State, System.Data.ConnectionState.Closed);
            }
        }

        [TestMethod]
        public void SetTransaction_WithRollbackTransaction_RevertData()
        {
            using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
            {
                dbConnection.BeginTransaction();

                var parameters = new List<SqlDbParameter>
                {
                    new SqlDbParameter("FirstName", System.Data.ParameterDirection.Input, "Nilav"),
                    new SqlDbParameter("LastName", System.Data.ParameterDirection.Input, "Patel"),
                    new SqlDbParameter("Identity ", System.Data.ParameterDirection.Output, 0)
                };
                dbConnection.ExecuteNonQuery("insert into customer(FirstName, LastName) values(@FirstName, @LastName) SET @Identity = SCOPE_IDENTITY()", parameters);

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
