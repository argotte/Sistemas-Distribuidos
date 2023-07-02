using System.Net;
using System.Net.Sockets;
using System.Text;
using ProyectoConsola.Model;

namespace ServidorA
{
    class Servidor
    {
        private static bool _serverRunning = false;
        //acá la ruta del archivo TXT 
        private static readonly UserContext _userContext = new("/home/jorgegetsmad/RiderProjects/SistemasDistribuidosProyecto/ServidorB/bin/Debug/net6.0/Usuarios.txt");

        static void Main(string[] args)
        {
            StartServer();
        }

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

        public static void CloseConnection(Socket clientSocket)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }    
}