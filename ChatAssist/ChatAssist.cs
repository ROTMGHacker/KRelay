﻿using System;
using System.IO;
using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace ChatAssist
{
    internal struct DragonInfo
    {
        public string Name;
        public bool Alive;
    }

    public class ChatAssist : IPlugin
    {
        private readonly string[] _npcIgnoreList = { "#Mystery Box Shop", "#The Alchemist", "#Login Seer", "#The Tinkerer", "#Bandit Leader", "#Drake Baby", "#Dwarf King", "#Killer Pillar", "#Haunted Armor", "#Red Demon", "#Cyclops God", "#Belladonna", "#Sumo Master", "#Avatar of the Forgotten King", "#Small Ghost", "#Medium Ghost", "#Large Ghost", "#Ghost Master", "#Ghost King", "#Lich", "#Haunted Spirit", "#Rock Construct", "#Steel Construct", "#Wood Construct", "#Phylactery Bearer", "#Mini Yeti", "#Big Yeti", "#Esben the Unwilling", "#Creepy Weird Dark Spirit Mirror Image Monster", "#Ent Ancient", "#Kage Kami", "#Twilight Archmage", "#The Forgotten Sentinel", "#The Cursed Crown", "#The Forgotten King", "#Titanum of Cruelty", "#Titanum of Despair", "#Titanum of Lies", "#Titanum of Hate", "#Grand Sphinx", "#Troll Matriarch", "#Dreadstump the Pirate King", "#Stone Mage", "#Deathmage", "#Horrid Reaper", "#Bes", "#Nut", "#Geb", "#Nikao the Azure Dragon", "#Limoz the Veridian Dragon", "#Pyyr the Crimson Dragon", "#Feargus the Obsidian Dragon", "#Nikao the Defiler", "#Limoz the Plague Bearer", "#Feargus the Demented", "#Pyyr the Wicked", "#Ivory Wyvern", "#Red Soul of Pyrr", "#Blue Soul of Nikao", "#Green Soul of Limoz", "#Black Soul of Feargus", "#Shaitan the Advisor", "#Left Hand of Shaitan", "#Right Hand of Shaitan", "#The Puppet Master", "#Trick in a Box", "#Davy Jones", "#Lord of the Lost Lands", "#Dr Terrible", "#Sheep" };
        private readonly string[] _npcImporttantList = { "", "#Oryx the Mad God", "#Ghost of Skuld", "#Altar of Draconis", "#Master Rat", "#Golden Rat", "#Janus the Doorwarden", "#Thessal the Mermaid Goddess" };

        private readonly string[,] _npcResponseList = { { "What time is it?", "Its pizza time!" }, { "Where is the safest place in the world?", "Inside my shell." }, { "What is fast, quiet and hidden by the night?", "A ninja of course!" }, { "How do you like your pizza?", "Extra cheese, hold the anchovies." }, { "Who did this to me?", "Dr. Terrible, the mad scientist." }, { "Is King Alexander alive?", "He lives and reigns and conquers the world" }, { "Say, 'READY' when you are ready to face your opponents.", "ready" }, { "Prepare yourself... Say 'READY' when you wish the battle to begin!", "ready" }, { "Well, before I explain how this all works, let me tell you that you can always say SKIP and we'll just get on with it. Otherwise, just wait a sec while I get everything in order.", "skip" } };

        private int _lastNotif;
        private int _cemCurrentWave;
        private readonly DragonInfo[] _dragons = new DragonInfo[4];
        private string _lastMessage = "";

        public string GetAuthor()
        { return "KrazyShank / Kronks / RotMGHacker"; }

        public string GetName()
        { return "Chat Assist"; }

        public string GetDescription()
        { return "A collection of tools to help your reduce the spam and clutter of chat and make it easier prevent future spam."; }

        public string[] GetCommands()
        {
            return new[]
            {
                "/chatassist <On|Off>",
                "/chatassist settings",
                "/chatassist add <message> - add a string to the spam filter",
                "/chatassist remove <message> - removes a string from the spam filter",
                "/chatassist list - list all strings included in the spam filter",
                "/chatassist log <On|Off> - Toggle Chat logging",
                "/re [new recipient] - Resends the last message you've typed on chat. Optionally to a new recipient."
            };
        }

        public void Initialize(Proxy proxy)
        {
            // Initialize Dragons
            _dragons[0].Name = "black";
            _dragons[1].Name = "green";
            _dragons[2].Name = "blue";
            _dragons[3].Name = "red";

            proxy.HookPacket(PacketType.TEXT, OnText);
            proxy.HookPacket(PacketType.PLAYERTEXT, OnPlayerText);
            proxy.HookPacket(PacketType.MAPINFO, OnMapInfo);

            proxy.HookCommand("chatassist", OnChatAssistCommand);
            proxy.HookCommand("ca", OnChatAssistCommand);
            proxy.HookCommand("re", OnResendCommand);
        }

        private static void OnChatAssistCommand(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            {
                client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Chat Assist is " + (ChatAssistConfig.Default.Enabled ? "Enabled" : "Disabled")));
            }
            else
            {
                switch (args[0])
                {
                    case "on":
                    case "enable":
                        ChatAssistConfig.Default.Enabled = true;
                        ChatAssistConfig.Default.Save();
                        client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Chat Assist Enabled!"));
                        break;
                    case "off":
                    case "disable":
                        ChatAssistConfig.Default.Enabled = false;
                        ChatAssistConfig.Default.Save();
                        client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Chat Assist Disabled!"));
                        break;
                    case "settings":
                        PluginUtils.ShowGUI(new FrmChatAssistSettings());
                        break;
                    case "add":
                        if (args.Length > 1)
                        {
                            // This is needed because args are seperated by whitespaces and the string to filter coud contain whitespaces so we concatenate them together
                            string toFilter = "";
                            for (int i = 1; i < args.Length; ++i)
                            {
                                toFilter += args[i] + " ";
                            }

                            toFilter = toFilter.Trim();

                            // Only add valid entries
                            if (toFilter.Length > 0)
                            {
                                ChatAssistConfig.Default.Blacklist.Add(toFilter);
                                client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, toFilter + " added to spam filter!"));

                                ChatAssistConfig.Default.Save(); // Save our changes
                            }
                            else
                            {
                                client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Invalid message!"));
                            }
                        }
                        else
                        {
                            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Missing message to filter!"));
                        }
                        break;
                    case "remove":
                    case "rem":
                        if (args.Length > 1)
                        {
                            string toRemove = "";
                            for (int i = 1; i < args.Length; ++i)
                            {
                                toRemove += args[i] + " ";
                            }

                            toRemove = toRemove.Trim();

                            if (ChatAssistConfig.Default.Blacklist.Contains(toRemove))
                            {
                                ChatAssistConfig.Default.Blacklist.Remove(toRemove);
                                client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, toRemove + " removed from spam filter!"));

                                ChatAssistConfig.Default.Save(); // Save our changes
                            }
                            else
                            {
                                client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Couldn't find " + toRemove + " in spam filter!"));
                            }
                        }
                        else
                        {
                            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Missing message to remove!"));
                        }
                        break;
                    case "list":
                        if (ChatAssistConfig.Default.Blacklist.Count == 0)
                        {
                            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Spam filter is empty!"));
                            return;
                        }

                        string message = "Spam filter contains: ";

                        // Construct our list
                        foreach (string filter in ChatAssistConfig.Default.Blacklist)
                        {
                            message += filter + ", ";
                        }

                        message = message.Remove(message.Length - 2);

                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", message));
                        break;
                    case "log":
                        if (args.Length == 1)
                        { ChatAssistConfig.Default.LogChat = !ChatAssistConfig.Default.LogChat; }
                        else
                        {
                            if (args[1] == "on")
                            { ChatAssistConfig.Default.LogChat = true; }
                            else if (args[1] == "off")
                            { ChatAssistConfig.Default.LogChat = false; }
                        }

                        client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Turned Chat logging " + (ChatAssistConfig.Default.LogChat ? "On" : "Off")));
                        break;
                    default:
                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", "Unrecognized command: " + args[0]));
                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", "Usage:"));
                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", "'/chatassist <On:Off>' - turn chatassist On/Off"));
                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", "'/chatassist settings' - open settings"));
                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", "'/chatassist add <message>' - add the give string to the spam filter"));
                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", "'/chatassist remove <message>' - remove the give string from the spam filter"));
                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", "'/chatassist list' - display all string in the spam filter"));
                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", "'/chatassist log [On/Off]' - turn chat logging On/Off"));
                        client.SendToClient(PluginUtils.CreateOryxNotification("ChatAssist", "'/re [new recipient]' - resend the last message you've typed. Optionally to a new recipient"));
                        break;
                }
            }
        }

        private void OnResendCommand(Client client, string command, string[] args)
        {
            if (_lastMessage == "")
            { return; }

            PlayerTextPacket playerTextPacket = (PlayerTextPacket) Packet.Create(PacketType.PLAYERTEXT);

            // Only try to change the recipient when whispering
            if (args.Length == 0 && (_lastMessage.StartsWith("/t ") || _lastMessage.StartsWith("/tell ") || _lastMessage.StartsWith("/w ") || _lastMessage.StartsWith("/whisper ")))
            {
                const int count = 3;
                string[] sub = _lastMessage.Split(" ".ToCharArray(), count);
                // sub[0] = original command (/t, /tell, /w, /whisper)
                // args[0] = new recipient
                // sub[2] = the actual message
                playerTextPacket.Text = sub[0] + " " + args[0] + " " + sub[2];
            }
            else // same or no recipient
            {
                playerTextPacket.Text = _lastMessage;
            }

            client.SendToServer(playerTextPacket);
        }

        private void OnText(Client client, Packet packet)
        {
            if (!ChatAssistConfig.Default.Enabled)
            { return; }

            TextPacket text = packet.To<TextPacket>();

            if (text.NumStars == -1) // Not a message from a user
            {
                if (ChatAssistConfig.Default.EnableNPCFilter)
                {
                    foreach (string name in _npcIgnoreList)
                    {
                        if (text.Name.Contains(name))
                        {
                            text.Send = false;
                            return;
                        }
                    }
                }

                if (ChatAssistConfig.Default.AutoResponse)
                {
                    for (int i = 0; i < _npcResponseList.GetLength(0); ++i)
                    {
                        if (text.Text.Contains(_npcResponseList[i, 0]))
                        {
                            PlayerTextPacket playerText = (PlayerTextPacket) Packet.Create(PacketType.PLAYERTEXT);
                            playerText.Text = _npcResponseList[i, 1];
                            client.SendToServer(playerText);
                        }
                    }

                    // Lair of Draconis
                    if (text.Text == "Choose the Dragon Soul you wish to commune with!")
                    {
                        for (int i = 0; i < _dragons.Length; ++i)
                        {
                            if (_dragons[i].Alive)
                            {
                                PlayerTextPacket playerText = (PlayerTextPacket) Packet.Create(PacketType.PLAYERTEXT);
                                playerText.Text = _dragons[i].Name;
                                client.SendToServer(playerText);

                                return;
                            }
                        }
                    }
                }

                // Event notifications

                string message;

                if (text.Text == "{\"key\":\"server.oryx_closed_realm\"}")
                { message = "Realm has Closed!"; }
                else if (text.Text.Contains("stringlist.Lich.one"))
                { message = "Final Lich!"; }
                else if (text.Text == "Squeek!")
                { message = "Golden Rat Encountered!"; }
                else if (text.Text == "You thrash His Lordship's castle, kill his brothers, and challenge us. Come, come if you dare.")
                { message = "Janus Spawned!"; }
                else if (text.Text == "Sweet treasure awaits for powerful adventurers!")
                { message = "Crystal Spawned!"; }
                else if (text.Text == "Me door is open. Come let me crush you!")
                { message = "Door Opened!"; }
                else if (text.Text == "Innocent souls. So delicious. You have sated me. Now come, I shall give you your reward.")
                { message = "Inner Sanctum OOpened!"; }
                else if (text.Name == "#Ghost of Skuld" && (text.Text.Contains("3 seconds") || text.Text.Contains("'READY'")))
                {
                    _cemCurrentWave += 1;

                    message = "Wave " + _cemCurrentWave + "/5!";
                    text.Text = message;
                    text.CleanText = message;
                }
                else if (text.Name == "#Altar of Draconis")
                {
                    if (text.Text.Contains("Feargus"))
                    { _dragons[0].Alive = false; } // black
                    else if (text.Text.Contains("Limoz"))
                    { _dragons[1].Alive = false; } // green
                    else if (text.Text == "Do not let the tranquil surroundings fool you!")
                    { _dragons[2].Alive = false; } // blue
                    else if (text.Text.Contains("Pyyr"))
                    { _dragons[3].Alive = false; } // red
                    return;
                }
                else if (text.Name.Contains("#Oryx"))
                {
                    if (text.Text.Contains("Hermit_God"))
                    { message = "Hermit God"; }
                    else if (text.Text.Contains("Lord_of_the_Lost_Lands"))
                    { message = "Lord of the Lost Lands"; }
                    else if (text.Text.Contains("Grand_Sphinx"))
                    { message = "Grand Sphinx"; }
                    else if (text.Text.Contains("Pentaract"))
                    { message = "Pentaract"; }
                    else if (text.Text.Contains("shtrs_Defense_System"))
                    { message = "Avatar"; }
                    else if (text.Text.Contains("Ghost_Ship"))
                    { message = "Ghost Ship"; }
                    else if (text.Text.Contains("Dragon_Head_Leader"))
                    { message = "Rock Dragon"; }
                    else if (text.Text.Contains("Cube_God"))
                    { message = "Cube God"; }
                    else if (text.Text.Contains("Skull_Shrine"))
                    { message = "Skull Shrine"; }
                    else if (text.Text.Contains("Temple_Encounter"))
                    { message = "Temple Statues"; }
                    else
                    { message = "Unknown New: " + text.Text; }

                    if (text.Text.Contains("new"))
                    { message += " Spawned!"; }
                    else if (text.Text.Contains("killed") || text.Text.Contains("death"))
                    { message += " Died!"; }
                    else
                    { return; }
                }
                else
                {
#if DEBUG
                    if (text.Name != "")
                    {
                        PluginUtils.Log("ChatAssist", "Unknown Server Message: '" + text.Text + "' From: '" + text.Name + "'");
                    }
#endif // DEBUG
                    return;
                }

                if (Environment.TickCount - _lastNotif > 1000)
                {
                    _lastNotif = Environment.TickCount;
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, message));
                    return;
                }
                PluginUtils.Delay(1000 - (Environment.TickCount - _lastNotif), () =>
                {
                    _lastNotif = Environment.TickCount;
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, message));
                });
            }

            if (ChatAssistConfig.Default.DisableMessages && text.Recipient == "" ||
                text.Recipient == "" && text.NumStars < ChatAssistConfig.Default.StarFilter && text.NumStars != -1 ||
                text.Recipient != "" && text.NumStars < ChatAssistConfig.Default.StarFilterPM && text.NumStars != -1)
            {
                text.Send = false;
                return;
            }

            if (ChatAssistConfig.Default.EnableSpamFilter)
            {
                foreach (string filter in ChatAssistConfig.Default.Blacklist)
                {
                    if (filter.ToLower().Trim() == "")
                    {
                        continue;
                    }

                    if (text.Text.ToLower().Contains(filter.ToLower().Trim()))
                    {
                        // Is spam
                        if (ChatAssistConfig.Default.CensorSpamMessages)
                        {
                            text.Text = "...";
                            text.CleanText = "...";
                        }
                        else
                        { text.Send = false; }

                        if (ChatAssistConfig.Default.AutoIgnoreSpamMessage ||
                           ChatAssistConfig.Default.AutoIgnoreSpamPM && text.Recipient != "")
                        {
                            // Ignore
                            PlayerTextPacket playerText = (PlayerTextPacket) Packet.Create(PacketType.PLAYERTEXT);
                            playerText.Text = "/ignore " + text.Name;
                            client.SendToServer(playerText);
                        }

                        return;
                    }
                }
            }

            // ChatLog
            if (ChatAssistConfig.Default.LogChat && text.NumStars != -1)
            {
                using (StreamWriter file = new StreamWriter("ChatAssist.log", true))
                {
                    file.WriteLine("<" + DateTime.Now + ">: " + text.Name + "[" + text.NumStars + "]: '" + text.Text + "'");
                }
            }
        }

        private void OnPlayerText(Client client, Packet packet)
        {
            PlayerTextPacket playerTextPacket = (PlayerTextPacket) packet;

            if (!playerTextPacket.Text.StartsWith("/") || playerTextPacket.Text.StartsWith("/t ") || playerTextPacket.Text.StartsWith("/tell ") || playerTextPacket.Text.StartsWith("/w ") || playerTextPacket.Text.StartsWith("/whisper ") || playerTextPacket.Text.StartsWith("/yell "))
            {
                _lastMessage = playerTextPacket.Text;
            }

            if (ChatAssistConfig.Default.LogChat)
            {
                using (StreamWriter file = new StreamWriter("ChatAssist.log", true))
                {
                    file.WriteLine("<" + DateTime.Now + ">: You: '" + playerTextPacket.Text + "'");
                }
            }
        }

        private void OnMapInfo(Client client, Packet packet)
        {
            MapInfoPacket mip = (MapInfoPacket) packet;

            if (mip.Name.Contains("Haunted Cemetery"))
            {
                _cemCurrentWave = 0;
            }
            else if (mip.Name == "Lair of Draconis")
            {
                for (int i = 0; i < _dragons.Length; ++i)
                {
                    _dragons[i].Alive = true; // mark all dragons as alive
                }
            }
        }
    }
}
