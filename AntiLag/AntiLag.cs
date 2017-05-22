using System.Collections.Generic;
using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace AntiLag
{
    public class AntiLag : IPlugin
    {
        public Dictionary<Client, bool> AllEffects = new Dictionary<Client, bool>();

        public string GetAuthor()
        { return "059 (updated by Todddddd)"; }

        public string GetName()
        { return "AntiLag"; }

        public string GetDescription()
        { return "Blocks certain packets which contribute significantly to lagging your client."; }

        public string[] GetCommands()
        { return new[] { "/antilag", "/antilag effects all" }; }

        public void Initialize(Proxy proxy)
        {
            proxy.ClientConnected += c => AllEffects.Add(c, false);
            proxy.ClientDisconnected += c => AllEffects.Remove(c);

            proxy.HookPacket(PacketType.SHOWEFFECT, OnShowEffect);
            proxy.HookPacket(PacketType.ALLYSHOOT, OnAllyShoot);
            proxy.HookPacket(PacketType.DAMAGE, OnDamage);
            proxy.HookPacket(PacketType.SERVERPLAYERSHOOT, OnServerPlayerShoot);

            proxy.HookCommand("antilag", OnCommand);
        }

        public void OnServerPlayerShoot(Client client, Packet packet)
        {
            ServerPlayerShootPacket sps = (ServerPlayerShootPacket) packet;
            if (AntiLagConfig.Default.Other && sps.OwnerId != client.ObjectId)
            {
                packet.Send = false;
            }
        }

        // TODO: Change to switch
        private void OnCommand(Client client, string command, string[] args)
        {
            if (args.Length == 0 || args[0] == "settings" || args[0] == "config")
            {
                PluginUtils.ShowGenericSettingsGUI(AntiLagConfig.Default, "AntiLag Settings");
            }
            else
            {
                if (args[0] == "effects" && args[1] == "all")
                {
                    AllEffects[client] = !AllEffects[client];
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "AntiLag ALL Particles " + AllEffects[client]));
                }
            }
        }

        private void OnShowEffect(Client client, Packet packet)
        {
            if (AntiLagConfig.Default.Effects)
            {
                ShowEffectPacket sep = (ShowEffectPacket) packet;
                if (AllEffects[client])
                {
                    // TODO: Check logic this is always going to be true?

                    if (sep.EffectType == EffectType.Nova && sep.TargetId != client.ObjectId)
                    {
                        packet.Send = false;
                    }
                    else
                    {
                        packet.Send = false;
                    }
                }
                else
                {
                    switch ((int) sep.EffectType - 1)
                    {
                        case 0:
                        case 1:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 11:
                        case 16:
                        case 17:
                        case 18:
                            packet.Send = false;
                            break;
                        case 4:
                            if (sep.TargetId != client.ObjectId)
                            {
                                packet.Send = false;
                            }

                            break;
                    }
                }
            }
        }

        private static void OnAllyShoot(Client client, Packet packet)
        {
            if (AntiLagConfig.Default.Ally)
            {
                packet.Send = false;
            }
        }

        private static void OnDamage(Client client, Packet packet)
        {
            if (AntiLagConfig.Default.Damage)
            {
                DamagePacket dp = (DamagePacket) packet;
                if (dp.ObjectId != client.ObjectId)
                {
                    packet.Send = false;
                }
            }
        }
    }
}
