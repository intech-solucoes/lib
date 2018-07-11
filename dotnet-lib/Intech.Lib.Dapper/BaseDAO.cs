#region Usings
using Dapper.Contrib.Extensions;
using Intech.Lib.Web;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq; 
#endregion

namespace Intech.Lib.Dapper
{
    public abstract class BaseDAO<T> : IDisposable
        where T : class
    {
        public DbConnection Conexao { get; set; }
        public AppSettings Config { get; private set; }
        
        public BaseDAO()
        {
            var config = AppSettings.Get();

            var provider = config.ConnectionProvider;
            var connectionString = config.ConnectionString;

            if (provider == "sqlserver")
                Conexao = new SqlConnection(connectionString);
            else
                Conexao = new OracleConnection(connectionString);

            Conexao.Open();
        }

        public List<T> Listar()
        {
            return Conexao.GetAll<T>().ToList();
        }

        public T BuscarPorChave(object chave)
        {
            return Conexao.Get<T>(chave);
        }

        public virtual long Inserir(T entidade)
        {
            return Conexao.Insert(entidade);
        }

        public virtual bool Atualizar(T entidade)
        {
            return Conexao.Update(entidade);
        }

        public virtual bool Deletar(T entidade)
        {
            return Conexao.Delete(entidade);
        }

        public void Dispose()
        {
            Conexao.Close();
        }
    }
}
