﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PROJEKT_CHAT1_CLIENT
{
    static class Program
    {
        static Socket socketClient = null;
        static IPAddress addressIP = null;
        static IPEndPoint iPEndPoint = null;

        static Form1 mainForm = null;
        
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new Form1(buttonConnectClick, buttonSendClick);
            Application.Run(mainForm);
        }

        static EventHandler buttonConnectClick = DoConnect;
        static EventHandler buttonSendClick = SendMessage;

        static void DoConnect(object sender, EventArgs e)
        {
            addressIP = IPAddress.Parse(mainForm.GetIPString());
            iPEndPoint = new IPEndPoint(addressIP, mainForm.GetPortNumber());

            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // Nawiązanie połączenia
                socketClient.Connect(iPEndPoint);
                mainForm.SetStatusLabel(true, iPEndPoint.ToString());
                mainForm.SetSendEnabled(true);
                mainForm.Println($"Połączenie {iPEndPoint} z serwerem");

                // Ciągłe odbieranie wiaodmości wysyłanych z serwera
                Thread thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start(socketClient);
            }
            catch (Exception ex)
            {
                mainForm.Println("Error: " + ex.Message);
            }
        }

        static void Receive(object obj)
        {
            Socket socketSend = obj as Socket;
            while (true)
            {
                try
                {
                    // Otrzymuj wiaodmości wysłane
                    byte[] buffor = new byte[1024 * 1024 * 2];
                    int len = socketSend.Receive(buffor);
                    if (len == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffor, 0, len);
                    mainForm.Println(str);
                }
                catch (Exception ex)
                {
                    mainForm.SetStatusLabel(false);
                    mainForm.SetSendEnabled(false);
                    mainForm.Println($"Serwer został rozłączony: {ex.Message}");
                    break;
                }

            }
        }

        static void SendMessage(object sender, EventArgs e)
        {
            string message = mainForm.GetMessageString();
            if (message == "")
            {
                return;
            }
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            socketClient.Send(sendBytes);
            mainForm.ClearMessageText();
        }
    }
}