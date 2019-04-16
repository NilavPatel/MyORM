using System.Data;

namespace MyORM.Core
{
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
