using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using AccesoADatos.Library;

using AccesoADatos.Pruebas.Models;

namespace AccesoADatos.Pruebas
{
    public partial class Default : System.Web.UI.Page
    {
        private BaseDeDatos_v2 _bd;
        //private BaseDeDatos _bd;

        public Default()
        {
            _bd = new BaseDeDatos_v2("EjemploBD");
            //_bd = new BaseDeDatos("EjemploBD");
            //_bd = new BaseDeDatos("Server=.;Database=Ejemplo;Integrated Security=True","System.Data.SqlClient")
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Probar();
            }
            catch (Exception ex)
            {
                lblMensajeError.Text = ex.Message;
            }
        }

        private void Probar()
        {
            using (var ts = new TransactionScope())
            {
                ContarUsuarios();
                ContarUsuariosActivos();
                ObtenerDataTableUsuariosDeZona1();
                ObtenerDataTableUsuariosActivasConZonas();
                ObtenerListaUsuariosDeZona2();
                ObtenerListaIdsDeUsuariosActivosDeZona2();
                ObtenerListaUsuariosConZonas();
                ObtenerUsuario1();
                ObtenerUsuario2();
                ActivarUsuarios();
                DesactivarUsuario3();

                var nuevoUsuarioId = Guid.NewGuid();
                AgregarUsuarioNuevo(nuevoUsuarioId);
                
                EliminarUsuario(nuevoUsuarioId);

                ts.Complete();
            }
        }

        private void ObtenerListaUsuariosConZonas()
        {
            var usuariosConZonas =
                _bd.ObtenerLista<Usuario>(
                    "SELECT U.*, Z.Nombre AS ZonaNombre FROM Usuarios AS U JOIN Zonas AS Z ON U.ZonaId = Z.ZonaId",
                    MapearUsuarioConZona
                );
        }

        private void EliminarUsuario(Guid nuevoUsuarioId)
        {
            var filasEliminado =
                _bd.Ejecutar(
                    "DELETE Usuarios WHERE UsuarioId = @UsuarioId",
                    new Parametro("@UsuarioId", nuevoUsuarioId)
                );
        }

        private void AgregarUsuarioNuevo(Guid nuevoUsuarioId)
        {
            var filasAgregado =
                _bd.Ejecutar(
                    "INSERT INTO Usuarios (UsuarioId, NombreUsuario, EstaActivo, ZonaId, FechaCreacion, Observaciones) VALUES (@UsuarioId, @NombreUsuario, @EstaActivo, @ZonaId, @FechaCreacion, @Observaciones)",
                    new Parametro("@UsuarioId", nuevoUsuarioId),
                    new Parametro("@NombreUsuario", "nuevo.usuario"),
                    new Parametro("@EstaActivo", true),
                    new Parametro("@ZonaId", 3),
                    new Parametro("@FechaCreacion", DateTime.Now),
                    new Parametro("@Observaciones", "Nuevo usuario agregado"));
        }

        private void DesactivarUsuario3()
        {
            var filasUsuario3Desactivado =
                _bd.Ejecutar(
                    "UPDATE Usuarios SET EstaActivo = @EstaActivoNo WHERE UsuarioId = @UsuarioId",
                    new Parametro("@EstaActivoNo", false), 
                    new Parametro("@UsuarioId", Guid.Parse("d9a55dc9-2efa-424e-a955-f9078250c656"))
                );
        }

        private void ActivarUsuarios()
        {
            var filasUsuariosActivados =
                _bd.Ejecutar(
                    "UPDATE Usuarios SET EstaActivo = @EstaActivoSi WHERE EstaActivo = @EstaActivoNo",
                    new Parametro("@EstaActivoSi", true), 
                    new Parametro("@EstaActivoNo", false));
        }

        private void ObtenerUsuario2()
        {
            var usuario2 =
                _bd.Obtener<Usuario>(
                    "SELECT * FROM Usuarios WHERE UsuarioId = @UsuarioId",
                    MapearUsuario,
                    new Parametro("@UsuarioId", Guid.Parse("1875203d-5511-46d8-b5aa-a629c32bbddd"))
                );
        }

        private void ObtenerUsuario1()
        {
            var usuario1 =
                _bd.Obtener<Usuario>(
                    "SELECT * FROM Usuarios WHERE NombreUsuario = @NombreUsuario",
                    MapearUsuario,
                    new Parametro("@NombreUsuario", "usuario.1"));
        }

        private void ObtenerListaUsuariosDeZona2()
        {
            var zona2 =
                _bd.ObtenerLista<Usuario>(
                    "SELECT * FROM Usuarios WHERE ZonaId = @ZonaId",
                    MapearUsuario,
                    new Parametro("@ZonaId", 2));
        }

        private void ObtenerDataTableUsuariosDeZona1()
        {
            var zona1 =
                _bd.ObtenerDataTable(
                    "SELECT * FROM Usuarios WHERE ZonaId = @ZonaId",
                    new Parametro("@ZonaId", 1));
        }

        private void ObtenerDataTableUsuariosActivasConZonas()
        {
            var usuarios =
                _bd.ObtenerDataTable(
                    "SELECT U.*, Z.Nombre as ZonaNombre FROM Usuarios AS U JOIN Zonas AS Z ON U.ZonaId = Z.ZonaId WHERE U.EstaActivo = @EstaActivo",
                    new Parametro("@EstaActivo", true));
        }

        private void ContarUsuariosActivos()
        {
            var cantTotalActivos =
                _bd.Obtener(
                    "SELECT COUNT(*) FROM Usuarios WHERE EstaActivo = @EstaActivo",
                    new Parametro("@EstaActivo", true));
        }

        private void ObtenerListaIdsDeUsuariosActivosDeZona2()
        {
            var activosZona2 =
                _bd.ObtenerLista<Guid>(
                    "SELECT UsuarioId FROM Usuarios WHERE EstaActivo = @EstaActivo AND ZonaId = @ZonaId",
                    (lector) => (Guid)lector["UsuarioId"],
                    new Parametro("@EstaActivo", true),
                    new Parametro("@ZonaId", 2));
        }

        private void ContarUsuarios()
        {
            var cantTotal1 = _bd.Obtener("SELECT COUNT(*) FROM Usuarios");
        }

        private Usuario MapearUsuario(Lector lector)
        {
            return new Usuario
            {
                UsuarioId = (Guid)lector["UsuarioId"],
                NombreUsuario = Convert.ToString(lector["NombreUsuario"]),
                EstaActivo = Convert.ToBoolean(lector["EstaActivo"]),
                ZonaId = Convert.ToByte(lector["ZonaId"]),
                FechaCreacion = Convert.ToDateTime(lector["FechaCreacion"]),
                Observacion = (string)lector["Observaciones"]
            };
        }

        private Usuario MapearUsuarioConZona(Lector lector)
        {
            var usuario = new Usuario
            {
                UsuarioId = (Guid)lector["UsuarioId"],
                NombreUsuario = Convert.ToString(lector["NombreUsuario"]),
                EstaActivo = Convert.ToBoolean(lector["EstaActivo"]),
                ZonaId = Convert.ToByte(lector["ZonaId"]),
                FechaCreacion = Convert.ToDateTime(lector["FechaCreacion"]),
                Observacion = (string)lector["Observaciones"]
            };

            usuario.Zona = new Zona
            {
                ZonaId = usuario.ZonaId,
                Nombre = Convert.ToString(lector["ZonaNombre"])
            };
            
            return usuario;
        }

        public override void Dispose()
        {
            if (_bd != null)
                _bd.Dispose();

            base.Dispose();
        }
    }
}