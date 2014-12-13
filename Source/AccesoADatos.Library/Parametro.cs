using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccesoADatos.Library
{
    public class Parametro
    {
        public Parametro(string nombre, object valor)
        {
            Nombre = nombre;
            Valor = valor;
        }

        public string Nombre { get; set; }
        public object Valor { get; set; }
    }
}