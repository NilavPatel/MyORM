using System;
using System.Collections.Generic;
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

        public static IEnumerable<T> ReadAll<T>(this IDataReader reader, Mapper<T> mapper)
        {
            return reader.ReadAll((r, i1) => mapper(r));
        }

        public static IEnumerable<T> ReadAll<T>(this IDataReader reader, MapperWithIndex<T> mapper)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (mapper == null)
            {
                throw new ArgumentNullException("mapper");
            }
            var set = new List<T>();
            var i = 0;
            while (reader.Read())
            {
                set.Add(mapper(reader, i));
                i++;
            }
            return set.AsReadOnly();
        }

        public static T ReadFirstOrDefault<T>(this IDataReader reader, Mapper<T> mapper)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (mapper == null)
            {
                throw new ArgumentNullException("mapper");
            }
            if (!reader.Read())
            {
                return default(T);
            }
            return mapper(reader);
        }
    }
}
