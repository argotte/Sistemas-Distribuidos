using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ProyectoConsola.Model;

namespace ServidorB
{
    /// <summary>
    /// Clase que representa el servidor de autenticación. Espera conexiones de clientes y verifica las credenciales de los usuarios.
    /// </summary>
    class Servidor
    {
        //inyección de dependencias para ser usadas 
        private static bool _serverRunning = false; // pasa estar en el ciclo infinito
        //private static readonly UserContext _userContext = new("/home/jorgegetsmad/RiderProjects/SistemasDistribuidosProyecto/ServidorB/bin/Debug/net6.0/Usuarios.txt");
        private static readonly UserContext _userContext = new("C:\\Users\\Daniel Toro\\Documents\\universidad\\Sistemas Distribuidos\\proyecto\\Usuarios.txt");

        //clase main donde se inicia el proyecto A
        static void Main(string[] args)
        {
            StartServer(); // inicio dle server 
        }

        /// <summary>
        /// Inicia el servidor de autenticación. Espera conexiones de clientes y verifica las credenciales de los usuarios.
        /// </summary>
        public static void StartServer()
        {
            // Establecer el endpoint para el socket se acordó el puerto 5002 para el servidor de claves
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 5003); // 

            // Crear un socket TCP/IP
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Vincular el socket a un endpoint y escuchar por conexiones
            listener.Bind(endpoint);
            listener.Listen(2);

            _serverRunning = true; // para entrar al ciclo infinito 

            Console.WriteLine("Servidor Auth. Esperando conexiones...");
            // Console.WriteLine($"Dirección Ip Ethernet del servidor: {GetLocalIPAddress()} ");
            // Console.WriteLine($"Dirección Ip WIFI del servidor: {GetWifiIPAddress()} ");

            try
            {
                while (_serverRunning)
                {
                    // Esperar por una conexión
                    Socket handler = listener.Accept();

                    Console.WriteLine("Conexión aceptada desde " + handler.RemoteEndPoint);

                    // Iniciar un nuevo hilo para manejar la conexión del cliente
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
                // Cerrar el socket al terminar
                listener.Close();
            }
        }


        /// <summary>
        /// Maneja la conexión de un cliente. Espera a recibir las credenciales del cliente (nombre de usuario y contraseña) y verifica si son válidas.
        /// </summary>
        /// <param name="handler">Socket que representa la conexión con el cliente.</param>
        public static void HandleClient(Socket handler)
        {
            bool clientRunning = true; // para el ciclo infinito

            try
            {
                byte[] buffer = new byte[1024]; //codificación para el bufer 
                string data = ""; // la data se inicializa en blanco

                while (clientRunning)
                {
                    if (!clientRunning)
                    {
                        break; // para salir dle while en caso de que sea falso 
                    }

                    int bytesRec = handler.Receive(buffer); // metodo que recibe el paquete enviado desde el socket cliente
                    data = Encoding.ASCII.GetString(buffer, 0, bytesRec); //deserealizando el paquete e introduciendolo a variable data definida arriba
                    Console.WriteLine("Mensaje recibido del cliente: " + data); // mostrar en consola , se puede quitar cuando esté listo el proyecto

                    if (data.IndexOf("EOF") > -1) // sale del while si se recibe como indice <EOF>
                    {
                        break;
                    }
                    if (data == "cerrar") // si recibe cerrar cierra la conexión del cliente que envío la frase 
                    {
                        CloseConnection(handler);
                         Console.WriteLine("Conexión cerrada exitosamente");
                        break;
                    }
                    else
                    {
                        HandleClavesConn(handler, data); // metodo que revisa el nombreUSURIO 
                    }
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Maneja la conexión de un cliente y verifica si las credenciales del usuario son válidas.
        /// </summary>
        /// <param name="sender">Socket que representa la conexión con el cliente.</param>
        /// <param name="data">Credenciales del usuario (nombre de usuario y contraseña).</param>
        public static void HandleClavesConn(Socket sender, string data)
        {
            string[] credentials = data.Split("\n");
            string username = credentials[0];

            User user = _userContext.FindUser(username);
            string response;
            if (user != null && user.Clave == credentials[1]) response = "1";
            else response = "0";
            byte[] messageBytes = Encoding.ASCII.GetBytes(response);
            sender.Send(messageBytes);
        }


        /// <summary>
        /// Cierra la conexión con un cliente.
        /// </summary>
        /// <param name="clientSocket">Socket que representa la conexión con el cliente.</param>
        public static void CloseConnection(Socket clientSocket)
        {
            bool _clientRunning = false;
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        /// <summary>
        /// Obtiene la dirección IP local del servidor.
        /// </summary>
        /// <returns>La dirección IP local del servidor.</returns>
        public static string GetLocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }

            return localIP;
        }

        /// <summary>
        /// Obtiene la dirección IP local del servidor a través de la interfaz de red inalámbrica (Wi-Fi).
        /// </summary>
        /// <returns>La dirección IP local del servidor a través de la interfaz de red inalámbrica (Wi-Fi).</returns>
        public static string GetWifiIPAddress()
        {
            var wifiInterface = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && i.OperationalStatus == OperationalStatus.Up);

            if (wifiInterface == null)
                return null;

            var ipProps = wifiInterface.GetIPProperties();
            var wifiIp = ipProps.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            return wifiIp?.Address.ToString();
        }
    }
}