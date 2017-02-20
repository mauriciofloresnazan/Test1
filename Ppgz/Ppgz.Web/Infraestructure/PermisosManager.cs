namespace Ppgz.Web.Infraestructure
{
    public static class PermisosManager
    {
        public static string[] NazanAccessos
        {
            get
            {
                return new[]{
                    "*FULL*",
                    "ProveedorAdministrarUsuarios",
                    "ProveedorMensajesInstitucionales"
                };
            }
        }

    }
}