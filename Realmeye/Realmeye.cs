using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using Lib_K_Relay;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Utilities;
using Realmeye.Properties;

namespace Realmeye
{
    public class Realmeye : IPlugin
    {
        public static string ItemAbbrXml { get; private set; }

        public Dictionary<string, string> ItemAbbrs = new Dictionary<string, string>();

        public string GetAuthor()
        { return "CrazyJani (updated by Todddddd)"; }

        public string GetName()
        { return "Realmeye"; }

        public string GetDescription()
        { return "Quick commands to send tells to MrEyeball and other actions. This plugin contains a list of the most common item abbreviations that you can use with the /sell command."; }

        public string[] GetCommands()
        {
            return new[]
            {
                "/player <playername>",
                "/friends",
                "/hide",
                "/stats",
                "/mates",
                "/market",
                "/lefttomax",
                "/scammer <playername>"
            };
        }

        public void Initialize(Proxy proxy)
        {
            LoadXmlData();

            proxy.HookCommand("friends", OnCommand);
            proxy.HookCommand("hide", OnCommand);
            proxy.HookCommand("stats", OnCommand);
            proxy.HookCommand("mates", OnCommand);
            proxy.HookCommand("lefttomax", OnCommand);
            proxy.HookCommand("scammer", OnCommand);

            proxy.HookCommand("player", OnWebCommand);
            proxy.HookCommand("market", OnWebCommand);
            proxy.HookCommand("sell", OnWebCommand);
        }

        public void LoadXmlData()
        {
            // Load the XML of item abbreviations
            ItemAbbrXml = Resources.ItemAbbreviations;
            XDocument doc = XDocument.Parse(ItemAbbrXml);
            doc.Element("items")
                ?.Elements("item")
                .ForEach(item =>
                {
                    string name = item.AttrDefault("name", "");
                    foreach (XElement abbr in item.Descendants("abbr"))
                    {
                        ItemAbbrs.Add(abbr.Value, name);
                    }
                });

            PluginUtils.Log("RealmEye", "Found {0} item abbreviations.", ItemAbbrs.Count);
        }

        private static void OnCommand(Client client, string command, string[] args)
        {
            string msg = "/t mreyeball ";

            switch (command)
            {
                case "hide":
                    msg += "hide me";
                    break;
                case "scammer":
                    if (args.Length == 0)
                    {
                        client.SendToClient(PluginUtils.CreateOryxNotification("RealmEye", "Missing name argument"));
                        return;
                    }
                    else
                    { msg += "scammer " + args[0]; }
                    break;
                default:
                    msg += command;
                    break;
            }

            PlayerTextPacket ptp = (PlayerTextPacket) Packet.Create(PacketType.PLAYERTEXT);

            ptp.Text = msg;

            client.SendToServer(ptp);
        }

        private void OnWebCommand(Client client, string command, string[] args)
        {
            switch (command)
            {
                case "market":
                    Process.Start("https://www.realmeye.com/current-offers");
                    break;
                case "player":
                    if (args.Length > 0)
                    {
                        Process.Start("https://www.realmeye.com/player/" + args[0]);
                    }
                    break;
                case "sell":
                    if (args.Length > 1)
                    {
                        int item1 = 0;
                        int item2 = 0;

                        if (ItemAbbrs.ContainsKey(args[0].ToLower()))
                        {
                            // Try to find the item id from our list of abbreviations
                            try
                            { item1 = GameData.Items.ByName(ItemAbbrs[args[0].ToLower()]).ID; }
                            catch { }
                        }
                        else
                        {
                            // Try to find the item id from the string entered as is
                            try
                            { item1 = GameData.Items.ByName(args[0]).ID; }
                            catch { }
                        }

                        if (ItemAbbrs.ContainsKey(args[1].ToLower()))
                        {
                            // Try to find the item id from our list of abbreviations
                            try
                            { item2 = GameData.Items.ByName(ItemAbbrs[args[1].ToLower()]).ID; }
                            catch { }
                        }
                        else
                        {
                            // Try to find the item id from the string entered as is
                            try
                            { item2 = GameData.Items.ByName(args[1]).ID; }
                            catch { }
                        }

                        if (item2 == 0)
                        {
                            client.SendToClient(PluginUtils.CreateOryxNotification("Error", "Couldn't find any item with the name or abbreviation \"" + args[1] + "\""));
                            return;
                        }

                        Process.Start("https://www.realmeye.com/offers-to/sell/" + item2 + "/" + item1);
                    }
                    break;
                default:
                    client.SendToClient(PluginUtils.CreateOryxNotification("RealmEye", "Unrecognized command: " + args[0]));
                    break;
            }
        }
    }
}
