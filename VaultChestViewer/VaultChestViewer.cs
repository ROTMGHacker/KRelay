using System.Collections.Generic;
using System.Linq;
using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;

namespace VaultChestViewer
{
    public class VaultChestViewer : IPlugin
    {
        private int _mCurrentGameId;
        private Dictionary<int, int[]> _mChests;

        public string GetAuthor()
        {
            return "creepylava / ossimc82";
        }

        public string GetName()
        {
            return "Vault Highlight";
        }

        public string GetDescription()
        {
            return "Lost in vault, trying to find empty chest? Look for the indicators added under the chests!";
        }

        public string[] GetCommands()
        {
            return new string[0];
        }

        public void Initialize(Proxy proxy)
        {
            proxy.HookPacket<HelloPacket>(OnHelloPacket);
            proxy.HookPacket<MapInfoPacket>(OnMapInfoPacket);
            proxy.HookPacket<UpdatePacket>(OnUpdatePacket);
            proxy.HookPacket<NewTickPacket>(OnNewTickPacket);
        }

        private void OnHelloPacket(Client client, HelloPacket packet)
        {
            _mCurrentGameId = packet.GameId;
            _mChests = new Dictionary<int, int[]>();
        }

        private void OnMapInfoPacket(Client client, MapInfoPacket packet)
        {
            if (_mCurrentGameId != -5)
            { return; }

            packet.ClientXML = packet.ClientXML.Concat(new[]
                {
                    @"	<Objects>
        <Object type=""0x0504"" id=""Vault Chest"">
            <Class>Container</Class>
            <Container/>
            <CanPutNormalObjects/>
            <CanPutSoulboundObjects/>
            <ShowName/>
            <Texture><File>lofiObj2</File><Index>0x0e</Index></Texture>
            <SlotTypes>0, 0, 0, 0, 0, 0, 0, 0</SlotTypes>
        </Object>
    </Objects>"
                }).ToArray();
        }

        private void OnUpdatePacket(Client client, UpdatePacket packet)
        {
            if (_mCurrentGameId != -5)
            { return; }

            foreach (Entity ent in packet.NewObjs)
            {
                if (ent.ObjectType == 0x0504) //Vault Chest
                {
                    UpdateStats(ref ent.Status);
                }
            }
        }

        private void OnNewTickPacket(Client client, NewTickPacket packet)
        {
            if (_mCurrentGameId != -5)
            { return; }

            for (int i = 0; i < packet.Statuses.Length; i++)
            {
                if (_mChests.ContainsKey(packet.Statuses[i].ObjectId))
                {
                    UpdateStats(ref packet.Statuses[i]);
                }
            }
        }

        private void UpdateStats(ref Status stats)
        {
            ParseChestData(stats);

            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                if (_mChests.ContainsKey(stats.ObjectId) && _mChests[stats.ObjectId][i] != -1)
                { count++; }
            }

            string name = $"Items: {count}/8";
            stats.Data = stats.Data.Concat(new[]
            {
                new StatData
                {
                     Id = StatsType.Name,
                     StringValue = name
                }
            }).ToArray();
        }

        private void ParseChestData(Status stats)
        {
            foreach (StatData stat in stats.Data)
            {
                if (stat.Id - StatsType.Inventory0 < 8 && stat.Id - StatsType.Inventory0 > -1)
                {
                    if (!_mChests.ContainsKey(stats.ObjectId))
                    {
                        _mChests.Add(stats.ObjectId, new int[8]);
                    }

                    _mChests[stats.ObjectId][stat.Id - StatsType.Inventory0] = stat.IntValue;
                }
            }
        }
    }
}
