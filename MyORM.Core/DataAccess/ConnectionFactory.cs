using System.Configuration;

namespace MyORM.Core.DataAccess
{
    public static class ConnectionFactory
    {
        public static SqlDbConnection CreateConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return new SqlDbConnection(connectionString);
        }

        public static SqlDbConnection CreateConnection(string connectionString)
        {
            return new SqlDbConnection(connectionString);
        }
    }
}
