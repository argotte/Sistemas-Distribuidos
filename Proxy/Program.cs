using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ServidorSocket;
    class Servidor
    {
        private static bool _serverRunning = false;

        static void Main(string[] args)
        {
            StartServer();
        }

        public static void StartServer()
        {
            // Establecer el endpoint para el socket
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 5000);

            // Crear un socket TCP/IP
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Vincular el socket a un endpoint y escuchar por conexiones
            listener.Bind(endpoint);
            listener.Listen(2);

            _serverRunning = true;

            Console.WriteLine("Servidor Proxy. Esperando conexiones...");
            // Console.WriteLine($"Dirección Ip Ethernet del servidor: {GetLocalIPAddress()} ");
            // Console.WriteLine($"Dirección Ip WIFI del servidor: {GetWifiIPAddress()} ");

            Console.ReadKey();
            
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

        public static void HandleAuthConn(Socket sender)
        {
            // Enviar el mensaje al servidor
            byte[] messageBytes = Encoding.ASCII.GetBytes("Hello from proxy server");
            sender.Send(messageBytes);

            // Recibir la respuesta del servidor y mostrarla en la consola
            byte[] responseBytes = new byte[1024];
            int bytesRec = sender.Receive(responseBytes);
            string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);
            Console.WriteLine(response);
        }
        
        public static void HandleClavesConn(Socket sender, Socket handlerServer, string clave)
        {
            // Enviar el mensaje al servidor
            byte[] messageBytes = Encoding.ASCII.GetBytes(clave);
            sender.Send(messageBytes);

            // Recibir la respuesta del servidor y mostrarla en la consola
            byte[] responseBytes = new byte[1024];
            int bytesRec = sender.Receive(responseBytes);
            string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);
            SendToClient(handlerServer, response);
        }

        public static void HandleClient(Socket handler)
        {
            bool _clientRunning = true; // variable para indicar si el cliente está conectado

            try
            {
                byte[] buffer = new byte[1024];
                string data = null;

                while (_clientRunning) // verificar si el cliente está conectado
                {
                    if (!_clientRunning) // verificar si el cliente está conectado
                    {
                        break;
                    }

                    int bytesRec = handler.Receive(buffer);
                    data += Encoding.ASCII.GetString(buffer, 0, bytesRec);
                    Console.WriteLine("Mensaje recibido del cliente: \n" + data);
                    string[] words = data.Split("\n");
                    string firstWord = words[0];

                    if (firstWord == "FIRMAR")
                    {
                        //Conectar con Servidor Claves puerto 5003
                        IPEndPoint endpointClaves = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5002);
                        Socket senderClaves = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        senderClaves.Connect(endpointClaves);

                        Thread threadClaves = new Thread(() => HandleClavesConn(senderClaves, handler,  words[1]));
                        threadClaves.Start();
                    }
                    
                    else if (firstWord == "AUTENTICAR")
                    {
                        //Conectar con Servidor Autenticacion puerto 5002
                        IPEndPoint endpointAuth = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5003);
                        Socket senderAuth = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        senderAuth.Connect(endpointAuth);
            
                        Thread threadAuth = new Thread(() => HandleAuthConn(senderAuth));
                        threadAuth.Start();
                    }
                    else if (firstWord == "INTEGRIDAD")
                    {
                        //INTEGRIDAD accion
                    }
                    
                    if (data.IndexOf("<EOF>") > -1)
                        break;

                }
                CloseConnection(handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void SendToClient(Socket clientSocket, string message)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            clientSocket.Send(messageBytes);
        }

        public static void CloseConnection(Socket clientSocket)
        {
            bool _clientRunning = false;
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

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