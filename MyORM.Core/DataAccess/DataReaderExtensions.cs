using System;
using System.Collections.Generic;
using System.Data;

namespace MyORM.Core.DataAccess
{
    /// <summary>
    /// data reader extensions class
    /// </summary>
    public static class DataReaderExtensions
    {
        /// <summary>
        /// get value or default by field name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// get value or default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this IDataRecord row, int ordinal)
        {
            return (T)(row.IsDBNull(ordinal) ? default(T) : row.GetValue(ordinal));
        }

        /// <summary>
        /// to list mapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IList<T> ToList<T>(this IDataReader reader, Mapper<T> mapper)
        {
            return reader.ToList((r, i1) => mapper(r));
        }

        /// <summary>
        /// to list mapper with index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IList<T> ToList<T>(this IDataReader reader, MapperWithIndex<T> mapper)
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

        /// <summary>
        /// first or default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this IDataReader reader, Mapper<T> mapper)
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
