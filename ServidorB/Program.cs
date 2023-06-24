using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ProyectoConsola.Model;

namespace ServidorSocket
{


    class Servidor
    {
        //inyección de dependencias para ser usadas 
        private static bool _serverRunning = false; // pasa estar en el ciclo infinito
        private static readonly UserContext _dbContext = new(); // instanca al DbContext ubicado en proyectoConsola 

        //clase main donde se inicia el proyecto A
        static void Main(string[] args)
        {
            StartServer(); // inicio dle server 
        }

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

        //metodo que controla la conexión con el cliente recibe los mensaje e envía mensaje, recibe un objeto socket TCP
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
                                                                          // Console.WriteLine("Mensaje recibido del cliente: " + data); // mostrar en consola , se puede quitar cuando esté listo el proyecto

                    if (data.IndexOf("<EOF>") > -1) // sale del while si se recibe como indice <EOF>
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
        //metodo que recibe el nombre de usuario y el socket del cliente tambien el mensaje que se pasó ya deserealizado
        //realiza una conexión a la BD y comprueba que que existe el user de no existir lo crea en la BD
        public static void HandleClavesConn(Socket sender, string data)
        {
            // Separar el nombre de usuario y la contraseña
            string[] parts = data.Split(' ');
            string username = parts[0];
            string password = parts[1];

            // Buscar el usuario en la base de datos
            var usuario = _dbContext.Users.FirstOrDefault(u => u.UserName == username);
           
            byte[] messageBytes = Encoding.ASCII.GetBytes(usuario != null && usuario.Clave == password ? "1" : "0");
            sender.Send(messageBytes);
        }
        
        //metodo para cerrar la conexión de algún cliente, recibe un socket y luego cierra la conexión del mismo
        public static void CloseConnection(Socket clientSocket)
        {
            bool _clientRunning = false;
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        //metodo para conocer la IP local del server, muestra la IP del puerto ethernet
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

        //metodo para conocer la IP local del server, muestra la IP del puerto WIFI ethernet 

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