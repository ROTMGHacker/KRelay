using System;
using System.Collections.Generic;
using System.Linq;
using Lib_K_Relay;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace DailyQuest
{
    internal class QuestHelper
    {
        public int Goal;
        public string Map = "";
        public Dictionary<int, Location> BagLocations = new Dictionary<int, Location>();
        public int LastNotif;
        public bool ReqSent;
    }

    public class DailyQuest : IPlugin
    {
        private readonly Dictionary<Client, QuestHelper> _dQuest = new Dictionary<Client, QuestHelper>();

        private readonly short[] _bags = { (short) Bags.Red, (short) Bags.Purple, (short) Bags.Blue, (short) Bags.Cyan, (short) Bags.White, (short) Bags.Pink, (short) Bags.Normal, (short) Bags.Egg };

        public string GetAuthor()
        { return "Todddddd / RotMGHacker"; }

        public string GetName()
        { return "Daily Quest"; }

        public string GetDescription()
        {
            return "Show what item you are looking for, and quickly turn in the quest item.";
        }

        public string[] GetCommands()
        {
            return new[]
            {
                "/dq",
                "/dq settings"
            };
        }

        public void Initialize(Proxy proxy)
        {
            proxy.ClientConnected += c => _dQuest.Add(c, new QuestHelper());
            proxy.ClientDisconnected += c => _dQuest.Remove(c);

            proxy.HookPacket(PacketType.QUESTFETCHRESPONSE, OnQuestFetch);
            proxy.HookPacket(PacketType.QUESTREDEEMRESPONSE, OnQuestRedeem);
            proxy.HookPacket(PacketType.MAPINFO, OnMapInfo);
            proxy.HookPacket(PacketType.UPDATE, OnUpdate);
            proxy.HookPacket(PacketType.MOVE, OnMove);
            proxy.HookPacket(PacketType.LOGINREWARDMSG, OnLoginReward);

            proxy.HookCommand("dq", OnCommand);
        }

        public void OnMove(Client client, Packet packet)
        {
            if (!_dQuest.ContainsKey(client))
            { return; }

            foreach (int bagId in _dQuest[client].BagLocations.Keys)
            {
                float distance = _dQuest[client].BagLocations[bagId].DistanceTo(client.PlayerData.Pos);

                if (DailyQuestConfig.Default.BagNotifications && Environment.TickCount - _dQuest[client].LastNotif > 2000 && distance < 15)
                {
                    _dQuest[client].LastNotif = Environment.TickCount;
                    client.SendToClient(PluginUtils.CreateNotification(bagId, 0x0000FF, "Current Daily Quest: " + GameData.Objects.ByID((ushort) _dQuest[client].Goal).Name));
                }
            }
        }

        public void OnUpdate(Client client, Packet packet)
        {
            if (!_dQuest.ContainsKey(client))
            { return; }

            if (
                (DailyQuestConfig.Default.AutoRequest ||
                (DailyQuestConfig.Default.AutoTurnIn && _dQuest[client].Map == "Daily Quest Room")
                ) && !_dQuest[client].ReqSent)
            {
                _dQuest[client].ReqSent = true;
                client.SendToServer(Packet.Create(PacketType.QUESTFETCHASK));
            }

            UpdatePacket update = (UpdatePacket) packet;
            if (_dQuest[client].Goal == 0)
            { return; }

            foreach (Entity entity in update.NewObjs)
            {
                short type = entity.ObjectType;
                if (_bags.Contains(type))
                {
                    int bagId = entity.Status.ObjectId;

                    if (entity.Status.Data.Any(statData => statData.Id >= 8 && statData.Id <= 15 && statData.IntValue == _dQuest[client].Goal))
                    {
                        if (!_dQuest[client].BagLocations.ContainsKey(bagId))
                        {
                            _dQuest[client].BagLocations.Add(bagId, entity.Status.Position);
                        }
                        else
                        {
                            _dQuest[client].BagLocations[bagId] = entity.Status.Position;
                        }
                    }
                }
            }
        }

        public void OnMapInfo(Client client, Packet packet)
        {
            if (!_dQuest.ContainsKey(client))
            { return; }

            MapInfoPacket mip = (MapInfoPacket) packet;
            _dQuest[client].Map = mip.Name;
        }

        public void OnQuestFetch(Client client, Packet packet)
        {
            if (!_dQuest.ContainsKey(client))
            { return; }

            QuestFetchResponsePacket qfrp = (QuestFetchResponsePacket) packet;

            if (qfrp.Goal == "")
            { return; }

            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Current Daily Quest: " + GameData.Objects.ByID((ushort) qfrp.Goal.ParseInt()).Name));
            _dQuest[client].Goal = qfrp.Goal.ParseInt();

            // The quest can only be turned in when you are in the Daily Quest Room
            if (DailyQuestConfig.Default.AutoTurnIn && _dQuest[client].Map == "Daily Quest Room")
            {
                byte slot = 0;
                if (_dQuest[client].Goal != 0)
                {
                    for (byte i = 0; i < 8; i++)
                    {
                        if (client.PlayerData.Slot[i + 4] == _dQuest[client].Goal)
                        {
                            slot = (byte) (i + 4);
                            break;
                        }

                        if (client.PlayerData.BackPack[i] == _dQuest[client].Goal)
                        {
                            slot = (byte) (i + 12);
                            break;
                        }
                    }
                }

                // If slot does not equal 0 that means we have the item
                if (slot != 0)
                {
                    TurnInQuest(client, slot);
                }
            }
        }

        public void OnQuestRedeem(Client client, Packet packet)
        {
            if (!_dQuest.ContainsKey(client))
            {
                return;
            }

            QuestRedeemResponsePacket qrrp = (QuestRedeemResponsePacket) packet;
            if (qrrp.Success)
            {
                _dQuest[client].Goal = 0;
                client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Quest Turned In!"));

                PluginUtils.Log("DailyQuest", "Requesting Quest Data");
                client.SendToServer(Packet.Create(PacketType.QUESTFETCHASK));
            }
        }

        public void TurnInQuest(Client client, byte slot)
        {
            PluginUtils.Log("DailyQuest", "Attempting turn in");

            QuestRedeemPacket tqp = (QuestRedeemPacket) Packet.Create(PacketType.QUESTREDEEM);
            tqp.Slot = new SlotObject
            {
                SlotId = slot,
                ObjectId = client.PlayerData.OwnerObjectId,
                ObjectType = slot > 11 ? client.PlayerData.BackPack[slot - 12] : client.PlayerData.Slot[slot]
            };


            client.SendToServer(tqp);
        }

        public void OnCommand(Client client, string command, string[] args)
        {
            if (!_dQuest.ContainsKey(client))
            { return; }

            if (args.Length == 0)
            {
                // The quest can only be turned in when you are in the Daily Quest Room
                if (_dQuest[client].Map == "Daily Quest Room")
                {
                    byte slot = 0;
                    if (_dQuest[client].Goal != 0)
                    {
                        for (byte i = 0; i < 8; i++)
                        {
                            if (client.PlayerData.Slot[i + 4] == _dQuest[client].Goal)
                            {
                                slot = (byte) (i + 4);
                                break;
                            }
                            if (client.PlayerData.BackPack[i] == _dQuest[client].Goal)
                            {
                                slot = (byte) (i + 12);
                                break;
                            }
                        }
                    }
                    // If slot does not equal 0 that means we have the item
                    if (slot != 0)
                    {
                        TurnInQuest(client, slot);
                    }
                    else
                    {
                        PluginUtils.Log("DailyQuest", "Requesting Quest Data");
                        client.SendToServer(Packet.Create(PacketType.QUESTFETCHASK));
                    }
                }
                else
                {
                    PluginUtils.Log("DailyQuest", "Requesting Quest Data");
                    client.SendToServer(Packet.Create(PacketType.QUESTFETCHASK));
                }
            }
            else
            {
                switch (args[0])
                {
                    case "get":
                        PluginUtils.Log("DailyQuest", "Requesting Quest Data");
                        client.SendToServer(Packet.Create(PacketType.QUESTFETCHASK));
                        break;
                    case "turnin":
                        if (!byte.TryParse(args[1], out byte slot))
                        { return; }

                        QuestRedeemPacket tqp = (QuestRedeemPacket) Packet.Create(PacketType.QUESTREDEEM);
                        tqp.Slot = new SlotObject
                        {
                            SlotId = slot,
                            ObjectId = client.PlayerData.OwnerObjectId,
                            ObjectType = slot > 11 ? client.PlayerData.BackPack[slot - 12] : client.PlayerData.Slot[slot]
                        };

                        tqp.Send = true;

                        client.SendToServer(tqp);
                        break;
                    case "settings":
                        PluginUtils.ShowGenericSettingsGUI(DailyQuestConfig.Default, "Daily Quest Settings");
                        break;
                    default:
                        client.SendToClient(PluginUtils.CreateOryxNotification("DailyQuest", "Unrecognized command: " + args[0]));
                        break;
                }
            }
        }

        public void OnLoginReward(Client client, Packet packet)
        {
            ClaimDailyRewardResponsePacket cdrr = packet.To<ClaimDailyRewardResponsePacket>();

            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Claimed " + cdrr.Qty + "x " + GameData.Objects.ByID((ushort) cdrr.ItemId).Name));
        }
    }
}
