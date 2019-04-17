# MyORM

## Technology: ADO.Net and SQL server
## Test cases available for all scenarios.

````
1. Create new database connection object using 
    using (var dbConnection = new SqlDbConnection(connectionString))
    {
        // database operations
    }
2. where connectionString is string object which has value if connection string for database from web.config
````

### Methods for text Query
````
1. dbConnection.ExecuteSingle<T>();
2. dbConnection.ExecuteList<T>();
3. dbConnection.ExecuteNonQuery();
4. dbConnection.ExecuteScalar();
````

### Methods for Stored Procedure
````
1. dbConnection.ExecuteSingleProc<T>();
2. dbConnection.ExecuteListProc<T>();
3. dbConnection.ExecuteNonQueryProc();
4. dbConnection.ExecuteScalarProc();
````

### Methods for transaction
````
1. dbConnection.BeginTransaction();
2. dbConnection.CommitTransaction();
3. dbConnection.RollbackTransaction();
````

### Other Methods
````
1. dbConnection.GetConnectionString();
2. dbConnection.GetOutParameters();
3. dbConnection.Dispose();
````