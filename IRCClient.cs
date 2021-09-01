using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using WebSocketSharp;

namespace MassBanTool
{
    public class IRCClient
    {
        private static Hashtable certificateErrors = new Hashtable();
        static bool mt_pause = false;

        private readonly string _password;


        // server to connect to (edit at will)
        private readonly string _server;

        // user information defined in RFC 2812 (IRC: Client Protocol) is sent to the IRC server 
        private readonly string _user;

        // channel to join
        private string _channel;
        private string _displayname;

        public int cooldown { get; set; }

        private Form form;

        Thread messageThread = null;
        List<String> toBan = new List<string>();
        int toBanLenght = 0;
        string userstate = "";

        WebSocket ws;


        public IRCClient(string server, string user, string channel, string password, Form f, int maxRetries = 3)
        {
            form = f;
            _server = server;
            _user = user;
            _password = password;
            _channel = channel;
            if (!_channel.StartsWith("#"))
            {
                _channel = "#" + _channel;
            }

            ws = new WebSocket(_server);
            ws.SslConfiguration.ServerCertificateValidationCallback = ValidateServerCertificate;

            ws.OnOpen += (sender, e) => { connect(); };

            ws.OnMessage += (sender, e) => { HandleMessage(e.Data.Trim()); };
            ws.Connect();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => ws.Close();
        }

        public LinkedList<string> MessagesQueue { get; private set; } = new LinkedList<string>();
        public LinkedList<string> MessagesQueueStopped { get; private set; } = new LinkedList<string>();
        public DateTime LastMessageSentAt { get; private set; }

        public bool Moderator { get; private set; } = false;

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

        private Thread makeMessageSender()
        {
            return new Thread(() =>
            {
                try
                {
                    TimeSpan eta;
                    Thread.CurrentThread.IsBackground = true;
                    Thread.CurrentThread.Name = "MessageSenderThread";
                    string message;
                    while (true)
                    {
                        if (mt_pause)
                        {
                            Thread.Sleep(100);
                        }

                        if (MessagesQueue.Count > 0)
                        {
                            message = MessagesQueue.First.Value;
                            MessagesQueue.RemoveFirst();
                            int banindex = (toBanLenght - MessagesQueue.Count);
                            if (banindex % 2 == 0)
                            {
                                Console.WriteLine(banindex);
                                eta = TimeSpan.FromMilliseconds(MessagesQueue.Count * cooldown);
                                form.setBanProgress(this, banindex, toBanLenght);
                                form.setETA(this,  eta.ToString("g"));
                            }

                            if (!ws.IsAlive)
                            {
                                Thread.Sleep(100);
                                ws.Connect();
                            }
                            ws.Send(message);
                            if (MessagesQueue.Count == 0)
                            {
                                
                                form.setBanProgress(this, 100, 100);
                                form.setETA(this, "-");
                            }
#if DEBUG
                            Console.WriteLine($"MT: {DateTime.Now.ToString("dd.MM H:mm:ss")} > {message}");
#endif
                            Thread.Sleep(cooldown);

                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"{e.GetType().Name} {e.Message}", "ERROR");
                    Environment.Exit(-1);
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
                    ws.Send("PASS  " + _password);
                }

                ws.Send("NICK " + _user);
                ws.Send("CAP REQ :twitch.tv/commands");
                ws.Send("CAP REQ :twitch.tv/tags");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void run()
        {
            while (true)
            {
                Thread.Sleep(100);
            }
        }

        public void HandleMessage(string m)
        {
            string[] multimessage = m.Split(new char[] {'\r', '\n'});
            string message = "";
            string cache = "";
            string display_name = "";
            foreach (string _message in multimessage)
            {
                message = _message.Trim();
                if (message.Length == 0)
                {
                    continue;
                }

                #region IRC STUFF

                Console.WriteLine(message);

                string[] splitInput = message.Split(new Char[] {' '});


                if (splitInput[0] == "PING")
                {
                    string PongReply = splitInput[1];
                    ws.Send("PONG " + PongReply);
                    Console.WriteLine($"{DateTime.Now.ToString("dd.MM hh:mm:ss")} > PONG {PongReply}");
                    return;
                }

                if (splitInput[1].Equals("001"))
                {
#if DEBUG
                    Console.WriteLine("Joining Channel: " + _channel);
#endif
                    ws.Send("JOIN " + _channel);
                    messageThread = makeMessageSender();
                    messageThread.Start();
                    return;
                }

                if (splitInput[0].StartsWith("@badge-info="))
                {
                    cache = splitInput[0].Substring(splitInput[0].IndexOf("display-name=") + "display-name=".Length);
                    display_name = cache.Substring(0, cache.IndexOf(";"));
                    if (display_name.ToLower().Equals(_user) && splitInput[2].Equals("USERSTATE"))
                    {
                        if (message.Equals(userstate))
                        {
                            return;
                        }

                        userstate = message;
                        _displayname = display_name;
                        bool moderator = splitInput[0].ToLower().Contains("badges=moderator/1");
                        bool broadcaster = splitInput[0].ToLower().Contains("badges=broadcaster/1");
                        Moderator = moderator || broadcaster;
                        form.setMod(this, moderator, broadcaster);
                        form.setInfo(this, _channel, _displayname);
                        return;
                    }
                }

                #endregion
            }
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

            for (int i = 0; i < toBan.Count; i++)
            {
                sendMessage($"/ban {toBan[i].Trim()} {reason.Trim()}", _channel);
#if DEBUG
                //sendMessage($"<command> {toBan[i].Trim()} {reason.Trim()}", _channel);
#else
                //sendMessage($"/ban {toBan[i].Trim()} {reason.Trim()}", _channel);
#endif
            }
        }

        public void StopQueue()
        {
            mt_pause = true;
            MessagesQueue.Clear();
            mt_pause = false;
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

            ws.Send($"PART {_channel}");
            _channel = newChannel;
            ws.Send($"JOIN {_channel}");
        }
    }
}