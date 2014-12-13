using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace AccesoADatos.Library
{
    public class Lector : IDisposable
    {
        private DbDataReader _reader;

        public Lector(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            _reader = reader;
        }

        public object this[string nombre]
        {
            get 
            {
                var valor = _reader[nombre];
                if (valor is DBNull)
                    return null;
                
                return valor;
            }    
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}