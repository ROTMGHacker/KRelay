using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lib_K_Relay;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace Follow
{
    /// <summary>
    /// Represents a cached world map
    /// </summary>
    public class Map
    {
        /// <summary>
        /// The UUID of the map
        /// </summary>
        public uint UUID;

        /// <summary>
        /// The dimensions of the map
        /// </summary>
        public int Width, Height;

        /// <summary>
        /// The raw data of the map such that Data[x, y] is the tile type
        /// </summary>
        public ushort[,] Data;

        /// <summary>
        /// Constructs an empty map from the given packet
        /// </summary>
        /// <param name="info">The MapInfoPacket</param>
        public Map(MapInfoPacket info)
        {
            UUID = info.Fp;
            Width = info.Width;
            Height = info.Height;
            Data = new ushort[Width, Height];
        }

        /// <summary>
        /// Gets the type of the tile at the given coordinates
        /// </summary>
        /// <returns>The tile type</returns>
        public ushort At(int x, int y)
        {
            return Data[x, y];
        }

        /// <summary>
        /// Gets the type of the tile at the given coordinates
        /// </summary>
        /// <returns>The tile type</returns>
        public ushort At(float x, float y)
        {
            return Data[(int) x, (int) y];
        }
    }

    public class ClientInfo
    {
        public Client Client;
        public bool Master, Slave;
        public Stopwatch Sw = new Stopwatch();
        public Map Map;

        public ClientInfo(Client c)
        {
            Client = c;
            Master = false;
            Slave = false;
            Sw.Start();
        }

        public void SetMaster()
        {
            Slave = false;
            Master = true;
        }
        public void SetSlave()
        {
            Master = false;
            Slave = true;
        }

        public void UpdateTick()
        {
            Sw.Restart();
        }

        public ushort CurrentTileType()
        {
            return Map.At(Client.PlayerData.Pos.X, Client.PlayerData.Pos.Y);
        }
    }

    public class Follow : IPlugin
    {
        public Dictionary<Client, ClientInfo> ListOfClients = new Dictionary<Client, ClientInfo>();

        public Client Master;

        public bool Enabled;

        public string GetAuthor()
        { return "Todddddd"; }

        public string GetName()
        { return "Follow"; }

        public string GetDescription()
        { return "Set up clients to follow a master client around the map"; }

        public string[] GetCommands()
        {
            return new[]
            {
                "/follow <master|slave>",
                "/follow <start|stop>"
            };
        }

        public void Initialize(Proxy proxy)
        {
            proxy.ClientConnected += OnConnect;
            proxy.ClientDisconnected += OnDisconnect;

            proxy.HookPacket(PacketType.GOTO, OnGoto);
            proxy.HookPacket(PacketType.GOTOACK, OnGotoAck);
            proxy.HookPacket(PacketType.NEWTICK, OnNewTick);
            proxy.HookPacket(PacketType.USEPORTAL, OnUsePortal);
            proxy.HookPacket(PacketType.MAPINFO, OnMapInfo);
            proxy.HookPacket(PacketType.UPDATE, OnUpdate);

            proxy.HookCommand("follow", OnCommand);
        }

        public void OnCommand(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            { return; }

            switch (args[0])
            {
                case "master":
                    Master = client;
                    ListOfClients[client].SetMaster();
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Set as Master Client!"));
                    break;
                case "slave":
                    ListOfClients[client].SetSlave();
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Set as Slave Client!"));
                    break;
                case "start":
                    Enabled = true;
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Follow Started!"));
                    break;
                case "stop":
                    Enabled = false;
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Follow Stopped!"));
                    break;
                default:
                    client.SendToClient(PluginUtils.CreateOryxNotification("Follow", "Unrecognized command: " + args[0]));
                    break;
            }
        }

        public void OnGoto(Client client, Packet packet)
        {
            if (Enabled && ListOfClients[client].Slave)
            {
                // Send our own GOTOACK when we get a GOTO from the server
                GotoAckPacket gotoack = (GotoAckPacket) Packet.Create(PacketType.GOTOACK);
                gotoack.Time = client.Time;
                client.SendToServer(gotoack);
            }
        }

        public void OnUsePortal(Client client, Packet packet)
        {
            UsePortalPacket upp = (UsePortalPacket) packet;
            // Check if the master client used a portal
            if (Enabled && ListOfClients[client].Master)
            {
                // Go through each client in the list and send the same packet if they are a slave
                foreach (KeyValuePair<Client, ClientInfo> ci in ListOfClients)
                {
                    if (ci.Value.Slave)
                    {
                        ci.Value.Client.SendToServer(upp);
                    }
                }
            }
        }

        public void OnGotoAck(Client client, Packet packet)
        {
            GotoAckPacket goap = (GotoAckPacket) packet;
            if (Enabled && ListOfClients[client].Slave)
            {
                // Block the GOTOACK if the client is a slave
                goap.Send = false;
            }
        }

        public void OnNewTick(Client client, Packet packet)
        {
            if (Enabled && ListOfClients[client].Slave)
            {
                // Distance to Master
                double distance = Math.Sqrt(Math.Pow(Master.PlayerData.Pos.X - client.PlayerData.Pos.X, 2) + Math.Pow(Master.PlayerData.Pos.Y - client.PlayerData.Pos.Y, 2));
                // Will hold the angle
                // Get the total milliseconds since the last NewTick
                long timeElapsed = ListOfClients[client].Sw.ElapsedMilliseconds;
                // This will be used as a multiplier, ive capped it at 250
                timeElapsed = timeElapsed >= 200 ? 200 : timeElapsed;

                // The formula from the client is (MIN_MOVE_SPEED + this.speed_ / 75 * (MAX_MOVE_SPEED - MIN_MOVE_SPEED)) * this.moveMultiplier_
                // this.moveMultiplier_ = the tile movement speed (1 by default)
                // MIN_MOVE_SPEED = 0.004 * this.moveMultiplier_
                // MAX_MOVE_SPEED = 0.0096
                // I dont take into account tile movement speed i justed used the default value of 1
                // I also decreased the max movement speed to avoid disconnect
                float moveMultiplier = GameData.Tiles.ByID(ListOfClients[client].CurrentTileType()).Speed;
                float minSpeed = 0.004f * moveMultiplier;
                float speed = (minSpeed + client.PlayerData.Speed / 75.0f * (0.007f - minSpeed)) * moveMultiplier * timeElapsed;
                Console.WriteLine(@"SW: {0}ms, Dist: {1}, Spd: {2}, TPT: {3}", ListOfClients[client].Sw.ElapsedMilliseconds, distance, speed, client.PlayerData.TilesPerTick());

                //speed = client.PlayerData.TilesPerTick();


                Location newLoc = new Location();
                // Check if the distance to the master is greater then the distance the slave can move
                if (distance > speed)
                {
                    // Calculate the angle
                    float angle = (float) Math.Atan2(Master.PlayerData.Pos.Y - client.PlayerData.Pos.Y, Master.PlayerData.Pos.X - client.PlayerData.Pos.X);
                    // Calculate the new location
                    newLoc.X = client.PlayerData.Pos.X + (float) Math.Cos(angle) * speed;
                    newLoc.Y = client.PlayerData.Pos.Y + (float) Math.Sin(angle) * speed;
                }
                else
                {
                    // Set the move location as the master location
                    newLoc.X = Master.PlayerData.Pos.X;
                    newLoc.Y = Master.PlayerData.Pos.Y;
                }
                // Send the GOTO packet
                GotoPacket go = (GotoPacket) Packet.Create(PacketType.GOTO);
                go.ObjectId = client.ObjectId;
                go.Location = new Location();
                go.Location = newLoc;
                client.SendToClient(go);
            }

            if (ListOfClients[client].Slave)
            {
                // This is used to get the number of milliseconds between each NewTick
                ListOfClients[client].UpdateTick();
            }
        }

        public void OnMapInfo(Client client, Packet packet)
        {
            MapInfoPacket mip = (MapInfoPacket) packet;
            if (ListOfClients.ContainsKey(client))
            {
                ListOfClients[client].Map = new Map(mip);
            }
        }

        public void OnUpdate(Client client, Packet packet)
        {
            UpdatePacket up = (UpdatePacket) packet;
            foreach (Tile tile in up.Tiles)
            {
                if (ListOfClients.ContainsKey(client))
                {
                    ListOfClients[client].Map.Data[tile.X, tile.Y] = tile.Type;
                }
            }
        }

        // TODO: Keep settings between disconnects (like using a portal)
        public void OnConnect(Client client)
        {
            if (!ListOfClients.ContainsKey(client))
            {
                ListOfClients.Add(client, new ClientInfo(client));
            }
            else
            {
                ListOfClients[client] = new ClientInfo(client);
            }
        }
        public void OnDisconnect(Client client)
        {
            // If the Master client disconnects turn off the plugin
            if (ListOfClients.ContainsKey(client) && ListOfClients[client].Master)
            { Enabled = false; }

            if (ListOfClients.ContainsKey(client))
            {
                ListOfClients.Remove(client);
            }
        }

    }
}
