using System;
using System.Collections.Generic;
using System.Text;

namespace DbHelpers
{
    public interface ISqlExecutor:IDisposable
    {
        void ExecuteProcedure(IProcedure procedure);
        List<T> GetListFromSelect<T>(string selectCommand);
    }
}
