namespace ProyectoConsola.Model
{
    /// <summary>
    /// Clase que representa un usuario del sistema de autenticación.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Nombre de usuario del usuario.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Clave de acceso del usuario.
        /// </summary>
        public string? Clave { get; set; }
    }
}