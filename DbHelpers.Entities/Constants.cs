using System;
using System.Collections.Generic;
using System.Text;

namespace DbHelpers
{
    using Exceptions;
    public static class Constants
    {
        private static Dictionary<string,string> ConnectionStrings;
        //private static Dictionary<string, SqlConnection> Connections;
        public static string GetConnectionString(string Key)
        {
            if (string.IsNullOrEmpty(Key))
                throw new NullReferenceException("Key cannot be null");
            if (!ConnectionStrings.ContainsKey(Key))
                throw new KeyNotFoundException(Key);
            return ConnectionStrings[Key];
        }
    }
}
