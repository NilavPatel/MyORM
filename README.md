# MyORM
## ADO.Net and SQL server  
  
1.  Create new database connection object using 
````csharp
using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
{
    // database operations
}
````
OR
````csharp
using (var dbConnection = ConnectionFactory.CreateConnection())
{
    // database operations
}
````
2.  where connectionString is string object which has value of connectionstring for database, or it will take default connectionstring from web config.
    Name of the default connectionstring must be "DefaultConnection".
    
### Methods for text Query
```csharp
dbConnection.ExecuteSingle<T>(); // with/Without Mapper
dbConnection.ExecuteList<T>(); // with/Without Mapper
dbConnection.ExecuteNonQuery();
dbConnection.ExecuteScalar<T>();
dbConnection.ExecuteNonQueryWithScope<T>(); // with return parameter
```
  
### Methods for Stored Procedure
```csharp
dbConnection.ExecuteSingleProc<T>(); // with/Without Mapper
dbConnection.ExecuteListProc<T>(); // with/Without Mapper
dbConnection.ExecuteNonQueryProc();
dbConnection.ExecuteScalarProc<T>();
dbConnection.ExecuteNonQueryProcWithReturn<T>() // with return parameter, Name of the return parameter must be "ReturnValue"
```

### Methods for transaction
```csharp
dbConnection.BeginTransaction();
dbConnection.CommitTransaction();
dbConnection.RollbackTransaction();
```

### Other Methods
```csharp
dbConnection.GetConnectionString();
dbConnection.GetOutParameters();
dbConnection.Dispose();  // dbConnection object will be automatically disposed as it inherits IDisposable.
```
