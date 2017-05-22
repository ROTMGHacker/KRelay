using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace ClientStatAnnouncer
{
    public class ClientStatAnnouncer : IPlugin
    {
        public string GetAuthor()
        { return "KrazyShank / Kronks & Alde / RotMGHacker"; }

        public string GetName()
        { return "ClientStat Announcer"; }

        public string GetDescription()
        { return "Lets you know when you progress on in-game achievements."; }

        public string[] GetCommands()
        { return new string[] { }; }

        public void Initialize(Proxy proxy)
        {
            proxy.HookPacket(PacketType.CLIENTSTAT, OnClientStat);
        }

        private static void OnClientStat(Client client, Packet packet)
        {
            ClientStatPacket clientStat = (ClientStatPacket) packet;

            string toDisplay = clientStat.Name + "  has increased to  ";

            if (clientStat.Name.Equals("Shots")) // 0: 'Shots',
            {
                toDisplay = "Bullets shot : ";
            }
            else if (clientStat.Name.Equals("ShotsThatDamage")) // 1: 'ShotsThatDamage',
            {
                toDisplay = "Bullets that damaged : ";
            }
            else if (clientStat.Name.Equals("SpecialAbilityUses")) // 2: 'SpecialAbilityUses',
            {
                toDisplay = "Ability uses : ";
            }
            else if (clientStat.Name.Equals("TilesUncovered")) // 3: 'TilesUncovered',
            {
                toDisplay = "Tiles uncovered : ";
            }
            else if (clientStat.Name.Equals("Teleports")) // 4: 'Teleports',
            {
                toDisplay = "Teleports : ";
            }
            else if (clientStat.Name.Equals("PotionsDrunk")) // 5: 'PotionsDrunk',
            {
                toDisplay = "Potions drank : ";
            }
            else if (clientStat.Name.Equals("MonsterKills")) // 6: 'MonsterKills',
            {
                toDisplay = "Monster kills : ";
            }
            else if (clientStat.Name.Equals("MonsterAssists")) // 7: 'MonsterAssists',
            {
                toDisplay = "Monster assists : ";
            }
            else if (clientStat.Name.Equals("GodKills")) // 8: 'GodKills',
            {
                toDisplay = "God kills : ";
            }
            else if (clientStat.Name.Equals("GodAssists")) // 9: 'GodAssists',
            {
                toDisplay = "God assists : ";
            }
            else if (clientStat.Name.Equals("CubeKills")) // 10: 'CubeKills',
            {
                toDisplay = "Cube kills : ";
            }
            else if (clientStat.Name.Equals("OryxKills")) // 11: 'OryxKills',
            {
                toDisplay = "Oryx kills : ";
            }
            else if (clientStat.Name.Equals("QuestsCompleted")) // 12: 'QuestsCompleted',
            {
                toDisplay = "Quests completed : ";
            }
            else if (clientStat.Name.Equals("PirateCavesCompleted")) // 13: 'PirateCavesCompleted',
            {
                toDisplay = "Pirate Cave(s) completed : ";
            }
            else if (clientStat.Name.Equals("UndeadLairsCompleted")) // 14: 'UndeadLairsCompleted',
            {
                toDisplay = "Undead Lair(s) completed : ";
            }
            else if (clientStat.Name.Equals("AbyssOfDemonsCompleted")) // 15: 'AbyssOfDemonsCompleted',
            {
                toDisplay = "Abyss of Demon(s) completed : ";
            }
            else if (clientStat.Name.Equals("SnakePitsCompleted")) // 16: 'SnakePitsCompleted',
            {
                toDisplay = "Snake Pit(s) completed : ";
            }
            else if (clientStat.Name.Equals("SpiderDensCompleted")) // 17: 'SpiderDensCompleted',
            {
                toDisplay = "Spider Den(s) completed : ";
            }
            else if (clientStat.Name.Equals("SpriteWorldsCompleted")) // 18: 'SpriteWorldsCompleted',
            {
                toDisplay = "Sprite World(s) completed : ";
            }
            else if (clientStat.Name.Equals("LevelUpAssists")) // 19: 'LevelUpAssists',
            {
                toDisplay = "Level-up assist(s) : ";
            }
            else if (clientStat.Name.Equals("MinutesActive")) // 20: 'MinutesActive',
            {
                toDisplay = "Minute(s) active : ";
            }
            else if (clientStat.Name.Equals("TombsCompleted")) // 21: 'TombsCompleted',
            {
                toDisplay = "Tomb(s) completed : ";
            }
            else if (clientStat.Name.Equals("TrenchesCompleted")) // 22: 'TrenchesCompleted',
            {
                toDisplay = "Trenche(s) completed : ";
            }
            else if (clientStat.Name.Equals("JunglesCompleted")) // 23: 'JunglesCompleted',
            {
                toDisplay = "Jungle(s) completed : ";
            }
            else if (clientStat.Name.Equals("ManorsCompleted")) // 24: 'ManorsCompleted',
            {
                toDisplay = "Manor(s) completed : ";
            }
            else if (clientStat.Name.Equals("ForestMazeCompleted")) // 25: 'ForestMazeCompleted',
            {
                toDisplay = "Forest Maze(s) completed : ";
            }

            else if (clientStat.Name.Equals("HauntedCemeteryCompleted")) // 28: 'HauntedCemeteryCompleted'
            {
                toDisplay = "Haunted Cementery(s) completed : ";
            }

            else if (clientStat.Name.Equals("ToxicSewersCompleted")) // 42: 'ToxicSewersCompleted'
            {
                toDisplay = "Toxic Sewer(s) completed : ";
            }

            else if (clientStat.Name.Equals("DungeonTypesComplete")) // ??: 'DungeonTypesComplete'
            {
                toDisplay = "Dungeon Type(s) completed : ";
            }
            else if (clientStat.Name.Equals("IceCaveCompleted")) // ??: 'IceCaveCompleted'
            {
                toDisplay = "Ice Cave(s) completed : ";
            }

            else
            {
                //print(toDisplay = "Unknown -> Name :" + clientStat.Name + " Value :" + clientStat.Value);
            }

            toDisplay += clientStat.Value;

            client.SendToClient(
                PluginUtils.CreateOryxNotification(
                    "ClientStat Announcer", toDisplay));
        }
    }
}
