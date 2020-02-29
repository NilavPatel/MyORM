using System;
using System.Data;

namespace MyORM.Core.DataAccess
{
    /// <summary>
    /// Mapper with Index delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public delegate T MapperWithIndex<out T>(IDataReader reader, Int32 index);
}
