using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClienteSocket.ProgramPractica03
{
    class Cliente
    {
        static void Main(string[] args)
        {
            try
            {
                while (true)
                {
                    // Console.Write("Introduzca la direccion IP del servidor (-1 para salir): ");
                    // string? ipAddress = Console.ReadLine();
                    //
                    // if (ipAddress == "-1")
                    //     break;
                    //
                    // Console.Write("Introduzca el numero de puerto: ");
                    // string? portNumStr = Console.ReadLine();
                    //
                    // int portNumInt;
                    // int.TryParse(portNumStr, out portNumInt);
                    //
                    // StartClient(ipAddress, portNumInt);
                    StartClient("10.12.35.104", 5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            // Esperar a que el usuario presione Enter para salir
            // Console.WriteLine("Presione Enter para salir.");
            // Console.ReadLine();
            Console.WriteLine("Programa finalizado");
        }

        public static void StartClient(string? ipAddr, int portNum)
        {
            // Establecer el endpoint para el socket
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ipAddr!), portNum);

            // Crear un socket TCP/IP
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Conectar el socket al endpoint remoto
                sender.Connect(endpoint);
                Console.WriteLine("Conexión establecida con el servidor.");

                while (true)
                {
                    // Leer input del usuario desde la consola
                    Console.WriteLine("Ingrese un mensaje para el servidor('quit' para salir): ");
                    string message = Console.ReadLine();

                    // Si el mensaje es "x", salir del ciclo
                    if (message == "quit")
                        break;

                    // Obtener la hora actual del cliente
                    DateTime clientTime = DateTime.Now;

                    // Agregar la hora actual del cliente al mensaje
                    message = clientTime + ";" + message;

                    // Enviar el mensaje al servidor
                    byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                    sender.Send(messageBytes);

                    // Recibir la respuesta del servidor y mostrarla en la consola
                    byte[] responseBytes = new byte[1024];
                    int bytesRec = sender.Receive(responseBytes);
                    string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);
                    Console.WriteLine(response);

                    // Enviar la diferencia de tiempo entre el cliente y el servidor al servidor
                    TimeSpan timeDiff = DateTime.Now - clientTime;
                    byte[] timeDiffBytes = Encoding.ASCII.GetBytes(timeDiff.TotalMilliseconds.ToString());
                    sender.Send(timeDiffBytes);
                }
            }
            catch (SocketException socketEx)
            {
                Console.WriteLine($"SOCKET ERROR: {socketEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Cerrar el socket
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
        }
    }
}
