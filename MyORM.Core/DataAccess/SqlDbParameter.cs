using System.Data;

namespace MyORM.Core.DataAccess
{
    /// <summary>
    /// Db parameter class
    /// </summary>
    public class SqlDbParameter
    {
        public string Name { get; set; }

        public ParameterDirection Direction { get; set; }

        public object Value { get; set; }

        public SqlDbParameter(string paramName, ParameterDirection paramDirection, object paramValue)
        {
            Name = paramName;
            Direction = paramDirection;
            Value = paramValue;
        }
    }
}
