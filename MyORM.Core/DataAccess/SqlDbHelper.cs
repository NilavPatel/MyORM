using System;
using System.Data.SqlTypes;

namespace MyORM.Core.DataAccess
{
    /// <summary>
    /// helper class for sql data types
    /// convert nullable or values
    /// </summary>
    public class SqlDbHelper
    {
        public static SqlDateTime GetSqlDateTime(DateTime? dt)
        {
            if (dt == null)
            {
                return SqlDateTime.Null;
            }
            else if (dt.Equals(DateTime.MinValue))
            {
                return SqlDateTime.Null;
            }
            else
            {
                return dt.Value;
            }
        }

        public static SqlInt32 GetSqlInt32(int? value)
        {
            return (value == null) ? SqlInt32.Null : value.Value;
        }

        public static SqlInt32 GetSqlInt32(int? value, int null_value)
        {
            return (value == null || value == null_value) ? SqlInt32.Null : value.Value;
        }

        public static SqlDecimal GetSqlDecimal(Decimal? value)
        {
            return (value == null) ? SqlDecimal.Null : value.Value;
        }

        public static SqlDecimal GetSqlDecimal(Decimal? value, Decimal null_value)
        {
            return (value == null || value == null_value) ? SqlDecimal.Null : value.Value;
        }

        public static SqlString GetSqlString(string value)
        {
            return value ?? string.Empty;
        }

        public static SqlString GetSqlString(string value, string null_value)
        {
            return (String.IsNullOrEmpty(value)) ? SqlString.Null : value;
        }

        public static SqlDouble GetSqlDouble(double? value)
        {
            return (value == null) ? SqlDouble.Null : value.Value;
        }

        public static SqlBoolean GetSqlBoolean(bool? value)
        {
            return (value == null) ? SqlBoolean.Null : value.Value;
        }
    }
}
