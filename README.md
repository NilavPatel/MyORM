# MyORM
## ADO.Net and SQL server  
  
- Create new database connection object using 
````javascript
using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
{
    // database operations
}
````
````javascript
using (var dbConnection = ConnectionFactory.CreateConnection())
{
    // database operations
}
````
- where connectionString is string object which has value of connection string for database, or it will take default connection string from web config.
  
### Methods for text Query
- dbConnection.ExecuteSingle<T>(); // with/Without Mapper
- dbConnection.ExecuteList<T>(); // with/Without Mapper
- dbConnection.ExecuteNonQuery();
- dbConnection.ExecuteScalar<T>();
- dbConnection.ExecuteNonQueryWithScope<T>();
  
### Methods for Stored Procedure
- dbConnection.ExecuteSingleProc<T>(); // with/Without Mapper
- dbConnection.ExecuteListProc<T>(); // with/Without Mapper
- dbConnection.ExecuteNonQueryProc();
- dbConnection.ExecuteScalarProc<T>();
- dbConnection.ExecuteNonQueryProcWithReturn<T>() // with return parameter
  
### Methods for transaction
- dbConnection.BeginTransaction();
- dbConnection.CommitTransaction();
- dbConnection.RollbackTransaction();
  
### Other Methods
- dbConnection.GetConnectionString();
- dbConnection.GetOutParameters();
- dbConnection.Dispose();  // dbConnection object will be automatically disposed as it inherits IDisposable.
