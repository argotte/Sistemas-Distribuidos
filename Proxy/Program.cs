﻿using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ServidorSocket
{

    /// <summary>
    /// Clase que representa un servidor proxy que maneja conexiones a otros servidores.
    /// </summary>
    class Servidor
    {
        private static bool _serverRunning = false;

        static void Main(string[] args)
        {
            StartServer();
        }

            /// <summary>
            /// Inicia el servidor proxy y escucha por conexiones entrantes.
            /// </summary>
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
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Cerrar el socket al terminar
                listener.Close();
            }
        }

            /// <summary>
            /// Maneja una conexión de autenticación con un servidor externo.
            /// </summary>
            /// <param name="sender">Socket que se utiliza para enviar mensajes al servidor externo.</param>
            /// <param name="handlerServer">Socket que se utiliza para enviar mensajes al cliente.</param>
            /// <param name="user">Nombre de usuario para autenticación.</param>
            /// <param name="clave">Clave de acceso para autenticación.</param>
            public static void HandleAuthConn(Socket sender,Socket handlerServer, string user, string clave)
            {
                    // Enviar el mensaje al servidor
                    byte[] messageBytes = Encoding.ASCII.GetBytes(user +"\n"+ clave);
                    sender.Send(messageBytes);

                    // Recibir la respuesta del servidor y mostrarla en la consola
                    byte[] responseBytes = new byte[1024];
                    int bytesRec = sender.Receive(responseBytes);
                    string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);
                    Console.WriteLine(response);
                    SendToClient(handlerServer, response);
            }

        /// <summary>
        /// Maneja una conexión con un servidor de claves externo.
        /// </summary>
        /// <param name="sender">Socket que se utiliza para enviar mensajes al servidor externo.</param>
        /// <param name="handlerServer">Socket que se utiliza para enviar mensajes al cliente.</param>
        /// <param name="clave">Clave a buscar en el servidor de claves.</param>
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

        /// <summary>
        /// Maneja la conexión de un cliente al servidor proxy.
        /// </summary>
        /// <param name="handler">Socket que se utiliza para comunicarse con el cliente.</param>
        public static void HandleClient(Socket handler)
        {
            bool _clientRunning = true; // variable para indicar si el cliente está conectado
            
            byte[] buffer = new byte[1024];
            string data = null;
            Socket? handlerActual = null;
            try
            {
                while (_clientRunning) // verificar si el cliente está conectado
                {
                    if (!_clientRunning) // verificar si el cliente está conectado
                        break;
                    
                    int bytesRec = handler.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, bytesRec);
                    Console.WriteLine("Mensaje recibido del cliente: \n" + data);
                    string[] words = data.Split("\n");
                    string firstWord = words[0];

                    if (firstWord == "CLAVE")
                    {
                        //Conectar con Servidor Claves puerto 5002
                        IPEndPoint endpointClaves = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5002);
                        handlerActual = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                            ProtocolType.Tcp);
                        handlerActual.Connect(endpointClaves);
                        HandleClavesConn(handlerActual, handler, words[1]);
                    }

                    else if (firstWord == "AUTENTICAR")
                    {
                        //Conectar con Servidor Autenticacion puerto 5003
                        IPEndPoint endpointAuth = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5003);
                        Socket senderAuth = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        senderAuth.Connect(endpointAuth);
                        HandleAuthConn(senderAuth,handler, words[1], words[2]);
                    }
                    if (data.IndexOf("EOF") > -1)
                    {
                        _clientRunning = false;
                        CloseConnection(handler);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                CloseConnection(handler);
            }
        }

        /// <summary>
        /// Envia un mensaje al cliente a través del socket.
        /// </summary>
        /// <param name="clientSocket">Socket que se utiliza para enviar el mensaje.</param>
        /// <param name="message">Mensaje a enviar.</param>
        public static void SendToClient(Socket clientSocket, string message)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            clientSocket.Send(messageBytes);
        }

        /// <summary>
        /// Cierra la conexión con el cliente.
        /// </summary>
        /// <param name="clientSocket">Socket que se utiliza para comunicarse con el cliente.</param>
        public static void CloseConnection(Socket clientSocket)
        {
            bool _clientRunning = false;
            byte[] messageBytes = Encoding.ASCII.GetBytes("EOF");
            clientSocket.Send(messageBytes);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }
}
