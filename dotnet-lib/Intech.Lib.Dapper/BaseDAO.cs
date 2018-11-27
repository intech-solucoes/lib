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

        public virtual List<T> Listar()
        {
            try
            {
                return Conexao.GetAll<T>().ToList();
            }
            finally
            {
                Conexao.Close();
            }
        }

        public virtual T BuscarPorChave(object chave)
        {
            try
            {
                return Conexao.Get<T>(chave);
            }
            finally
            {
                Conexao.Close();
            }
        }

        public virtual long Inserir(T entidade)
        {
            try
            {
                return Conexao.Insert(entidade);
            }
            finally
            {
                Conexao.Close();
            }
        }

        public virtual bool Atualizar(T entidade)
        {
            try
            {
                return Conexao.Update(entidade);
            }
            finally
            {
                Conexao.Close();
            }
        }

        public virtual bool Deletar(T entidade)
        {
            try
            {
                return Conexao.Delete(entidade);
            }
            finally
            {
                Conexao.Close();
            }
        }

        public void Dispose()
        {
            Conexao.Close();
        }
    }
}
