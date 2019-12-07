using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MassBanTool
{
    class IRCClient
    {
        private static Hashtable certificateErrors = new Hashtable();

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }



        // server to connect to (edit at will)
        private readonly string _server;
        // server port (6667 by default)
        private readonly int _port;
        // user information defined in RFC 2812 (IRC: Client Protocol) is sent to the IRC server 
        private readonly string _user;
        private string _displayname;

        private readonly string _password;

        // channel to join
        private string _channel;

        private readonly int _maxRetries;
        private StreamReader reader;
        private StreamWriter writer;

        private Form form;
        private readonly SslStream sslStream;

        public LinkedList<string> MessagesQueue { get; private set; } = new LinkedList<string>();
        public LinkedList<string> MessagesQueueStopped { get; private set; } = new LinkedList<string>();
        public DateTime LastMessageSentAt { get; private set; }
                     
        public bool Moderator { get; private set; } = false;
        private int cooldownNormal = 1600;
        private int cooldownVIP_Moderator = 350;
        List<String> toBan = new List<string>();
        int toBanLenght = 0;

        public IRCClient(string server, int port, string user, string channel, string password, Form f ,int maxRetries = 3)
        {
            form = f;
            _server = server;
            _port = port;
            _user = user;
            _password = password;
            _channel = channel;
            if (!_channel.StartsWith("#"))
            {
                _channel = "#" + _channel;
            }
            _maxRetries = maxRetries;
    
            var irc = new TcpClient(_server, _port);
            irc.ReceiveTimeout = 310000;

            sslStream = new SslStream(irc.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            
            try
            {
                sslStream.AuthenticateAsClient(server);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                irc.Close();

            }
            reader = new StreamReader(sslStream);
            sslStream.ReadTimeout = 310000;
            writer = new StreamWriter(sslStream);
        }

        private Thread makeMessageSender()
        {
            return new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Name = "MessageSenderThread";
                string message;
                while (true)
                {
                    if (MessagesQueue.Count > 0)
                    {
                        message = MessagesQueue.First.Value;
                        MessagesQueue.RemoveFirst();
                        int banindex = (toBanLenght - MessagesQueue.Count);
                        if (banindex % 2 == 0)
                        {
                            Console.WriteLine(banindex);
                            form.setBanProgress(this, banindex, toBanLenght);
                        }
                        writer.WriteLine(message);
                        writer.Flush();
#if DEBUG
                        Console.WriteLine($"MT: {DateTime.Now.ToString("dd.MM H:mm:ss")} > {message}");
#endif
                        if (Moderator)
                        {
                            Thread.Sleep(cooldownVIP_Moderator);
                        }
                        else
                        {
                            Thread.Sleep(cooldownNormal);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }

        private bool connect()
        {
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Logging in");
            Console.ForegroundColor = ConsoleColor.White;
#endif
            try
            {
                if (!_password.Equals(""))
                {
                    writer.WriteLine("PASS  " + _password);
                    writer.Flush();
                }
                writer.WriteLine("NICK " + _user);
                writer.Flush();

                writer.WriteLine("CAP REQ :twitch.tv/commands");
                writer.Flush();
                writer.WriteLine("CAP REQ :twitch.tv/tags");
                writer.Flush();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void Start()
        {
            while (!connect())
            {
                Console.WriteLine("Failed to connect retry in 5s ...");
                Thread.Sleep(5000);
            }
            run();
        }

        public void run()
        {
            Thread messageThread = null;

            try
            {
                string inputLine;
                while ((inputLine = reader.ReadLine()) != null)
                {
                    #region IRC STUFF
                    Console.WriteLine(inputLine);
                    string[] splitInput = inputLine.Split(new Char[] { ' ' });
                    if (splitInput[0] == "PING")
                    {
                        string PongReply = splitInput[1];
                        writer.WriteLine("PONG " + PongReply);
                        Console.WriteLine($"{DateTime.Now.ToString("dd.MM hh:mm:ss")} > PONG {PongReply}");
                        writer.Flush();
                        continue;
                    }
                    switch (splitInput[1])
                    {
                        case "001":
#if DEBUG
                            Console.WriteLine("Joining Channel: " + _channel);
#endif
                            writer.WriteLine("JOIN " + _channel);
                            writer.Flush();
                            break;

                        case "376":
                            messageThread = makeMessageSender();
                            messageThread.Start();
                            break;

                        default:
                            break;
                    }

                    #endregion
                    if (splitInput[2].Equals("USERSTATE"))
                    {
                        if (splitInput[0].StartsWith("@badge-info="))
                        {
                            bool moderator = splitInput[0].ToLower().Contains("badges=moderator/1");
                            bool broadcaster = splitInput[0].ToLower().Contains("badges=broadcaster/1");
                            if (moderator || broadcaster)
                            {
                                string cache = splitInput[0].Substring(splitInput[0].IndexOf("display-name=") + "display-name=".Length);
                                _displayname = cache.Substring(0, cache.IndexOf(";"));
                                Moderator = true;
                                form.setMod(this, moderator, broadcaster);
                                form.setInfo(this, _channel, _displayname);
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
                Console.WriteLine(e.ToString());
                Thread.Sleep(1000);
            }
            Console.WriteLine("end");
        }

        private void sendMessage(string message, string channel)
        {
            if (!message.Trim().Equals(String.Empty))
            {
                string prefix = "PRIVMSG " + _channel + " :";
                string response = prefix + message;
                MessagesQueue.AddLast(response.Trim());
            }
        }
        public void addToBann(List<string> toBan, string reason)
        {
            toBanLenght = toBan.Count;
            if (reason.Trim().Equals(String.Empty))
            {
                reason = "no reason Given.";
            }
            for(int i = 0; i<toBan.Count;i++)
            {
                sendMessage($"/ban {toBan[i].Trim()} {reason.Trim()}", _channel);
            }
        }
        public void StopQueue()
        {
            foreach(var message in MessagesQueue)
            {
                MessagesQueueStopped.AddFirst(MessagesQueue.First.Value);
                MessagesQueue.RemoveFirst();
            }            
        }
        public void switchChannel(string newChannel)
        {
            newChannel = newChannel.Trim();
            if (newChannel.Equals(string.Empty))
            {
                throw new ArgumentException("channel may not be empty");
            }
            if (!newChannel.StartsWith("#"))
            {
                newChannel = "#" + newChannel;
            }
            MessagesQueue.AddLast($"PART {_channel}");
            _channel = newChannel;
            MessagesQueue.AddLast($"JOIN {_channel}");
        }

    }
}
