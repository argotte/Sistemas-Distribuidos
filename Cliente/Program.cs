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
            int opcion = -1;

            while (opcion != 0)
            {
                Console.WriteLine("Seleccione una opcion:");
                Console.WriteLine("1) Clave");
                Console.WriteLine("2) Autenticar");
                Console.WriteLine("3) Integridad");
                Console.WriteLine("0) Salir");

                try
                {
                    opcion = Convert.ToInt32(Console.ReadLine());

                    switch (opcion)
                    {
                        case 1:
                            Console.WriteLine("Opcion de clave seleccionada");
                            CLAVE("127.0.0.1", 5000);
                            break;
                        case 2:
                            Console.WriteLine("Opcion de autenticar seleccionada");
                            break;
                        case 3:
                            Console.WriteLine("Opcion de integridad seleccionada");
                            break;
                        case 0:
                            Console.WriteLine("Saliendo del programa...");
                            break;
                        default:
                            Console.WriteLine("Opcion no valida");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
            Console.WriteLine("Programa finalizado");
        }


        public static void CLAVE(string? ipAddr, int portNum)
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
                string message = "CLAVE\njorge_ld";
                
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
                
                
                // Pedir clave con "CLAVE" seguido de "NOMBREUSUARIO" separados por \n
                
                //Pedir autenticacion "AUTENTICAR" seguido de "NOMBREUSUARIO" y "CONTRASEÑA" separados por \n
                
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
                CloseConnection(sender);
            }
        }
        
        public static void CloseConnection(Socket clientSocket)
        {
            bool _clientRunning = false;
            byte[] messageBytes = Encoding.ASCII.GetBytes("EOF");
            clientSocket.Send(messageBytes);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
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
        //                CLAVE("192.168.56.1", 5002);
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

        //    public static void CLAVE(string? ipAddr, int portNum)
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


//                Console.WriteLine("Conexión establecida con el servidor.");
                //Console.WriteLine("Ingrese su nombre de usuario de FIRMA:");
                //string username = Console.ReadLine();
                //Console.WriteLine("Ingrese su mensaje de FIRMA:");
                //string message = Console.ReadLine();
                //string usernamemessage = "FIRMA\n"+username + "\n" + message;
                //Console.WriteLine("Mensaje se enviará como:\n" + usernamemessage);