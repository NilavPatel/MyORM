using System.Data;

namespace MyORM.Core.DataAccess
{
    /// <summary>
    /// Mapper delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <returns></returns>
    public delegate T Mapper<out T>(IDataReader reader);
}
