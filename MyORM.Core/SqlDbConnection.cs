using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace MyORM.Core
{
    /// <summary>
    /// SQL generic connection class
    /// </summary>
    public class SqlDbConnection : IDisposable
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
        private List<SqlDbParameter> _outParameters { get; set; }

        public delegate T Mapper<out T>(IDataReader reader);

        public delegate T MapperWithIndex<out T>(IDataReader reader, Int32 index);

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
        internal SqlDbConnection(string connectionString)
        {
            _connection = SqlClientFactory.Instance.CreateConnection();
            _connection.ConnectionString = connectionString;
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
        private object ExecuteProcedure(string procedureName, ExecuteType executeType, List<SqlDbParameter> parameters)
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
        private object ExecuteQuery(string text, ExecuteType executeType, List<SqlDbParameter> parameters)
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
        private object Execute(string commandText, ExecuteType executeType, List<SqlDbParameter> parameters, CommandType commandType)
        {
            object returnObject = null;

            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _command = _connection.CreateCommand();
                    _command.CommandText = commandText;
                    _command.CommandType = commandType;
                    // 60 * 2 seconds
                    _command.CommandTimeout = 120;

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
                _outParameters = new List<SqlDbParameter>();
                _outParameters.Clear();

                for (int i = 0; i < _command.Parameters.Count; i++)
                {
                    if (_command.Parameters[i].Direction == ParameterDirection.Output)
                    {
                        _outParameters.Add(new SqlDbParameter(_command.Parameters[i].ParameterName,
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
        public T ExecuteSingleProc<T>(string procedureName, List<SqlDbParameter> parameters = null) where T : new()
        {
            Open();

            DbDataReader reader = (DbDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);
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
        public List<T> ExecuteListProc<T>(string procedureName, List<SqlDbParameter> parameters = null) where T : new()
        {
            List<T> objects = new List<T>();

            Open();

            DbDataReader reader = (DbDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);

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

        public IEnumerable<T> ExecuteListProc<T>(string text, List<SqlDbParameter> parameters, Mapper<T> mapper)
        {
            Open();

            using (var reader = (DbDataReader)ExecuteProcedure(text, ExecuteType.ExecuteReader, parameters))
            {
                var result = reader.ReadAll(mapper);

                UpdateOutParameters();

                Close();

                return result;
            }
        }

        public T ExecuteSingleProc<T>(string text, List<SqlDbParameter> parameters, Mapper<T> mapper)
        {
            Open();

            using (var reader = (DbDataReader)ExecuteProcedure(text, ExecuteType.ExecuteReader, parameters))
            {
                var result = reader.ReadFirstOrDefault(mapper);

                UpdateOutParameters();

                Close();

                return result;
            }
        }

        /// <summary>
        /// executes non query stored procedure with parameters (Insert, Update, Delete)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQueryProc(string procedureName, List<SqlDbParameter> parameters)
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
        public object ExecuteScalarProc(string procedureName, List<SqlDbParameter> parameters = null)
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
        public T ExecuteSingle<T>(string text, List<SqlDbParameter> parameters = null) where T : new()
        {
            Open();

            DbDataReader reader = (DbDataReader)ExecuteQuery(text, ExecuteType.ExecuteReader, parameters);
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
        public List<T> ExecuteList<T>(string text, List<SqlDbParameter> parameters = null) where T : new()
        {
            List<T> objects = new List<T>();

            Open();

            DbDataReader reader = (DbDataReader)ExecuteQuery(text, ExecuteType.ExecuteReader, parameters);

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
        /// execute data reader
        /// </summary>
        /// <param name="text"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteList<T>(string text, List<SqlDbParameter> parameters, Mapper<T> mapper)
        {
            Open();

            using (var reader = (DbDataReader)ExecuteQuery(text, ExecuteType.ExecuteReader, parameters))
            {
                var result = reader.ReadAll(mapper);

                UpdateOutParameters();

                Close();

                return result;
            }
        }

        public T ExecuteSingle<T>(string text, List<SqlDbParameter> parameters, Mapper<T> mapper)
        {
            Open();

            using (var reader = (DbDataReader)ExecuteQuery(text, ExecuteType.ExecuteReader, parameters))
            {
                var result = reader.ReadFirstOrDefault(mapper);

                UpdateOutParameters();

                Close();

                return result;
            }
        }

        /// <summary>
        /// executes non query stored procedure with parameters (Insert, Update, Delete)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string text, List<SqlDbParameter> parameters)
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
        public object ExecuteScalar(string text, List<SqlDbParameter> parameters = null)
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
            return _connection.ConnectionString;
        }

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
        public List<SqlDbParameter> GetOutParameters()
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
