using System;
using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace Glow
{
    public class Glow : IPlugin
    {
        public string GetAuthor()
        { return "KrazyShank / Kronks / RotMGHacker"; }

        public string GetName()
        { return "Glow"; }

        public string GetDescription()
        { return "You're so excited about K Relay that you're literally glowing!"; }

        public string[] GetCommands()
        {
            return new[]
            {
                "/AmISpecial",
                "/Glow [On|Off] - You need to reconnect for the changes to apply"
            };
        }

        public void Initialize(Proxy proxy)
        {
            proxy.HookPacket(PacketType.UPDATE, OnUpdate);

            proxy.HookCommand("amispecial", OnSpecialCommand);
            proxy.HookCommand("glow", OnGlowCommand);
        }

        private static void OnSpecialCommand(Client client, string command, string[] args)
        {
            Random r = new Random();
            int val = 0;
            for (int i = 0; i < 10; ++i)
            {
                val += r.Next(400000, 723411);
                client.SendToClient(PluginUtils.CreateNotification(
                    client.ObjectId, val, "YOU ARE SPECIAL!"));
            }
        }

        private static void OnGlowCommand(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            {
                GlowSettings.Default.GlowEnable = !GlowSettings.Default.GlowEnable;
            }
            else
            {
                switch (args[0])
                {
                    case "on":
                    case "enable":
                        GlowSettings.Default.GlowEnable = true;
                        break;
                    case "off":
                    case "disable":
                        GlowSettings.Default.GlowEnable = false;
                        break;
                    default:
                        client.SendToClient(PluginUtils.CreateOryxNotification("Glow", "Unrecognized command: " + args[0]));
                        return;
                }
            }

            GlowSettings.Default.Save();
            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "Glow " + (GlowSettings.Default.GlowEnable ? "enabled" : "disabled") + "!"));
        }

        private static void OnUpdate(Client client, Packet packet)
        {
            if (!GlowSettings.Default.GlowEnable)
            { return; }

            UpdatePacket update = (UpdatePacket) packet;

            foreach (Entity ent in update.NewObjs)
            {
                if (ent.Status.ObjectId != client.ObjectId)
                { continue; }

                foreach (StatData data in ent.Status.Data)
                {
                    if (data.Id == 59)
                    {
                        data.IntValue = 100;
                    }
                }
            }
        }
    }
}
