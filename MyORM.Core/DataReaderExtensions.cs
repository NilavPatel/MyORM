using System;
using System.Data;

namespace MyORM.Core
{
    public static class DataReaderExtensions
    {
        public static T GetValueOrDefault<T>(this IDataRecord row, string fieldName)
        {
            try
            {
                int ordinal = row.GetOrdinal(fieldName);
                return row.GetValueOrDefault<T>(ordinal);
            }
            catch (IndexOutOfRangeException exception)
            {
                throw new ApplicationException("'" + fieldName + "' is invalid", exception);
            }
        }

        public static T GetValueOrDefault<T>(this IDataRecord row, int ordinal)
        {
            return (T)(row.IsDBNull(ordinal) ? default(T) : row.GetValue(ordinal));
        }
    }
}
