using System;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace ProyectoConsola.Model
{
    /// <summary>
    /// Clase que representa un contexto de usuarios. Permite buscar y agregar usuarios a partir de un archivo de texto.
    /// </summary>
    public class UserContext
    {
        private readonly string _filePath;
        private readonly object _lockObj = new object();

        /// <summary>
        /// Crea una nueva instancia de la clase UserContext.
        /// </summary>
        /// <param name="filePath">Ruta del archivo de texto donde se guardan los usuarios.</param>
    public UserContext(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Busca un usuario por su nombre de usuario. Este método está diseñado para ser seguro para su uso en entornos de concurrencia.
        /// </summary>
        /// <param name="userName">Nombre de usuario a buscar.</param>
        /// <returns>El usuario encontrado o null si no se encontró ningún usuario con ese nombre de usuario.</returns>
        public User FindUser(string userName)
        {
            lock (_lockObj) // Se utiliza un objeto de bloqueo para garantizar la exclusión mutua en el acceso al archivo.
            {
                if (!File.Exists(_filePath)) File.CreateText(_filePath);
                string[] lines = File.ReadAllLines(_filePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    if (parts[0] == userName)
                    {
                        return new User { UserName = parts[0], Clave = parts[1] };
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Agrega un usuario al archivo de texto. Este método está diseñado para ser seguro para su uso en entornos de concurrencia.
        /// </summary>
        /// <param name="user">Usuario a agregar.</param>
        public void AddUser(User user)
        {
            lock (_lockObj) // Se utiliza un objeto de bloqueo para garantizar la exclusión mutua en el acceso al archivo.
            {
                using (StreamWriter writer = (File.Exists(_filePath) ? File.AppendText(_filePath) : File.CreateText(_filePath)))
                {
                    writer.WriteLine(user.UserName + "," + user.Clave);
                }
            }
        }
    }
}