using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Data;
using System.Threading.Tasks;

namespace DbHelpers
{
    public sealed class SqlExecutor:ISqlExecutor
    {
        private SqlConnection Connection;
        private SqlTransaction Transaction;
        internal SqlExecutor(string ConnectionString)
        {
            Connection = new SqlConnection(ConnectionString);
            Connection.Open();
        }
        public void SetTransaction()
        {
            if (Transaction != null)
                throw new InvalidOperationException("Solo se puede llamar a SetTransaction 1 vez");
            Transaction = Connection.BeginTransaction();
        }
        public void ExecuteProcedure(IProcedure procedure)
        {
            using(SqlCommand command = new SqlCommand(procedure.Name, Connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                if(Transaction!=null)
                    command.Transaction = Transaction;
                GetParametersFromProcedure(procedure, command.Parameters);
                command.ExecuteNonQuery();
                SetOutputParameters(command.Parameters, procedure);
            }
        }
        public List<T> GetListFromSelect<T>(string SelectCommand)
        {
            using(SqlCommand command = new SqlCommand(SelectCommand,Connection))
            {
                if (Transaction != null)
                    command.Transaction = Transaction;
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    using(DataTable table = new DataTable())
                    {
                        List<T> toReturn = new List<T>();
                        adapter.Fill(table);
                        Parallel.For(0,table.Rows.Count,(l)=>
                        {
                            T t = Activator.CreateInstance<T>();
                            Type type = t.GetType();
                            foreach(DataColumn column in table.Columns)
                            {
                                PropertyInfo property = type.GetProperties().FirstOrDefault(x => x.GetCustomAttribute<ColumnAttribute>()?.Name == column.ColumnName)?? type.GetProperty(column.ColumnName);
                                property.SetValue(t, Convert.ChangeType(table.Rows[l][column.ColumnName],property.PropertyType));
                            }
                            toReturn.Add(t);
                        });
                        return toReturn;
                    }
                }
            }
        }
        public void Dispose()
        {
            if (Transaction != null)
                Transaction.Dispose();
            Connection.Close();
            Connection.Dispose();
        }
        private void SetOutputParameters(SqlParameterCollection parameters, IProcedure procedure)
        {
            Type procType = procedure.GetType();
            PropertyInfo[] properties = procType.GetProperties();
            properties = properties.Where(x => x.GetCustomAttribute<ParameterAttribute>() != null).ToArray();
            foreach (SqlParameter parameter in parameters)
            {
                if (parameter.Direction != ParameterDirection.Input)
                {
                    PropertyInfo property = properties.FirstOrDefault(x => x.GetCustomAttribute<ParameterAttribute>().Name == parameter.ParameterName);
                    property.SetValue(procedure, parameter.Value);
                }
            }
        }
        private void GetParametersFromProcedure(IProcedure procedure,SqlParameterCollection parameters)
        {
            Type procType = procedure.GetType();
            PropertyInfo[] properties = procType.GetProperties();
            properties = properties.Where(x => x.GetCustomAttribute<ParameterAttribute>() != null).ToArray();
            foreach(PropertyInfo propInfo in properties)
            {
                ParameterAttribute attribute = propInfo.GetCustomAttribute<ParameterAttribute>();
                object value = propInfo.GetGetMethod().Invoke(procedure,new object[] { });
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = attribute.Name,
                    Value = value,
                    Direction = attribute.Direction
                };
                if (attribute.Size != null)
                    parameter.Size = attribute.Size.Value;
                parameters.Add(parameter);
            }
        }
    }
}
