﻿using System.Configuration;

namespace MyORM.Core.DataAccess
{
    public static class ConnectionFactory
    {
        /// <summary>
        /// get default connection from web.config
        /// </summary>
        /// <returns></returns>
        public static SqlDbConnection CreateConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return new SqlDbConnection(connectionString);
        }

        /// <summary>
        /// get database connection object with connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static SqlDbConnection CreateConnection(string connectionString)
        {
            return new SqlDbConnection(connectionString);
        }

        /// <summary>
        /// get database connection object with connection string and timeout
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static SqlDbConnection CreateConnection(string connectionString, int timeOut)
        {
            return new SqlDbConnection(connectionString, timeOut);
        }
    }
}
