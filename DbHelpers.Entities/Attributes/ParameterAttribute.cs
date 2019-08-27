using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
namespace DbHelpers
{
    public class ParameterAttribute:Attribute
    {
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
        public string Name { get; set; }
        public int? Size { get; set; }
        public SqlDbType? SqlType { get; set; }
    }
}
