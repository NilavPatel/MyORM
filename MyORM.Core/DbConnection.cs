﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace MyORM.Core
{
    /// <summary>
    /// SQL generic connection class
    /// </summary>
    public class DbConnection : IDisposable
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
        public List<DbParameter> _outParameters { get; private set; }

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
        public DbConnection(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new SqlConnection(_connectionString);
        }

        /// <summary>
        /// get database connection object from old SqlConnection object
        /// </summary>
        /// <param name="oldConnection"></param>
        /// <param name="oldTransaction"></param>
        public DbConnection(SqlConnection oldConnection, SqlTransaction oldTransaction = null)
        {

            _connectionString = oldConnection.ConnectionString;
            _connection = oldConnection;

            //assign transaction if exist
            if (oldTransaction != null)
            {
                _transaction = oldTransaction;
            }
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
            if (_connection != null)
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

                    // pass stored procedure parameters to command
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
                    _transaction.Dispose();
                    _command.Dispose();
                    _connection.Dispose();
                }

                disposed = true;
            }
        }

        #endregion

        #region public methods

        #region stored procedure methods

        /// <summary>
        /// executes scalar query stored procedure without parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public T ExecuteSingleProc<T>(string procedureName) where T : new()
        {
            return ExecuteSingleProc<T>(procedureName, null);
        }

        /// <summary>
        /// executes scalar query stored procedure and maps result to single object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T ExecuteSingleProc<T>(string procedureName, List<DbParameter> parameters) where T : new()
        {
            Open();

            IDataReader reader = (IDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);
            T tempObject = new T();

            if (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                    propertyInfo.SetValue(tempObject, reader.GetValue(i), null);
                }
            }

            reader.Close();

            UpdateOutParameters();

            Close();

            return tempObject;
        }

        /// <summary>
        /// executes list query stored procedure without parameters (Select)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public List<T> ExecuteListProc<T>(string procedureName) where T : new()
        {
            return ExecuteListProc<T>(procedureName, null);
        }

        /// <summary>
        /// executes list query stored procedure and maps result generic list of objects (Select with parameters)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> ExecuteListProc<T>(string procedureName, List<DbParameter> parameters) where T : new()
        {
            List<T> objects = new List<T>();

            Open();

            IDataReader reader = (IDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);

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
        /// executes scalar query stored procedure without parameters (Count(), Sum(), Min(), Max() etc...)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public object ExecuteScalarProc(string procedureName)
        {
            return ExecuteScalarProc(procedureName, null);
        }

        /// <summary>
        /// executes scalar query stored procedure with parameters (Count(), Sum(), Min(), Max() etc...)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public object ExecuteScalarProc(string procedureName, List<DbParameter> parameters)
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
        /// executes scalar query stored procedure without parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public T ExecuteSingle<T>(string text) where T : new()
        {
            return ExecuteSingle<T>(text, null);
        }

        /// <summary>
        /// executes scalar query stored procedure and maps result to single object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T ExecuteSingle<T>(string text, List<DbParameter> parameters) where T : new()
        {
            Open();

            IDataReader reader = (IDataReader)ExecuteQuery(text, ExecuteType.ExecuteReader, parameters);
            T tempObject = new T();

            if (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                    propertyInfo.SetValue(tempObject, reader.GetValue(i), null);
                }
            }

            reader.Close();

            UpdateOutParameters();

            Close();

            return tempObject;
        }

        /// <summary>
        /// executes list query stored procedure without parameters (Select)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public List<T> ExecuteList<T>(string text) where T : new()
        {
            return ExecuteList<T>(text, null);
        }

        /// <summary>
        /// executes list query stored procedure and maps result generic list of objects (Select with parameters)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<T> ExecuteList<T>(string text, List<DbParameter> parameters) where T : new()
        {
            List<T> objects = new List<T>();

            Open();

            IDataReader reader = (IDataReader)ExecuteQuery(text, ExecuteType.ExecuteReader, parameters);

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
        /// executes scalar query stored procedure without parameters (Count(), Sum(), Min(), Max() etc...)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public object ExecuteScalar(string text)
        {
            return ExecuteScalar(text, null);
        }

        /// <summary>
        /// executes scalar query stored procedure with parameters (Count(), Sum(), Min(), Max() etc...)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public object ExecuteScalar(string text, List<DbParameter> parameters)
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
                _transaction = _connection.BeginTransaction();
            }
        }

        /// <summary>
        /// commit transaction
        /// </summary>
        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
            }
        }

        /// <summary>
        /// rollback transaction
        /// </summary>
        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
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
        /// get Sql Transaction
        /// </summary>
        /// <returns></returns>
        public SqlTransaction GetSqlTransaction()
        {
            return _transaction;
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

    /// <summary>
    /// Db parameter class
    /// </summary>
    public class DbParameter
    {
        public string Name { get; set; }
        public ParameterDirection Direction { get; set; }
        public object Value { get; set; }

        public DbParameter(string paramName, ParameterDirection paramDirection, object paramValue)
        {
            Name = paramName;
            Direction = paramDirection;
            Value = paramValue;
        }
    }
}
