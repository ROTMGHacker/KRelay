using System;
using System.Collections.Generic;
using System.Linq;
using Lib_K_Relay;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Utilities;

namespace AutoAbility
{
    public class AutoAbility : IPlugin
    {
        private int _cooldown = 20;
        private readonly Dictionary<Client, UseItemPacket> _useItemMap = new Dictionary<Client, UseItemPacket>();
        private readonly Dictionary<Client, int> _clientTimes = new Dictionary<Client, int>();
        private readonly Classes[] _validClasses = { Classes.Rogue, Classes.Priest, Classes.Paladin, Classes.Warrior };

        public string GetAuthor()
        { return "KrazyShank / Kronks"; }

        public string GetName()
        { return "Auto Ability"; }

        public string GetDescription()
        {
            return "Automatically uses your abilities based on your class and your specified conditions:\n" +
                   "Paladin: Automatically Seal Buff\n" +
                   "Priest: Automatically Tome Buff and/or Heal\n" +
                   "Warrior: Automatically Helm Buff";
        }

        public string[] GetCommands()
        {
            return new[]
            {
                "/aa on",
                "/aa off",
                "/aa settings"
            };
        }

        public void Initialize(Proxy proxy)
        {
            proxy.ClientConnected += c => _useItemMap.Add(c, null);
            proxy.ClientDisconnected += c => _useItemMap.Remove(c);

            proxy.HookPacket(PacketType.CREATESUCCESS, OnCreateSuccess);
            proxy.HookPacket(PacketType.USEITEM, OnUseItem);
            proxy.HookPacket(PacketType.UPDATE, OnUpdate);
            proxy.HookPacket(PacketType.NEWTICK, OnUpdate);
            proxy.HookPacket(PacketType.MOVE, OnMove);

            proxy.HookCommand("aa", OnAACommand);
        }

        public int GetTime(Client client)
        {
            return client.Time + (Environment.TickCount - _clientTimes[client]);
        }

        private void OnMove(Client client, Packet packet)
        {
            if (!_clientTimes.ContainsKey(client))
            {
                _clientTimes.Add(client, Environment.TickCount);
            }

            _clientTimes[client] = Environment.TickCount;
        }

        private static void OnCreateSuccess(Client client, Packet packet)
        {
            if (AutoAbilityConfig.Default.Enabled)
            {
                PluginUtils.Delay(2100, () =>
                    client.SendToClient(PluginUtils.CreateOryxNotification(
                        "Auto Ability", "Use your ability to Activate Auto Ability")));
            }
        }

        private void OnUseItem(Client client, Packet packet)
        {
            if (_useItemMap[client] == null && _validClasses.Contains(client.PlayerData.Class))
            {
                client.SendToClient(PluginUtils.CreateNotification(
                    client.ObjectId, "Auto Ability Activated!"));
            }

            _useItemMap[client] = packet as UseItemPacket;
        }

        private void OnUpdate(Client client, Packet packet)
        {
            if (_cooldown > 0 && packet.Type == PacketType.NEWTICK)
            { _cooldown--; }

            if (_cooldown != 0 || !AutoAbilityConfig.Default.Enabled || _useItemMap[client] == null)
            { return; }

            float manaPercentage = client.PlayerData.Mana / (float) client.PlayerData.MaxMana;
            float healthPercentage = client.PlayerData.Health / (float) client.PlayerData.MaxHealth;

            switch (client.PlayerData.Class)
            {
                case Classes.Paladin:
                    {
                        if (!client.PlayerData.HasConditionEffect(ConditionEffects.Damaging) &&
                            manaPercentage > AutoAbilityConfig.Default.RequiredManaPercent &&
                            AutoAbilityConfig.Default.PaladinAutoBuff)
                        {
                            SendUseItem(client);
                        }
                        break;
                    }
                case Classes.Priest:
                    {
                        if (!client.PlayerData.HasConditionEffect(ConditionEffects.Healing) &&
                            manaPercentage > AutoAbilityConfig.Default.RequiredManaPercent &&
                            AutoAbilityConfig.Default.PriestAutoBuff ||
                            healthPercentage < AutoAbilityConfig.Default.RequiredHealthPercent &&
                            AutoAbilityConfig.Default.PriestAutoHeal)
                        {
                            SendUseItem(client);
                        }
                        break;
                    }
                case Classes.Warrior:
                    {
                        if (!client.PlayerData.HasConditionEffect(ConditionEffects.Berserk) &&
                            manaPercentage > AutoAbilityConfig.Default.RequiredManaPercent &&
                            AutoAbilityConfig.Default.WarriorAutoBuff)
                        {
                            SendUseItem(client);
                        }
                        break;
                    }
                case Classes.Rogue:
                    {
                        if (!client.PlayerData.HasConditionEffect(ConditionEffects.Invisible) &&
                            manaPercentage > AutoAbilityConfig.Default.RequiredManaPercent &&
                            AutoAbilityConfig.Default.RogueAutoCloak)
                        {
                            SendUseItem(client);
                        }
                        break;
                    }
            }
        }

        private void OnAACommand(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            { return; }

            switch (args[0])
            {
                case "on":
                case "enable":
                    AutoAbilityConfig.Default.Enabled = true;
                    AutoAbilityConfig.Default.Save();
                    client.SendToClient(
                        PluginUtils.CreateNotification(
                            client.ObjectId, "Auto Ability Enabled!"));
                    break;
                case "off":
                case "disable":
                    AutoAbilityConfig.Default.Enabled = false;
                    AutoAbilityConfig.Default.Save();
                    client.SendToClient(
                        PluginUtils.CreateNotification(
                            client.ObjectId, "Auto Ability Disabled!"));
                    break;
                case "settings":
                    PluginUtils.ShowGenericSettingsGUI(AutoAbilityConfig.Default, "Auto Ablity Settings");
                    break;
                case "test":
                    SendUseItem(client);
                    break;
                default:
                    client.SendToClient(PluginUtils.CreateOryxNotification("AutoAbility", "Unrecognized command: " + args[0]));
                    break;
            }
        }

        private void SendUseItem(Client client)
        {
            _cooldown = AutoAbilityConfig.Default.RetryDelay;

            UseItemPacket useItem = _useItemMap[client];
            useItem.Time = GetTime(client);
            useItem.ItemUsePos = client.PlayerData.Pos;
            client.SendToServer(useItem);

            if (AutoAbilityConfig.Default.ShowBuffNotifications)
            {
                client.SendToClient(PluginUtils.CreateNotification(
                    client.ObjectId, "Auto-Buff Triggered!"));
            }
        }
    }
}
