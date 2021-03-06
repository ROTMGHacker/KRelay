﻿using System.Collections.Generic;
using Lib_K_Relay;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace IncFinder
{
    public class IncFinder : IPlugin
    {
        private readonly ushort INC_ID = GameData.Items.ByName("Wine Cellar Incantation").ID;
        private readonly Dictionary<int, string> _incHolders = new Dictionary<int, string>();

        public string GetAuthor()
        { return "KrazyShank / Kronks / RotMGHacker"; }

        public string GetName()
        { return "Inc Finder"; }

        public string GetDescription()
        { return "Tells you what people around you have Wine Cellar Incantations in their inventory."; }

        public string[] GetCommands()
        { return new[] { "/wc" }; }

        public void Initialize(Proxy proxy)
        {
            proxy.ClientConnected += client => _incHolders.Clear();

            proxy.HookPacket(PacketType.UPDATE, OnUpdate);

            proxy.HookCommand("wc", OnWCCommand);
        }

        private void OnWCCommand(Client client, string command, string[] args)
        {
            if (_incHolders.Count == 0)
            {
                client.SendToClient(PluginUtils.CreateOryxNotification("Inc Finder", "No one has an Inc!"));
            }
            else
            {
                string message = "Inc Holders: ";

                foreach (KeyValuePair<int, string> pair in _incHolders)
                {
                    message += pair.Value + ",";
                }

                message = message.Remove(message.Length - 1);

                client.SendToClient(PluginUtils.CreateOryxNotification("Inc Finder", message));
            }
        }

        private void OnUpdate(Client client, Packet packet)
        {
            UpdatePacket update = (UpdatePacket) packet;

            // New Objects
            foreach (Entity entity in update.NewObjs)
            {
                if (GameData.Objects.ByID((ushort) entity.ObjectType).Name == "Locked Wine Cellar Portal" && client.PlayerData.MapName == "Oryx's Chamber")
                {
                    if (_incHolders.Count == 0)
                    {
                        client.SendToClient(PluginUtils.CreateOryxNotification("Inc Finder", "No one has an Inc!"));
                    }
                    else
                    {
                        string message = "Inc Holders: ";

                        foreach (KeyValuePair<int, string> pair in _incHolders)
                        {
                            message += pair.Value + ",";
                        }

                        message = message.Remove(message.Length - 1);

                        client.SendToClient(PluginUtils.CreateOryxNotification("Inc Finder", message));
                    }
                    continue;
                }

                bool inc = false;
                string name = "";

                foreach (StatData statData in entity.Status.Data)
                {
                    if ((!statData.IsStringData() && statData.Id >= 8 && statData.Id <= 19 || statData.Id >= 71 && statData.Id <= 78) && statData.IntValue == INC_ID)
                    {
                        inc = true;
                    }

                    if (statData.Id == StatsType.Name)
                    {
                        name = statData.StringValue;
                    }
                }

                if (inc && entity.Status.ObjectId != client.ObjectId)
                {
                    if (!_incHolders.ContainsKey(entity.Status.ObjectId))
                    {
                        _incHolders.Add(entity.Status.ObjectId, name);
                    }

                    client.SendToClient(PluginUtils.CreateOryxNotification("Inc Finder", name + " has an Incantation!"));
                }
            }

            // Removed Objects
            foreach (int drop in update.Drops)
            {
                if (_incHolders.ContainsKey(drop))
                {
                    client.SendToClient(PluginUtils.CreateOryxNotification("Inc Finder", _incHolders[drop] + " has left!"));
                    _incHolders.Remove(drop);
                }
            }
        }
    }
}
