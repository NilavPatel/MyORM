using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Threading.Tasks;

namespace MyORM.Core.DataAccess
{
    /// <summary>
    /// Sql db connection class for async
    /// </summary>
    public class SqlDbConnectionAsync : IDisposable
    {
        #region private variables

        /// <summary>
        /// SQL connection
        /// </summary>
        private DbConnection _connection { get; set; }

        /// <summary>
        /// SQL command
        /// </summary>
        private DbCommand _command { get; set; }

        /// <summary>
        /// SQL transaction
        /// </summary>
        private DbTransaction _transaction { get; set; }

        /// <summary>
        /// output parameters
        /// </summary>
        private IList<SqlDbParameter> _outParameters { get; set; }

        /// <summary>
        /// time out
        /// default value is 2 minute ( 2 * 60 seconds)
        /// </summary>
        private int _commandTimeout { get; set; }

        /// <summary>
        /// is object disposed ?
        /// </summary>
        private bool disposed = false;

        #endregion

        #region constructor

        /// <summary>
        /// get new object for database connection
        /// </summary>
        /// <param name="str">connection string</param>
        /// <param name="oldConnection">pass connection if exist</param>
        /// <param name="oldTransaction">pass transaction if exist</param>
        internal SqlDbConnectionAsync(string connectionString, int commandTimeout = 120)
        {
            _connection = SqlClientFactory.Instance.CreateConnection();
            _connection.ConnectionString = connectionString;
            _commandTimeout = commandTimeout;
        }

        #endregion

        #region private methods

        /// <summary>
        /// open connection
        /// </summary>
        private void Open()
        {
            try
            {
                if (_connection != null && _connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }
            }
            catch (Exception)
            {
                Close();
            }
        }

        /// <summary>
        /// close connection
        /// </summary>
        private void Close()
        {
            if (_connection != null && _transaction == null)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// executes stored procedure with DB parameters if they are passed
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="executeType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private async Task<object> ExecuteProcedure(string procedureName, ExecuteType executeType, IList<SqlDbParameter> parameters)
        {
            return await Execute(procedureName, executeType, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// execute query with DB parameters if they are passed
        /// </summary>
        /// <param name="text"></param>
        /// <param name="executeType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private async Task<object> ExecuteQuery(string text, ExecuteType executeType, IList<SqlDbParameter> parameters)
        {
            return await Execute(text, executeType, parameters, CommandType.Text);
        }

        /// <summary>
        /// execute stored procedure or text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="executeType"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        private async Task<object> Execute(string commandText, ExecuteType executeType, IList<SqlDbParameter> parameters, CommandType commandType)
        {
            object returnObject = null;

            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _command = _connection.CreateCommand();
                    _command.CommandText = commandText;
                    _command.CommandType = commandType;
                    _command.CommandTimeout = _commandTimeout;

                    if (_transaction != null)
                    {
                        _command.Transaction = _transaction;
                    }

                    // pass parameters to command
                    if (parameters != null)
                    {
                        _command.Parameters.Clear();

                        foreach (SqlDbParameter dbParameter in parameters)
                        {
                            DbParameter parameter = _command.CreateParameter();
                            parameter.ParameterName = "@" + dbParameter.Name;
                            parameter.Direction = dbParameter.Direction;
                            parameter.Value = dbParameter.Value;
                            _command.Parameters.Add(parameter);
                        }
                    }

                    switch (executeType)
                    {
                        case ExecuteType.ExecuteReader:
                            returnObject = await _command.ExecuteReaderAsync();
                            break;
                        case ExecuteType.ExecuteNonQuery:
                            returnObject = await _command.ExecuteNonQueryAsync();
                            break;
                        case ExecuteType.ExecuteScalar:
                            returnObject = await _command.ExecuteScalarAsync();
                            break;
                        default:
                            break;
                    }
                }
            }

            return returnObject;
        }

        /// <summary>
        /// updates output parameters from stored procedure
        /// </summary>
        private void UpdateOutParameters()
        {
            if (_command.Parameters.Count > 0)
            {
                _outParameters = new List<SqlDbParameter>();
                _outParameters.Clear();

                for (int i = 0; i < _command.Parameters.Count; i++)
                {
                    if (_command.Parameters[i].Direction == ParameterDirection.Output)
                    {
                        _outParameters.Add(new SqlDbParameter(_command.Parameters[i].ParameterName.Substring(1),
                                                          ParameterDirection.Output,
                                                          _command.Parameters[i].Value));
                    }
                }
            }
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Dispose SqlGenericConnection class object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                    }
                    if (_command != null)
                    {
                        _command.Dispose();
                    }
                    if (_connection != null)
                    {
                        _connection.Close();
                        _connection.Dispose();
                    }
                }

                disposed = true;
            }
        }

        #endregion

        #region public methods

        #region stored procedure methods

        /// <summary>
        /// executes scalar query stored procedure and maps result to single object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<T> ExecuteSingleProc<T>(string procedureName, IList<SqlDbParameter> parameters = null) where T : new()
        {
            try
            {
                Open();

                DbDataReader reader = (DbDataReader)(await ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters));
                T tempObject = new T();

                if (reader.HasRows && reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                        propertyInfo.SetValue(tempObject, reader.GetValue(i), null);
                    }
                }
                else
                {
                    tempObject = default(T);
                }

                reader.Close();

                UpdateOutParameters();

                Close();

                return tempObject;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// executes list query stored procedure and maps result generic list of objects (Select with parameters)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IList<T>> ExecuteListProc<T>(string procedureName, IList<SqlDbParameter> parameters = null) where T : new()
        {
            try
            {
                IList<T> objects = new List<T>();

                Open();

                DbDataReader reader = (DbDataReader)(await ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters));

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        T tempObject = new T();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.GetValue(i) != DBNull.Value)
                            {
                                PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                                propertyInfo.SetValue(tempObject, reader.GetValue(i), null);
                            }
                        }

                        objects.Add(tempObject);
                    }
                }
                else
                {
                    objects = default(IList<T>);
                }

                reader.Close();

                UpdateOutParameters();

                Close();

                return objects;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// execute list procedure with mapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public async Task<IList<T>> ExecuteListProc<T>(string procedureName, IList<SqlDbParameter> parameters, Mapper<T> mapper)
        {
            try
            {
                Open();

                using (var reader = (DbDataReader)(await ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters)))
                {
                    var result = reader.ToList(mapper);

                    UpdateOutParameters();

                    Close();

                    return result;
                }
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// execute single procedure with mapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="parameters"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public async Task<T> ExecuteSingleProc<T>(string procedureName, IList<SqlDbParameter> parameters, Mapper<T> mapper)
        {
            try
            {
                Open();

                using (var reader = (DbDataReader)(await ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters)))
                {
                    var result = reader.FirstOrDefault(mapper);

                    UpdateOutParameters();

                    Close();

                    return result;
                }
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// executes non query stored procedure with parameters (Insert, Update, Delete)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryProc(string procedureName, IList<SqlDbParameter> parameters)
        {
            try
            {
                int returnValue;

                Open();

                returnValue = (int)(await ExecuteProcedure(procedureName, ExecuteType.ExecuteNonQuery, parameters));

                UpdateOutParameters();

                Close();

                return returnValue;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// executes scalar query stored procedure with parameters (Count(), Sum(), Min(), Max() etc...)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public async Task<T> ExecuteScalarProc<T>(string procedureName, IList<SqlDbParameter> parameters = null)
        {
            try
            {
                object returnValue;

                Open();

                returnValue = await ExecuteProcedure(procedureName, ExecuteType.ExecuteScalar, parameters);

                UpdateOutParameters();

                Close();

                return (T)returnValue;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>		        /// <summary>
        /// execute non query procedure with return value		
        /// </summary>		
        /// <param name="procedureName"></param>		
        /// <param name="parameters"></param>		
        /// <returns></returns>		
        public async Task<T> ExecuteNonQueryProcWithReturn<T>(string procedureName, IList<SqlDbParameter> parameters)
        {
            try
            {
                Open();
                if (parameters == null)
                {
                    parameters = new List<SqlDbParameter>();
                }
                parameters.Add(new SqlDbParameter("ReturnValue", ParameterDirection.ReturnValue, default(T)));
                await ExecuteProcedure(procedureName, ExecuteType.ExecuteNonQuery, parameters);
                UpdateOutParameters();
                Close();
                return (T)_command.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        #endregion

        #region query methods

        /// <summary>
        /// executes scalar query stored procedure and maps result to single object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<T> ExecuteSingle<T>(string text, IList<SqlDbParameter> parameters = null) where T : new()
        {
            try
            {
                Open();

                DbDataReader reader = (DbDataReader)(await ExecuteQuery(text, ExecuteType.ExecuteReader, parameters));
                T tempObject = new T();

                if (reader.HasRows && reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                        propertyInfo.SetValue(tempObject, reader.GetValue(i), null);
                    }
                }
                else
                {
                    tempObject = default(T);
                }

                reader.Close();

                UpdateOutParameters();

                Close();

                return tempObject;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// executes list query stored procedure and maps result generic list of objects (Select with parameters)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IList<T>> ExecuteList<T>(string text, IList<SqlDbParameter> parameters = null) where T : new()
        {
            try
            {
                IList<T> objects = new List<T>();

                Open();

                DbDataReader reader = (DbDataReader)(await ExecuteQuery(text, ExecuteType.ExecuteReader, parameters));

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        T tempObject = new T();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.GetValue(i) != DBNull.Value)
                            {
                                PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                                propertyInfo.SetValue(tempObject, reader.GetValue(i), null);
                            }
                        }

                        objects.Add(tempObject);
                    }
                }
                else
                {
                    objects = default(IList<T>);
                }

                reader.Close();

                UpdateOutParameters();

                Close();

                return objects;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// execute list query with mapper
        /// </summary>
        /// <param name="text"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IList<T>> ExecuteList<T>(string text, IList<SqlDbParameter> parameters, Mapper<T> mapper)
        {
            try
            {
                Open();

                using (var reader = (DbDataReader)(await ExecuteQuery(text, ExecuteType.ExecuteReader, parameters)))
                {
                    var result = reader.ToList(mapper);

                    UpdateOutParameters();

                    Close();

                    return result;
                }
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// execute single query with mapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <param name="parameters"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public async Task<T> ExecuteSingle<T>(string text, IList<SqlDbParameter> parameters, Mapper<T> mapper)
        {
            try
            {
                Open();

                using (var reader = (DbDataReader)(await ExecuteQuery(text, ExecuteType.ExecuteReader, parameters)))
                {
                    var result = reader.FirstOrDefault(mapper);

                    UpdateOutParameters();

                    Close();

                    return result;
                }
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// executes non query stored procedure with parameters (Insert, Update, Delete)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQuery(string text, IList<SqlDbParameter> parameters)
        {
            try
            {
                int returnValue;

                Open();

                returnValue = (int)(await ExecuteQuery(text, ExecuteType.ExecuteNonQuery, parameters));

                UpdateOutParameters();

                Close();

                return returnValue;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// execute non query with identity scope
        /// </summary>
        /// <param name="text"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<T> ExecuteNonQueryWithScope<T>(string text, IList<SqlDbParameter> parameters)
        {
            try
            {
                int returnValue;

                Open();

                if (parameters == null) parameters.Add(new SqlDbParameter("Identity ", ParameterDirection.Output, default(T)));
                {
                    parameters = new List<SqlDbParameter>();
                }
                parameters.Add(new SqlDbParameter("Identity", ParameterDirection.Output, 0));
                text = text + " SET @Identity = SCOPE_IDENTITY()";
                returnValue = (int)(await ExecuteQuery(text, ExecuteType.ExecuteNonQuery, parameters));

                UpdateOutParameters();

                Close();

                return (T)_command.Parameters["@Identity"].Value;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// executes scalar query stored procedure with parameters (Count(), Sum(), Min(), Max() etc...)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public async Task<T> ExecuteScalar<T>(string text, IList<SqlDbParameter> parameters = null)
        {
            try
            {
                object returnValue;

                Open();

                returnValue = await ExecuteQuery(text, ExecuteType.ExecuteScalar, parameters);

                UpdateOutParameters();

                Close();

                return (T)returnValue;
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        #endregion

        #region transaction methods

        /// <summary>
        /// begin transaction
        /// </summary>
        public void BeginTransaction()
        {
            try
            {
                if (_connection != null)
                {
                    _connection.Open();
                    _transaction = _connection.BeginTransaction();
                }
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// commit transaction
        /// </summary>
        public void CommitTransaction()
        {
            try
            {
                if (_connection != null && _transaction != null)
                {
                    _transaction.Commit();
                    _transaction = null;
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <summary>
        /// rollback transaction
        /// </summary>
        public void RollbackTransaction()
        {
            try
            {
                if (_connection != null && _transaction != null)
                {
                    _transaction.Rollback();
                    _transaction = null;
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        #endregion

        #region Other Methods
        /// <summary>
        /// get connection string
        /// </summary>
        /// <returns></returns>
        public string GetConnectionString()
        {
            return _connection.ConnectionString;
        }

        //TODO: This method is only for test cases, remove it.
        /// <summary>
        /// get Db connection object
        /// </summary>
        /// <returns></returns>
        public DbConnection GetSqlConnection()
        {
            return _connection;
        }

        /// <summary>
        /// get output parameters
        /// </summary>
        /// <returns></returns>
        public IList<SqlDbParameter> GetOutParameters()
        {
            return _outParameters;
        }
        #endregion

        #region dispose method

        /// <summary>
        /// Dispose SqlGenericConnection class object
        /// </summary>
        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion
    }
}
