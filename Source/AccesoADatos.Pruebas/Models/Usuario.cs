using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccesoADatos.Pruebas.Models
{
    public class Usuario
    {
        public Guid UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public byte ZonaId { get; set; }
        public bool EstaActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Observacion { get; set; }

        public Zona Zona { get; set; }
    }
}