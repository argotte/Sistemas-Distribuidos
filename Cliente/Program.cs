﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ClienteSocket.ProgramPractica03;
class Cliente
{
    public static StreamWriter writer = new ("archivoSalida.txt");
    /// <summary>
    /// Función principal del cliente que presenta un menú con tres opciones y permite al usuario seleccionar una opción ingresando el número correspondiente en la consola.
    /// </summary>
    /// <param name="args"></param>
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
                Console.WriteLine("║ 4) Enviar Mensaje a Cliente                         ║");
                Console.WriteLine("║                                                     ║");
                Console.WriteLine("║ 5) Recibir Mensaje de Cliente                       ║");
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
                            Integridad("127.0.0.1", 5000);
                            break;
                        case 4:
                            Console.WriteLine("\nOpcion de Envio de Mensajes con Cliente seleccionada");
                            Console.WriteLine("Introduzca la direccion ip del cliente:");
                            string ipAddr = Console.ReadLine();
                            Console.WriteLine("Introduzca el puerto: ");
                            string portNum = Console.ReadLine();
                            ClienteMensajesConnect(ipAddr, int.Parse(portNum));
                            break;
                        case 5:
                            Console.WriteLine("\nOpcion de Recepcion de Mensajes con Cliente seleccionada");
                            Console.WriteLine("Introduzca el puerto: ");
                            string portNumR = Console.ReadLine();
                            ClienteMensajesListen(IPAddress.Any.ToString(), int.Parse(portNumR));
                            break;
                        case 0:
                            Console.WriteLine("\nSaliendo del programa...");
                            writer.Close();
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
                Console.ReadLine();
            }

            Console.Clear();
            Console.WriteLine("\nPrograma finalizado");
            Console.ReadLine();
        }

    /// <summary>
    /// Función que maneja la lógica de la opción "Firmar" del menú del cliente. Se conecta al servidor remoto a través de un socket TCP/IP, autentica al usuario y firma el texto ingresado por el usuario.
    /// </summary>
    /// <param name="ipAddr">Dirección IP del servidor.</param>
    /// <param name="portNum">Número de puerto utilizado por el servidor.</param>
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
            else
                Console.WriteLine($"Clave: {response}");
            writer.WriteLine(response);

            //Firmar texto
            Console.WriteLine("Escriba su texto a firmar: ");
            string texto = Console.ReadLine();
            // Devuelve la firma
            string firma = SignText(texto, response);
            Console.WriteLine($"Firma: {firma}");
            writer.WriteLine(firma);

            // Guardar la firma en un archivo de texto en la carpeta de documentos del usuario
            Console.WriteLine("¿Desea guardar la firma en un archivo de texto? [y/n]");
            string respuesta = Console.ReadLine();

            if (respuesta == "y" || respuesta == "Y")
            {
                // Obtener la ruta de la carpeta de documentos del usuario actual
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // Combinar la ruta de la carpeta de documentos con el nombre de la carpeta y el archivo de firma
                string folderPath = Path.Combine(documentsPath, "psd_2023");
                string filePath = Path.Combine(folderPath, "firmas.txt");

                // Crear la carpeta si no existe
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Crear o abrir el archivo de texto para escribir la firma
                StreamWriter writer = new StreamWriter(filePath, true);

                try
                {
                    // Escribir la firma en el archivo
                    writer.WriteLine(firma);
                    Console.WriteLine($"Firma guardada en {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al guardar la firma: {ex.Message}");
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                }
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
            CloseConnection(sender);
        }
    }

    /// <summary>
    /// Función que maneja la lógica de la opción "Autenticar" del menú del cliente. Se conecta al servidor remoto a través de un socket TCP/IP y autentica al usuario.
    /// </summary>
    /// <param name="ipAddr">Dirección IP del servidor.</param>
    /// <param name="portNum">Número de puerto utilizado por el servidor.</param>
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
                writer.WriteLine(authResponse ? "VALIDO" : "INVALIDO");

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

    /// <summary>
    /// Función que maneja la lógica de la opción "Integridad" del menú del cliente. Se conecta al servidor remoto a través de un socket TCP/IP y verifica la integridad del archivo especificado por el usuario.
    /// </summary>
    /// <param name="ipAddr">Dirección IP del servidor.</param>
    /// <param name="portNum">Número de puerto utilizado por el servidor.</param>
    public static void Integridad(string? ipAddr, int portNum)
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

                // Recibir inputs
                Console.WriteLine("Ingrese su clave: ");
                string? clave = Console.ReadLine();
                Console.WriteLine("Ingrese el texto del mensaje: ");
                string? texto = Console.ReadLine();
                Console.WriteLine("Ingrese el texto del mensaje firmado: ");
                string? textoFirmado = Console.ReadLine();
                
                //Mostrar y escribir en archivo de salida
                Console.WriteLine( VerifyText(texto, textoFirmado, clave) ? "MENSAJE INTEGRO": "MENSAJE NO INTEGRO");
                writer.WriteLine( VerifyText(texto, textoFirmado, clave) ? "INTEGRO" : "NO INTEGRO");
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

    /// <summary>
    /// Función que envía una solicitud de autenticación al servidor remoto y devuelve un valor booleano que indica si el usuario está autenticado o no.
    /// </summary>
    /// <param name="sender">Socket que representa la conexión con el servidor.</param>
    /// <param name="username">Nombre de usuario del cliente.</param>
    /// <param name="password">Contraseña del cliente.</param>
    /// <returns>Un valor booleano que indica si el usuario está autenticado o no.</returns>
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

    /// <summary>
    /// Función que envía una solicitud de clave al servidor remoto y devuelve la respuesta.
    /// </summary>
    /// <param name="sender">Socket que representa la conexión con el servidor.</param>
    /// <param name="username">Nombre de usuario del cliente.</param>
    /// <returns>La respuesta del servidor.</returns>
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

    /// <summary>
    /// Función que firma un texto utilizando la clave privada del usuario y devuelve la firma.
    /// </summary>
    /// <param name="texto">Texto a firmar.</param>
    /// <param name="clave">Clave privada del usuario.</param>
    /// <returns>La firma del texto.</returns>
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

    /// <summary>
    /// Verifica la integridad de un texto firmado utilizando la clave del usuario.
    /// </summary>
    /// <param name="texto">El texto original.</param>
    /// <param name="textoFirmado">El texto firmado que se va a verificar.</param>
    /// <param name="clave">La clave del usuario utilizada para firmar el texto original.</param>
    /// <returns>Un valor booleano que indica si el texto firmado es válido o no.</returns>
    private static bool VerifyText(string texto, string textoFirmado, string clave)
    {
            byte[] mensajeBytes = Encoding.UTF8.GetBytes(texto);
            byte[] claveBytes = Encoding.UTF8.GetBytes(clave);

            using (HMACSHA256 hmac = new HMACSHA256(claveBytes))
            {
                byte[] computedHash = hmac.ComputeHash(mensajeBytes);
                string firma = BitConverter.ToString(computedHash).Replace("-", "");
                return String.Equals(textoFirmado, firma);
            }
    }

        public static void ClienteMensajesConnect(string? ipAddr, int portNum)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ipAddr!), portNum);
            // Crear un socket TCP/IP
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int cont = 0;
            bool blockingState = true;
            while (blockingState && cont++ < 5) 
            {
                try
                {
                    sender.Connect(endpoint);
                    Console.WriteLine("Conexión establecida con el servidor.");
                    HandleClient(sender);
                    blockingState = false;
                }
                catch (SocketException)
                {
                    Console.WriteLine($"Intento {cont} de conexion fallido");
                    Thread.Sleep(2000);
                }
            }
        }
        
        public static void ClienteMensajesListen(string? ipAddr, int portNum)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ipAddr!), portNum);
            
            // Crear un socket TCP/IP
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Vincular el socket a un endpoint y escuchar por conexiones
            listener.Bind(endpoint);
            listener.Listen(2);
            Socket handler = listener.Accept();
            HandleClientIncoming(handler);
            CloseConnection(handler);
        }

        private static void HandleClientIncoming(Socket handler)
        {
            while (true)
            {
                // Recibir la respuesta del servidor y mostrarla en la consola
                byte[] responseBytes = new byte[1024];
                int bytesRec = handler.Receive(responseBytes);
                string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);
                string[] words = response.Split("\n");
                
                if (response == "EOF")
                {
                    CloseConnection(handler);
                    break;
                }
                
                Console.WriteLine("Respuesta Recibida del cliente: " + words[0]);
                Console.WriteLine(VerifyText(words[0], words[2], words[1]) ? "MENSAJE AUTENTICO" : "MENSAJE NO AUTENTICO");

                Console.WriteLine("Introduzca mensaje para el otro cliente: ");
                string msg = Console.ReadLine();
                if (msg == "EOF")
                {
                    CloseConnection(handler);
                    break;
                }
                byte[] messageBytes = Encoding.ASCII.GetBytes(msg);
                handler.Send(messageBytes);
            }
        }

        private static void HandleClient(Socket handler)
        {
            while (true)
            {
                Console.Write("Introduzca mensaje para el otro cliente: ");
                string msg = Console.ReadLine();
                Console.Write("Introduzca su clave: ");
                string clave = Console.ReadLine();
                //aqui se deberia autenticar pero deberiamos implementar un estado mejor
                string firma = SignText(msg, clave);

                string msjSend = msg + "\n" + clave + "\n" + firma;

                if (msg == "EOF")
                {
                    CloseConnection(handler);
                    break;
                }
                
                handler.Send(Encoding.ASCII.GetBytes(msjSend));

                // Recibir la respuesta del servidor y mostrarla en la consola
                byte[] responseBytes = new byte[1024];
                int bytesRec = handler.Receive(responseBytes);
                string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRec);

                if (response == "EOF")
                {
                    CloseConnection(handler);
                    break;
                }
                
                Console.WriteLine("Respuesta Recibida del cliente: " + response);
            }
        }

    /// <summary>
    /// Función que cierra la conexión con el servidor remoto.
    /// </summary>
    /// <param name="clientSocket">Socket que representa la conexión con el servidor.</param>
    public static void CloseConnection(Socket clientSocket)
        {
            bool _clientRunning = false;
            byte[] messageBytes = Encoding.ASCII.GetBytes("EOF");
            clientSocket.Send(messageBytes);
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }