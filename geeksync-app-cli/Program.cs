using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using GeekSyncClient.Client;

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
            FileInfo config;
            try
            {
                mode = args[0];
                config = new FileInfo(args[1]);
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

        static CLIConfig LoadConfig(FileInfo config)
        {
            CLIConfig cfg;
            if (!config.Exists)
            {
                Console.WriteLine("Config file does not exist, creating default.");
                cfg = new CLIConfig() { ChannelID = Guid.NewGuid(), ServerURL = "http://localhost:5000/" };
                string jsonString;
                jsonString = JsonSerializer.Serialize(cfg);
                StreamWriter sw = config.AppendText();
                sw.WriteLine(jsonString);
                sw.Close();
            }

            StreamReader sr = config.OpenText();
            string configtext = sr.ReadToEnd();
            sr.Close();

            cfg = JsonSerializer.Deserialize<CLIConfig>(configtext);



            return cfg;
        }

        static void Sender(FileInfo config)
        {
            CLIConfig cfg = LoadConfig(config);
            Console.WriteLine("Sender channel: " + cfg.ChannelID.ToString());
            SenderClient client=new SenderClient(cfg.ChannelID,cfg.ServerURL);
            client.CheckIfAvailable();
            Console.WriteLine("Available: "+client.IsAvailable.ToString());
            client.SendMessage("TEST");
            
        }
        static void Receiver(FileInfo config)
        {
            CLIConfig cfg = LoadConfig(config);
            Console.WriteLine("Receiver Channel: " + cfg.ChannelID.ToString());
        }
    }
}
