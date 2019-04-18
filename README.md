# MyORM

## Technology: ADO.Net and SQL server
## Test cases available for all scenarios.

````javascript
1. Create new database connection object using 
    using (var dbConnection = ConnectionFactory.CreateConnection(connectionString))
    {
        // database operations
    }

    OR

    using (var dbConnection = ConnectionFactory.CreateConnection())
    {
        // database operations
    }

2. where connectionString is string object which has value of connection string for database,
   or it will take default connection string from web config.
````

### Methods for text Query
````javascript
1. dbConnection.ExecuteSingle<T>();
2. dbConnection.ExecuteList<T>();
3. dbConnection.ExecuteNonQuery();
4. dbConnection.ExecuteScalar();
````

### Methods for Stored Procedure
````javascript
1. dbConnection.ExecuteSingleProc<T>();
2. dbConnection.ExecuteListProc<T>();
3. dbConnection.ExecuteNonQueryProc();
4. dbConnection.ExecuteScalarProc();
````

### Methods for transaction
````javascript
1. dbConnection.BeginTransaction();
2. dbConnection.CommitTransaction();
3. dbConnection.RollbackTransaction();
````

### Other Methods
````javascript
1. dbConnection.GetConnectionString();
2. dbConnection.GetOutParameters();
3. dbConnection.Dispose();
````