using MyORM.Core.DataAccess;
using System.Data;

namespace MyORM.Test.Models
{
    public class CustomerMap
    {
        public static Customer Map(IDataReader dataReader)
        {
            return new Customer()
            {
                CustomerId = dataReader.GetValueOrDefault<long>("CustomerId"),
                FirstName = dataReader.GetValueOrDefault<string>("FirstName"),
                LastName = dataReader.GetValueOrDefault<string>("LastName")
            };
        }

        public static Customer MapProc(IDataReader dataReader)
        {
            return new Customer()
            {
                CustomerId = dataReader.GetValueOrDefault<long>("CustomerId"),
                FirstName = dataReader.GetValueOrDefault<string>("FirstName"),
                LastName = dataReader.GetValueOrDefault<string>("LastName"),
                CustomerName = dataReader.GetValueOrDefault<string>("FirstName") + " " +dataReader.GetValueOrDefault<string>("LastName")
            };
        }
    }
}
