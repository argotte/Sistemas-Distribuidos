using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace ClienteSocket.ProgramPractica03
{
    class Cliente
    {
        static void Main(string[] args)
        {
            try
            {
                Console.ReadKey();
                StartClient("127.0.0.1", 5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
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

                // Leer input del usuario desde la consola
                string message = "INTEGRIDAD\njorge_ld8\nHola Mundo";
                
                string[] words = message.Split("\n");
                string firstWord = words[0];
                
                // Enviar el mensaje al servidor
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                sender.Send(messageBytes);
                // Recibir la respuesta del servidor y mostrarla en la consola
                byte[] responseBytes = new byte[1024];
                int bytesRec = sender.Receive(responseBytes);
                string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);
                Console.WriteLine(response);
                
                
                if (firstWord == "FIRMAR")
                {
                    byte[] sendFirmar = Encoding.ASCII.GetBytes(response);
                    sender.Send(sendFirmar);
                    //En response estará la clave de 8 digitos del usuario
                    //Flujo de firmar que termina con escritura de un archivo de salida
                }
                if (firstWord == "AUTENTICAR")
                {
                    //Si la respuesta es "1" es VALIDO, si la respuesta es "0" es INVALIDO
                    //Flujo de autenticar que termina con escritura de un archivo de salida
                }
                if (firstWord == "INTEGRIDAD")
                {
                    //el mensaje aleatorio que trae response sera usado y comparado con el HASH de response.
                    //Mensaje de ejemplo y response que seria la firma
                    //calculamos el hash del mensaje de ejemplo localmente
                    byte[] mensajeBytes = Encoding.ASCII.GetBytes("prueba");
                    byte[] hashBytes;
                    using(SHA256 sha256 = SHA256.Create()) 
                    {
                        hashBytes=sha256.ComputeHash(mensajeBytes);
                    }
                    string hash = BitConverter.ToString(hashBytes).Replace("-", "");

                    //Ahora compararemos aca el hash local con el hash de la firma
                    if(hash.Equals(response,StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("El mensaje es INTEGRO.");
                    }
                    else
                    {
                        Console.WriteLine("El mensaje NO es INTEGRO.");
                    }

                }
                Console.WriteLine("Presione una tecla para continuar");
                Console.ReadKey();
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


        //esta se usa cuando sea el servidor B, la tarea que realizaría el proxy que es redirigir dependiendo el caso
        //. 
        //class Cliente
        //{
        //    static void Main(string[] args)
        //    {
        //        try
        //        {
        //            while (true)
        //            {
        //                StartClient("192.168.56.1", 5002);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //        // Esperar a que el usuario presione Enter para salir
        //        // Console.WriteLine("Presione Enter para salir.");
        //        // Console.ReadLine();
        //        Console.WriteLine("Programa finalizado");
        //    }

        //    public static void StartClient(string? ipAddr, int portNum)
        //    {
        //        // Establecer el endpoint para el socket
        //        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ipAddr!), portNum);

        //        // Crear un socket TCP/IP
        //        Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //        try
        //        {
        //            // Conectar el socket al endpoint remoto
        //            sender.Connect(endpoint);
        //            Console.WriteLine("Conexión establecida con el servidor.");

        //            while (true)
        //            {
        //                // Leer el nombre de usuario desde la consola
        //                Console.WriteLine("Ingrese el nombre de usuario: ");
        //                string username = Console.ReadLine();

        //                // Leer la contraseña desde la consola
        //                Console.WriteLine("Ingrese la contraseña: ");
        //                string password = Console.ReadLine();

        //                // Concatenar el nombre de usuario y la contraseña en una sola cadena separada por un espacio
        //                string message = $"{username} {password}";

        //                // Enviar el mensaje al servidor
        //                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        //                sender.Send(messageBytes);

        //                // Limpiar la variable message
        //                message = "";

        //                // Recibir la respuesta del servidor y mostrarla en la consola
        //                byte[] responseBytes = new byte[1024];
        //                int bytesRec = sender.Receive(responseBytes);
        //                string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);
        //                Console.WriteLine(response);

        //            }
        //        }
        //        catch (SocketException socketEx)
        //        {
        //            Console.WriteLine($"SOCKET ERROR: {socketEx.Message}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //        finally
        //        {
        //            // Cerrar el socket
        //            sender.Shutdown(SocketShutdown.Both);
        //            sender.Close();
        //        }
        //    }
        //}
    }
}
