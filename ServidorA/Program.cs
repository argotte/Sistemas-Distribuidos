using System.Net;
using System.Net.Sockets;
using System.Text;
using ProyectoConsola.Model;

namespace ServidorA
{
    /// <summary>
    /// Clase que representa el servidor de claves. Espera conexiones de clientes y genera claves aleatorias para ellos.
    /// </summary>
    class Servidor
    {
        private static bool _serverRunning = false;
        //acá la ruta del archivo TXT 
        //private static readonly UserContext _userContext = new("/home/jorgegetsmad/RiderProjects/SistemasDistribuidosProyecto/ServidorB/bin/Debug/net6.0/Usuarios.txt");
        private static readonly UserContext _userContext = new("C:\\Users\\Daniel Toro\\Documents\\universidad\\Sistemas Distribuidos\\proyecto\\Usuarios.txt");

        static void Main(string[] args)
        {
            StartServer();
        }

            /// <summary>
            /// Inicia el servidor de claves. Espera conexiones de clientes y genera claves aleatorias para ellos.
            /// </summary>
        public static void StartServer()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 5002);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endpoint);
            listener.Listen(2);
            _serverRunning = true;

            Console.WriteLine("Servidor de Claves. Esperando conexiones...");

            try
            {
                while (_serverRunning)
                {
                    Socket handler = listener.Accept();
                    Console.WriteLine("\n--------------------------------------------------------\n");
                    Console.WriteLine("Conexión aceptada desde " + handler.RemoteEndPoint);
                    Thread thread = new Thread(() => HandleClient(handler));
                    thread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                listener.Close();
            }
        }

        /// <summary>
        /// Maneja la conexión de un cliente. Espera a recibir el nombre de usuario del cliente y genera una clave aleatoria para él.
        /// </summary>
        /// <param name="handler">Socket que representa la conexión con el cliente.</param>
        public static void HandleClient(Socket handler)
        {
            bool clientRunning = true;

            try
            {
                byte[] buffer = new byte[1024];
                string data = "";

                while (clientRunning)
                {
                    if (!clientRunning)
                    {
                        break;
                    }

                    int bytesRec = handler.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, bytesRec);
                    Console.WriteLine("Mensaje recibido del cliente: " + data);

                    if (data.IndexOf("<EOF>") > -1)
                        break;
                    
                    if (data == "cerrar")
                    {
                        CloseConnection(handler);
                        Console.WriteLine("Conexión cerrada exitosamente");
                        break;
                    }
                    else
                    {
                        HandleClavesConn(handler, data);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Maneja la conexión de un cliente y genera una clave aleatoria para él.
        /// </summary>
        /// <param name="sender">Socket que representa la conexión con el cliente.</param>
        /// <param name="data">Nombre de usuario del cliente.</param>
        public static void HandleClavesConn(Socket sender, string data)
        {
            var usuario = _userContext.FindUser(data);
            if (usuario != null)
            {
                byte[] messageBytes = Encoding.ASCII.GetBytes("Usuario ya registrado");
                sender.Send(messageBytes);
            }
            else
            {
                var clave = GenerarClaveAleatoria();
                usuario = new User { UserName = data, Clave = clave };
                _userContext.AddUser(usuario);
                byte[] messageBytes = Encoding.ASCII.GetBytes(clave);
                sender.Send(messageBytes);
            }
        }

        /// <summary>
        /// Genera una clave aleatoria de 8 caracteres, compuesta por letras mayúsculas, minúsculas y números.
        /// </summary>
        /// <returns>La clave aleatoria generada.</returns>
        private static string GenerarClaveAleatoria()
        {
            const string caracteresPermitidos = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var clave = new char[8];
            for (int i = 0; i < clave.Length; i++)
            {
                int index = random.Next(caracteresPermitidos.Length);
                clave[i] = caracteresPermitidos[index];
            }
            Console.WriteLine("\nSe le a creado la clave exitosamente");
            return new string(clave);
        }

        /// <summary>
        /// Cierra la conexión con un cliente.
        /// </summary>
        /// <param name="clientSocket">Socket que representa la conexión con el cliente.</param>
        public static void CloseConnection(Socket clientSocket)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }    
}