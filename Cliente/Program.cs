using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ClienteSocket.ProgramPractica03;
class Cliente
    {
        static void Main(string[] args)
        {
            int opcion = -1;
            while (opcion != 0)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════════════════╗");
                Console.WriteLine("║                 Seleccione una opcion:              ║");
                Console.WriteLine("╠═══════════════════════════════════════════════════╣");
                Console.WriteLine("║                                                     ║");
                Console.WriteLine("║ 1) Firmar                                           ║");
                Console.WriteLine("║                                                     ║");
                Console.WriteLine("║ 2) Autenticar                                       ║");
                Console.WriteLine("║                                                     ║");
                Console.WriteLine("║ 3) Integridad                                       ║");
                Console.WriteLine("║                                                     ║");
                Console.WriteLine("║ 0) Salir                                            ║");
                Console.WriteLine("║                                                     ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════╝");
                Console.Write("Opcion: ");

                try
                {
                    opcion = Convert.ToInt32(Console.ReadLine());

                    switch (opcion)
                    {
                        case 1:
                            Console.WriteLine("\nOpcion de clave seleccionada");
                            Clave("127.0.0.1", 5000);
                            break;
                        case 2:
                            Console.WriteLine("\nOpcion de autenticar seleccionada");
                            Autenticar("127.0.0.1", 5000);
                            break;
                        case 3:
                            Console.WriteLine("\nOpcion de integridad seleccionada");
                            break;
                        case 0:
                            Console.WriteLine("\nSaliendo del programa...");
                            break;
                        default:
                            Console.WriteLine("\nOpcion no valida");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nError: " + e.Message);
                }

                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
            }

            Console.Clear();
            Console.WriteLine("\nPrograma finalizado");
            Console.ReadKey();
        }


        public static void Clave(string? ipAddr, int portNum)
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

                // Clave privada usuario
                Console.WriteLine("Ingrese su nombre de usuario: ");
                string username = Console.ReadLine();
                string? clave;
                string response = SendKeyRequest(sender, username);
                if (response == "Usuario ya registrado")
                {
                    Console.WriteLine($"Introduzca su clave: ");
                    clave = Console.ReadLine();
                    if (SendAuthRequest(sender, username, clave))
                        response = clave;
                    else
                    {
                        Console.WriteLine("Clave no Valida. Intente Nuevamente");
                        return;
                    }
                }
                Console.WriteLine($"Clave: {response}");

                //Firmar texto
                Console.WriteLine("Escriba su texto a firmar: ");
                string texto = Console.ReadLine();
                // Devuelve la firma
                string firma = SignText(texto, response);
                Console.WriteLine($"Firma: {firma}");
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
                CloseConnection(sender);
            }
        }

        public static void Autenticar(string? ipAddr, int portNum)
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
                Console.WriteLine("Ingrese su nombre de usuario a autenticar: ");
                string? username = Console.ReadLine();
                Console.WriteLine("Ingrese la clave del usuario a autenticar: ");
                string? clave = Console.ReadLine();

                bool authResponse = SendAuthRequest(sender, username, clave);
                Console.WriteLine(authResponse ? "VALIDO" : "INVALIDO");

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
                CloseConnection(sender);
            }
        }

        private static bool SendAuthRequest(Socket sender, string username, string clave)
        {
            //Enviar mensaje al servidor
            string message = $"AUTENTICAR\n{username}\n{clave}";
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            //Recibir mensaje servidor
            sender.Send(messageBytes);
            byte[] responseBytes = new byte[1024];
            int bytesRec = sender.Receive(responseBytes);
            string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);
            return response == "1";
        }

        private static string SendKeyRequest(Socket sender, string? username)
        {
            //Enviar mensaje al servidor
            string message = $"CLAVE\n{username}";
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            //Recibir mensaje servidor
            sender.Send(messageBytes);
            byte[] responseBytes = new byte[1024];
            int bytesRec = sender.Receive(responseBytes);
            string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);
            return response;
        }

        private static string SignText(string texto, string clave)
        {
            // Convierte el mensaje y la clave a arreglos de bytes
            byte[] mensajeBytes = Encoding.UTF8.GetBytes(texto);
            byte[] claveBytes = Encoding.UTF8.GetBytes(clave);

            // Crea un objeto HMACSHA256 usando la clave
            HMACSHA256 hmac = new HMACSHA256(claveBytes);

            // Calcula el hash del mensaje usando el hmac
            byte[] hash = hmac.ComputeHash(mensajeBytes);

            // Convierte el hash a un string hexadecimal
            string firma = BitConverter.ToString(hash).Replace("-", "");
            return firma;
        }


        public static void CloseConnection(Socket clientSocket)
        {
            bool _clientRunning = false;
            byte[] messageBytes = Encoding.ASCII.GetBytes("EOF");
            clientSocket.Send(messageBytes);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }