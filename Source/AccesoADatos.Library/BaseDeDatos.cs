using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace AccesoADatos.Library
{
    public class BaseDeDatos : IDisposable
    {
        #region Atributos

        private bool _inicializado;
        private DbProviderFactory _dbProviderFactory;

        #endregion Atributos

        #region Constructores

        public BaseDeDatos(string nombreConnectionString)
        {
            if (String.IsNullOrWhiteSpace(nombreConnectionString))
                throw new ArgumentNullException("nombreConnectionString");

            NombreConnectionString = nombreConnectionString;
        }

        public BaseDeDatos(string connectionString, string providerName)
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
            Inicializar();

            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                {
                    using (var adapter = CrearDataAdapter(command))
                    {
                        var resultados = new DataTable();
                        adapter.Fill(resultados);

                        return resultados;
                    }
                }
            }
        }

        public IList<T> ObtenerListaObjetos<T>(string cmd, Func<Lector, T> mapeo, params Parametro[] parametros)
            where T : class
        {
            Inicializar();

            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                using (var reader = command.ExecuteReader())
                using (var lector = new Lector(reader))
                {
                    try
                    {
                        var res = new List<T>();
                        while (reader.Read())
                        {
                            res.Add(mapeo(lector));
                        }
                        return res;
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }

        public T ObtenerObjeto<T>(string cmd, Func<Lector, T> mapeo, params Parametro[] parametros)
            where T : class
        {
            Inicializar();

            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                using (var reader = command.ExecuteReader())
                using (var lector = new Lector(reader))
                {
                    try
                    {
                        if (reader.Read())
                        {
                            return mapeo(lector);
                        }

                        return default(T);
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }

        public object ObtenerEscalar(string cmd, params Parametro[] parametros)
        {
            Inicializar();

            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                {
                    return command.ExecuteScalar();
                }
            }
        }

        public int Ejecutar(string cmd, params Parametro[] parametros)
        {
            Inicializar();

            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            //  por ahora nada
        }

        #endregion Publicos

        #region Privados

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