using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace MyORM.Core
{
    /// <summary>
    /// SQL generic connection class
    /// </summary>
    public class SqlDbConnection : IDisposable
    {
        #region private variables

        /// <summary>
        /// Connection string to connect with database
        /// </summary>
        private static string _connectionString { get; set; }

        /// <summary>
        /// SQL connection
        /// </summary>
        private SqlConnection _connection { get; set; }

        /// <summary>
        /// SQL command
        /// </summary>
        private SqlCommand _command { get; set; }

        /// <summary>
        /// SQL transaction
        /// </summary>
        private SqlTransaction _transaction { get; set; }

        /// <summary>
        /// output parameters
        /// </summary>
        private List<DbParameter> _outParameters { get; set; }

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
        public SqlDbConnection(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new SqlConnection(_connectionString);
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
        private object ExecuteProcedure(string procedureName, ExecuteType executeType, List<DbParameter> parameters)
        {
            return Execute(procedureName, executeType, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// execute query with DB parameters if they are passed
        /// </summary>
        /// <param name="text"></param>
        /// <param name="executeType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private object ExecuteQuery(string text, ExecuteType executeType, List<DbParameter> parameters)
        {
            return Execute(text, executeType, parameters, CommandType.Text);
        }

        /// <summary>
        /// execute stored procedure or text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="executeType"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        private object Execute(string commandText, ExecuteType executeType, List<DbParameter> parameters, CommandType commandType)
        {
            object returnObject = null;

            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _command = new SqlCommand(commandText, _connection)
                    {
                        CommandType = commandType
                    };

                    if (_transaction != null)
                    {
                        _command.Transaction = _transaction;
                    }

                    // pass parameters to command
                    if (parameters != null)
                    {
                        _command.Parameters.Clear();

                        foreach (DbParameter dbParameter in parameters)
                        {
                            SqlParameter parameter = new SqlParameter
                            {
                                ParameterName = "@" + dbParameter.Name,
                                Direction = dbParameter.Direction,
                                Value = dbParameter.Value
                            };
                            _command.Parameters.Add(parameter);
                        }
                    }

                    switch (executeType)
                    {
                        case ExecuteType.ExecuteReader:
                            returnObject = _command.ExecuteReader();
                            break;
                        case ExecuteType.ExecuteNonQuery:
                            returnObject = _command.ExecuteNonQuery();
                            break;
                        case ExecuteType.ExecuteScalar:
                            returnObject = _command.ExecuteScalar();
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
                _outParameters = new List<DbParameter>();
                _outParameters.Clear();

                for (int i = 0; i < _command.Parameters.Count; i++)
                {
                    if (_command.Parameters[i].Direction == ParameterDirection.Output)
                    {
                        _outParameters.Add(new DbParameter(_command.Parameters[i].ParameterName,
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
                    if(_transaction != null)
                    {
                        _transaction.Dispose();
                    }
                    if(_command != null)
                    {
                        _command.Dispose();
                    }
                    if(_connection != null)
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
        public T ExecuteSingleProc<T>(string procedureName, List<DbParameter> parameters = null) where T : new()
        {
            Open();

            SqlDataReader reader = (SqlDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);
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

        /// <summary>
        /// executes list query stored procedure and maps result generic list of objects (Select with parameters)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> ExecuteListProc<T>(string procedureName, List<DbParameter> parameters = null) where T : new()
        {
            List<T> objects = new List<T>();

            Open();

            SqlDataReader reader = (SqlDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);

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
                objects = default(List<T>);
            }

            reader.Close();

            UpdateOutParameters();

            Close();

            return objects;
        }

        /// <summary>
        /// executes non query stored procedure with parameters (Insert, Update, Delete)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQueryProc(string procedureName, List<DbParameter> parameters)
        {
            int returnValue;

            Open();

            returnValue = (int)ExecuteProcedure(procedureName, ExecuteType.ExecuteNonQuery, parameters);

            UpdateOutParameters();

            Close();

            return returnValue;
        }

        /// <summary>
        /// executes scalar query stored procedure with parameters (Count(), Sum(), Min(), Max() etc...)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public object ExecuteScalarProc(string procedureName, List<DbParameter> parameters = null)
        {
            object returnValue;

            Open();

            returnValue = ExecuteProcedure(procedureName, ExecuteType.ExecuteScalar, parameters);

            UpdateOutParameters();

            Close();

            return returnValue;
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
        public T ExecuteSingle<T>(string text, List<DbParameter> parameters = null) where T : new()
        {
            Open();

            SqlDataReader reader = (SqlDataReader)ExecuteQuery(text, ExecuteType.ExecuteReader, parameters);
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

        /// <summary>
        /// executes list query stored procedure and maps result generic list of objects (Select with parameters)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> ExecuteList<T>(string text, List<DbParameter> parameters = null) where T : new()
        {
            List<T> objects = new List<T>();

            Open();

            SqlDataReader reader = (SqlDataReader)ExecuteQuery(text, ExecuteType.ExecuteReader, parameters);

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
                objects = default(List<T>);
            }

            reader.Close();

            UpdateOutParameters();

            Close();

            return objects;
        }

        /// <summary>
        /// executes non query stored procedure with parameters (Insert, Update, Delete)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string text, List<DbParameter> parameters)
        {
            int returnValue;

            Open();

            returnValue = (int)ExecuteQuery(text, ExecuteType.ExecuteNonQuery, parameters);

            UpdateOutParameters();

            Close();

            return returnValue;
        }

        /// <summary>
        /// executes scalar query stored procedure with parameters (Count(), Sum(), Min(), Max() etc...)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public object ExecuteScalar(string text, List<DbParameter> parameters = null)
        {
            object returnValue;

            Open();

            returnValue = ExecuteQuery(text, ExecuteType.ExecuteScalar, parameters);

            UpdateOutParameters();

            Close();

            return returnValue;
        }

        #endregion

        #region transaction methods

        /// <summary>
        /// begin transaction
        /// </summary>
        public void BeginTransaction()
        {
            if (_connection != null)
            {
                _connection.Open();
                _transaction = _connection.BeginTransaction();
            }
        }

        /// <summary>
        /// commit transaction
        /// </summary>
        public void CommitTransaction()
        {
            if (_connection != null && _transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
                _connection.Close();
            }
        }

        /// <summary>
        /// rollback transaction
        /// </summary>
        public void RollbackTransaction()
        {
            if (_connection != null && _transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
                _connection.Close();
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
            return _connectionString;
        }

        /// <summary>
        /// get Sql connection object
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetSqlConnection()
        {
            return _connection;
        }

        /// <summary>
        /// get output parameters
        /// </summary>
        /// <returns></returns>
        public List<DbParameter> GetOutParameters()
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// execution type enumerations
    /// </summary>
    public enum ExecuteType
    {
        ExecuteReader,
        ExecuteNonQuery,
        ExecuteScalar
    }
}
