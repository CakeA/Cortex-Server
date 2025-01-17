﻿using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using MySql.Data.MySqlClient;

using Newtonsoft.Json.Linq;

using Cortex.Server.Socket;
using Cortex.Server.Events;
using Cortex.Server.Socket.Events;

using Cortex.Server.Discord;

namespace Cortex.Server {
    class Program {
        static public SocketClass Socket;

        public static JObject Config = JObject.Parse(File.ReadAllText("C:/Cortex/Cortex.json"));
        
        public static string Connection = $"server={Config["mysql"]["host"]};uid={Config["mysql"]["credentials"]["name"]};pwd={Config["mysql"]["credentials"]["password"]};database={Config["mysql"]["database"]};SslMode={Config["mysql"]["sslmode"]}";
        public static string ConnectionTest = $"server={Config["mysql"]["host"]};uid={Config["mysql"]["credentials"]["name"]};pwd={Config["mysql"]["credentials"]["password"]};database={Config["mysql"]["database"]}_test;SslMode={Config["mysql"]["sslmode"]}";

        public static DiscordClient Discord;

        static void Main(string[] args) {
            Console.Start();
            
            Console.WriteLine(  @"  _____           _           _      _____           _            " + Environment.NewLine +
                                @" |  __ \         (_)         | |    / ____|         | |           " + Environment.NewLine +
                                @" | |__) | __ ___  _  ___  ___| |_  | |     ___  _ __| |_ _____  __" + Environment.NewLine +
                                @" |  ___/ '__/ _ \| |/ _ \/ __| __| | |    / _ \| '__| __/ _ \ \/ /" + Environment.NewLine +
                                @" | |   | | | (_) | |  __/ (__| |_  | |___| (_) | |  | ||  __/>  < " + Environment.NewLine +
                                @" |_|   |_|  \___/| |\___|\___|\__|  \_____\___/|_|   \__\___/_/\_\" + Environment.NewLine +
                                @"                _/ |                                              " + Environment.NewLine +
                                @"               |__/                                               " + Environment.NewLine);
            
            Console.WriteLine("Project Cortex Server by Cake\t\thttps://project-cortex.net/" + Environment.NewLine);

            foreach (var instance in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(a => a.GetConstructor(Type.EmptyTypes) != null).Select(Activator.CreateInstance).OfType<IInitializationEvent>()) {
                instance.OnInitialization();
            }

            Socket = new SocketClass();

            foreach (var instance in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(a => a.GetConstructor(Type.EmptyTypes) != null).Select(Activator.CreateInstance).OfType<ISocketEvent>()) {
                if(instance.Event.Length == 0)
                    continue;
                
                if(!Socket.events.ContainsKey(instance.Event))
                    Socket.events[instance.Event] = new List<ISocketEvent>();

                Socket.events[instance.Event].Add(instance);
            }

            Socket.Open();


            if(Config["discord"]["bot"]["enabled"].ToObject<bool>()) {
                Discord = new DiscordClient();

                Thread discord = new Thread(Discord.Start);
                discord.Start();
            }

            while(true) {
                string input = Console.ReadLine();

                if(input.StartsWith("query")) {
                    int start = input.IndexOf(' ');

                    if(start == -1) {
                        Console.WriteLine("Query command must be followed by the exact database query!");

                        continue;
                    }

                    string query = input.Substring(start);

                    using MySqlConnection connection = new MySqlConnection(Connection);

                    connection.Open();

                    using MySqlCommand command = new MySqlCommand(query, connection);

                    int affected = command.ExecuteNonQuery();

                    Console.WriteLine("Query executed and affected " + affected + " rows!");
                }
            }
        }
    }
}
