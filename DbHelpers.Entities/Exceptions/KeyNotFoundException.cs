using System;
using System.Collections.Generic;
using System.Text;

namespace DbHelpers.Exceptions
{
    public class KeyNotFoundException:Exception
    {
        public string Key { get; set; }
        public KeyNotFoundException(string Key) : base($"Este key no fue encontrado {Key}") { }
    }
}
