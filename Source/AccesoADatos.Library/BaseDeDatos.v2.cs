using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace AccesoADatos.Library
{
    public class BaseDeDatos_v2 : IDisposable
    {
        #region Atributos

        private bool _inicializado;
        private DbProviderFactory _dbProviderFactory;

        #endregion Atributos

        #region Constructores

        public BaseDeDatos_v2(string nombreConnectionString)
        {
            if (String.IsNullOrWhiteSpace(nombreConnectionString))
                throw new ArgumentNullException("nombreConnectionString");

            NombreConnectionString = nombreConnectionString;
        }

        public BaseDeDatos_v2(string connectionString, string providerName)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("connectionString");
            if (String.IsNullOrWhiteSpace(providerName))
                throw new ArgumentNullException("providerName");

            ConnectionString = connectionString;
            ProviderName = providerName;
        }

        #endregion Constructores

        #region Propiedades

        public string NombreConnectionString { get; set; }

        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }

        #endregion Propiedades

        #region Metodos

        #region Publicos

        public DataTable ObtenerDataTable(string cmd, params Parametro[] parametros)
        {
            DataTable resultado = new DataTable();

            EjecutarOperacion(
                cmd, 
                parametros,
                (command) => 
                {
                    using (var adapter = CrearDataAdapter(command))
                    {
                        adapter.Fill(resultado);
                    }
                });

            return resultado;
        }

        public IList<T> ObtenerLista<T>(string cmd, Func<Lector, T> mapeo, params Parametro[] parametros)
        {
            IList<T> resultado = new List<T>();

            EjecutarOperacion(
                cmd,
                parametros,
                (command) => 
                {
                    using (var reader = command.ExecuteReader())
                    using (var lector = new Lector(reader))
                    {
                        while (reader.Read())
                        {
                            resultado.Add(mapeo(lector));
                        }
                    }
                });

            return resultado;
        }

        public T Obtener<T>(string cmd, Func<Lector, T> mapeo, params Parametro[] parametros)
        {
            T resultado = default(T);

            EjecutarOperacion(
                cmd, 
                parametros,
                (command) => 
                {
                    using (var reader = command.ExecuteReader())
                    using (var lector = new Lector(reader))
                    {
                        try
                        {
                            if (reader.Read())
                            {
                                resultado = mapeo(lector);
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                });

            return resultado;
        }

        public object Obtener(string cmd, params Parametro[] parametros)
        {
            object resultado = null;

            EjecutarOperacion(
                cmd,
                parametros,
                (command) => 
                {
                    resultado = command.ExecuteScalar();
                });

            return resultado;
        }

        public int Ejecutar(string cmd, params Parametro[] parametros)
        {
            int resultado = -1;

            EjecutarOperacion(
                cmd,
                parametros,
                (command) => 
                {
                    resultado = command.ExecuteNonQuery();
                });

            return resultado;
        }

        public void Dispose()
        {
            //  por ahora nada
        }

        #endregion Publicos

        #region Privados

        private void EjecutarOperacion(string cmd, IEnumerable<Parametro> parametros, Action<DbCommand> action)
        {
            Inicializar();

            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                {
                    action(command);
                }
            }
        }

        private void Inicializar()
        {
            if (_inicializado)
                return;

            if (!String.IsNullOrWhiteSpace(NombreConnectionString))
            {
                EstablecerDatosDeConexionDesdeConfiguracion(NombreConnectionString);
            }

            EstablecerDbProviderFactory(ProviderName);

            _inicializado = true;
        }

        private void EstablecerDatosDeConexionDesdeConfiguracion(string connStringName)
        {
            if (String.IsNullOrWhiteSpace(connStringName))
                throw new ArgumentNullException("connStringName");

            ConnectionStringSettings connString = null;
            try
            {
                connString = ConfigurationManager.ConnectionStrings[NombreConnectionString];
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    String.Format("No se ha configurado la Connection String '{0}' en el archivo de configuración.", NombreConnectionString),
                    "nombreConnectionString",
                    ex);
            }

            ConnectionString = connString.ConnectionString;
            ProviderName = connString.ProviderName;
        }

        private void EstablecerDbProviderFactory(string providerName)
        {
            _dbProviderFactory = DbProviderFactories.GetFactory(providerName);
            if (_dbProviderFactory == null)
                throw new ArgumentException("providerName");
        }

        private DbConnection CrearConnection()
        {
            var conn = _dbProviderFactory.CreateConnection();
            conn.ConnectionString = ConnectionString;

            return conn;
        }

        private DbCommand CrearCommand(DbConnection conn, string cmd, IEnumerable<Parametro> parametros)
        {
            var comando = conn.CreateCommand();
            comando.CommandText = cmd;
            comando.Parameters.AddRange(
                parametros
                .Select(p => CrearParameter(p.Nombre, p.Valor))
                .ToArray()
                );

            return comando;
        }

        private DbParameter CrearParameter(string nombre, object valor)
        {
            var parametro = _dbProviderFactory.CreateParameter();
            parametro.ParameterName = nombre;
            parametro.Value = valor;

            return parametro;
        }

        private DbDataAdapter CrearDataAdapter(DbCommand cmdSelect)
        {
            var adapter = _dbProviderFactory
                .CreateDataAdapter();
            adapter.SelectCommand = cmdSelect;

            return adapter;
        }

        #endregion Privados

        #endregion Metodos
    }
}