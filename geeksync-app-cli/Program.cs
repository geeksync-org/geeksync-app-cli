using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using GeekSyncClient.Client;
using GeekSyncClient;
using GeekSyncClient.Config;
using System.Collections.Generic;

namespace geeksync_app_cli
{
    class Program
    {
        static void ShowHelp(string msg)
        {
            Console.WriteLine(msg);
            ShowHelp();
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: geeksync-app-cli MODE CONFIG");
        }
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                ShowHelp("Too few arguments;");
                return;
            }
            string mode = "";
            string config="";
            try
            {
                mode = args[0];
                config = args[1];
            }
            catch (Exception ex)
            {
                ShowHelp("Error parsing command line: " + ex.Message);
                return;
            }
            switch (mode.ToLower())
            {
                case "sender": Sender(config); break;
                case "receiver": Receiver(config); break;
                default: ShowHelp("Wrong mode, use sender or receiver."); break;
            }

            Console.WriteLine("Done.");
        }

        static CLIConfig LoadConfig(string config)
        {
            CLIConfig cfg;
            FileInfo cf = new FileInfo(config);
            if (!cf.Exists)
            {
                Console.WriteLine("Config file does not exist, creating default.");
                cfg = new CLIConfig() { ServerURL = "http://localhost:5000/" };
                string jsonString;
                jsonString = JsonSerializer.Serialize(cfg);
                StreamWriter sw = cf.AppendText();
                sw.WriteLine(jsonString);
                sw.Close();
            }

            StreamReader sr = cf.OpenText();
            string configtext = sr.ReadToEnd();
            sr.Close();

            cfg = JsonSerializer.Deserialize<CLIConfig>(configtext);



            return cfg;
        }

        static void Sender(string config)
        {
            CLIConfig cfg = LoadConfig(config + ".local");

            ConfigManager cm = new ConfigManager(config);

            Console.WriteLine("Enter message:");
            string msg = Console.ReadLine();

            foreach (Peer p in cm.Config.Peers)
            {
                Console.WriteLine("Sender channel: " + p.ChannelID.ToString());
                SenderClient client = new SenderClient(cm, p.ChannelID, cfg.ServerURL);
                client.CheckIfAvailable();
                Console.WriteLine("Available: " + client.IsAvailable.ToString());

                client.SendMessage(msg);
            }




        }
        static void Receiver(string config)
        {
            CLIConfig cfg = LoadConfig(config + ".local");

            ConfigManager cm = new ConfigManager(config);
            List<ReceiverClient> list=new List<ReceiverClient>();

            foreach (Peer p in cm.Config.Peers)
            {
            Console.WriteLine("Receiver Channel: " + p.ChannelID.ToString());
            ReceiverClient client = new ReceiverClient(cm,p.ChannelID, cfg.ServerURL);
            client.MessageReceived = HandleReceivedMessage;
            client.Connect();
            }
            Console.WriteLine("Press enter to close");
            Console.ReadLine();
            //client.Disconnect();
            foreach (ReceiverClient r in list)
            {
                r.Disconnect();
            }
        }

        static void HandleReceivedMessage(string msg)
        {
            Console.WriteLine("Received: " + msg);

        }
    }
}
