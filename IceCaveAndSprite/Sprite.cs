using System.Collections.Generic;
using Lib_K_Relay;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace Sprite
{
    internal static class Extensions
    {
        public static void OryxMessage(this Client client, string fmt, params object[] args)
        {
            client.SendToClient(PluginUtils.CreateOryxNotification("Sprite", string.Format(fmt, args)));
        }
    }

    public class Sprite : IPlugin
    {
        private readonly Dictionary<Client, string> _currentMap = new Dictionary<Client, string>();
        private readonly Dictionary<int, string> _playersInDungeon = new Dictionary<int, string>();

        public string GetAuthor()
        { return "CrazyJani (updated by Todddddd)"; }

        public string GetName()
        { return "Sprite World No Clip"; }

        public string GetDescription()
        {
            return "Enables no-clipping in Sprite World. Also prevents sliding in Ice Cave.\nOptions:\nTrees - enabled by default, remove the trees in sprite world\n" +
              "Space - enabled by default, relace the space in Sprite World with gold tile to allow walked on\n" +
              "Floor - disabled by default, relace the floor tiles in Sprite World with gold tiles.\n" +
              "Ice - enabled by default, relace the floor in Ice Cave with gold told to prevent sliding";
        }

        public string[] GetCommands()
        {
            return new[]
            {
                "/sprite <On|Off> - enables/disables all",
                "/sprite <option> <On|Off>"
            };
        }

        public void Initialize(Proxy proxy)
        {
            proxy.HookPacket(PacketType.UPDATE, OnUpdate);
            proxy.HookPacket(PacketType.MAPINFO, OnMapInfo);

            proxy.HookCommand("sprite", OnCommand);
        }

        public void OnCommand(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            {
            }
            else
            {
                switch (args[0])
                {
                    case "on":
                    case "enable":
                        // Enabled all
                        Config.Default.SpriteTrees = true;
                        Config.Default.SpriteSpace = true;
                        Config.Default.SpriteFloor = true;
                        Config.Default.IceSlide = true;

                        client.SendToClient(PluginUtils.CreateOryxNotification("Sprite", "All Enabled"));
                        break;
                    case "off":
                    case "disable":
                        // Disable all
                        Config.Default.SpriteTrees = false;
                        Config.Default.SpriteSpace = false;
                        Config.Default.SpriteFloor = false;
                        Config.Default.IceSlide = false;

                        client.SendToClient(PluginUtils.CreateOryxNotification("Sprite", "All Disabled"));
                        break;
                    case "trees":
                        Config.Default.SpriteTrees = args[1] == "on";
                        client.SendToClient(PluginUtils.CreateOryxNotification("Sprite", "Sprite Trees: " + (Config.Default.SpriteTrees ? "Enabled" : " Disabled")));
                        break;
                    case "space":
                        Config.Default.SpriteSpace = args[1] == "on";
                        client.SendToClient(PluginUtils.CreateOryxNotification("Sprite", "Sprite Space: " + (Config.Default.SpriteSpace ? "Enabled" : " Disabled")));
                        break;
                    case "floor":
                        Config.Default.SpriteSpace = args[1] == "on";
                        client.SendToClient(PluginUtils.CreateOryxNotification("Sprite", "Sprite Floor: " + (Config.Default.SpriteFloor ? "Enabled" : " Disabled")));
                        break;
                    case "ice":
                        Config.Default.IceSlide = args[1] == "on";
                        client.SendToClient(PluginUtils.CreateOryxNotification("Sprite", "Ice Slide: " + (Config.Default.IceSlide ? "Enabled" : " Disabled")));
                        break;
                    default:
                        client.SendToClient(PluginUtils.CreateOryxNotification("Sprite", "Invalid Argument: " + args[0]));
                        break;
                }
            }
        }

        public void OnMapInfo(Client client, Packet packet)
        {
            MapInfoPacket mip = (MapInfoPacket) packet;
            _currentMap[client] = mip.Name;

            _playersInDungeon.Clear();
        }

        public void OnUpdate(Client client, Packet packet)
        {
            if (!_currentMap.ContainsKey(client))
            { return; }

            if (_currentMap[client] == "Sprite World" || _currentMap[client] == "Ice Cave" || _currentMap[client] == "The Inner Sanctum")
            {
                UpdatePacket up = (UpdatePacket) packet;

                // New Objects
                foreach (Entity entity in up.NewObjs)
                {
                    string name = "";

                    foreach (StatData statData in entity.Status.Data)
                    {
                        //if (!statData.IsStringData() && (statData.Id >= 8 && statData.Id <= 19) || (statData.Id >= 71 && statData.Id <= 78))
                        //    if (statData.IntValue == INC_ID) inc = true;

                        if (statData.Id == StatsType.Name)
                        { name = statData.StringValue; }
                    }

                    if (entity.Status.ObjectId != client.ObjectId && name != "")
                    {
                        if (!_playersInDungeon.ContainsKey(entity.Status.ObjectId))
                        {
                            _playersInDungeon.Add(entity.Status.ObjectId, name);

                            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, name + " joined!"));
                        }
                    }
                }

                foreach (int drop in up.Drops)
                {
                    if (_playersInDungeon.ContainsKey(drop))
                    {
                        client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, _playersInDungeon[drop] + " has left!"));
                        _playersInDungeon.Remove(drop);
                    }
                }

                // As long as PanicMode is activated and there are other players dont continue
                if (_playersInDungeon.Count > 0 && Config.Default.PanicMode)
                {
                    return;
                }

                if (Config.Default.SpriteTrees)
                {
                    foreach (Entity ent in up.NewObjs)
                    {
                        if (ent.ObjectType <= GameData.Objects.ByName("Yellow Sprite Tree").ID && ent.ObjectType >= GameData.Objects.ByName("White Sprite Tree").ID)
                        {
                            ent.ObjectType = 0;
                        }
                    }
                }

                foreach (Tile tile in up.Tiles)
                {
                    if (Config.Default.SpriteSpace)
                    {
                        if (tile.Type == GameData.Tiles.ByName("Space").ID)
                        {
                            tile.Type = GameData.Tiles.ByName("Gold Tile").ID;
                        }
                    }

                    if (Config.Default.SpriteFloor)
                    {
                        if (tile.Type > GameData.Tiles.ByName("White Alpha Square").ID && tile.Type <= GameData.Tiles.ByName("Yellow Alpha Square").ID)
                        {
                            tile.Type = GameData.Tiles.ByName("Gold Tile").ID;
                        }
                    }

                    if (Config.Default.IceSlide)
                    {
                        if (tile.Type == GameData.Tiles.ByName("Ice Slide").ID)
                        {
                            tile.Type = GameData.Tiles.ByName("Gold Tile").ID;
                        }
                    }
                }
            }
        }
    }
}
