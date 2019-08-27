using System;
namespace DbHelpers
{
    public class ColumnAttribute:Attribute
    {
        public string Name { get; set; }
    }
}
