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
using MapCacher;

namespace AutoNexus
{
    internal static class Extensions
    {
        public static void OryxMessage(this Client client, string fmt, params object[] args)
        {
            client.SendToClient(PluginUtils.CreateOryxNotification("Auto Nexus", string.Format(fmt, args)));
        }
    }

    internal struct Bullet
    {
        /// <summary>
        /// Map of piercing projectiles
        /// Object ID -> list of piercing projectile IDs
        /// </summary>
        public static Dictionary<int, List<int>> Piercing = new Dictionary<int, List<int>>();

        /// <summary>
        /// Map of armor break projectiles
        /// Object ID -> list of armor break projectile IDs
        /// </summary>
        public static Dictionary<int, List<int>> Breaking = new Dictionary<int, List<int>>();

        public static bool IsPiercing(int enemyType, int projectileType)
        {
            return Piercing.ContainsKey(enemyType) && Piercing[enemyType].Contains(projectileType);
        }

        public static bool IsArmorBreaking(int enemyType, int projectileType)
        {
            return Breaking.ContainsKey(enemyType) && Breaking[enemyType].Contains(projectileType);
        }

        // owner ID of bullet
        public int OwnerID;
        // the ID of the bullet
        public int ID;
        // the type of projectile
        public int ProjectileID;
        // raw damage
        public int Damage;
    }

    internal class ClientState
    {
        public int HP = 100;

        public bool Safe = true;

        public bool Armored;
        public bool ArmorBroken;

        // enemy id -> projectiles
        public Dictionary<int, List<Bullet>> BulletMap = new Dictionary<int, List<Bullet>>();

        // enemy id -> enemy type
        public Dictionary<int, ushort> EnemyTypeMap = new Dictionary<int, ushort>();

        public Client Client;

        public ClientState(Client client)
        {
            Client = client;
        }

        public void Update(UpdatePacket update)
        {
            foreach (Entity e in update.NewObjs)
            {
                if (!EnemyTypeMap.ContainsKey(e.Status.ObjectId))
                {
                    EnemyTypeMap[e.Status.ObjectId] = (ushort) e.ObjectType;
                }
            }
        }

        public void Tick(NewTickPacket tick)
        {
            HP += (int) (0.2 + Client.PlayerData.Vitality * 0.024);

            foreach (Status status in tick.Statuses)
            {
                if (status.ObjectId != Client.ObjectId)
                { continue; }

                foreach (StatData stat in status.Data)
                {
                    if (stat.Id == StatsType.HP)
                    {
                        HP = stat.IntValue;
                    }
                }
            }

            ArmorBroken = Client.PlayerData.HasConditionEffect(ConditionEffects.ArmorBroken);
            Armored = Client.PlayerData.HasConditionEffect(ConditionEffects.Armored);
        }

        private void MapBullet(Bullet b)
        {
            if (!BulletMap.ContainsKey(b.OwnerID))
            {
                BulletMap[b.OwnerID] = new List<Bullet>();
            }

            BulletMap[b.OwnerID].Add(b);
        }

        public void EnemyShoot(EnemyShootPacket eshoot)
        {
            for (int i = 0; i < eshoot.NumShots; i++)
            {
                Bullet b = new Bullet
                {
                    OwnerID = eshoot.OwnerId,
                    ID = eshoot.BulletId + i,
                    ProjectileID = eshoot.BulletType,
                    Damage = eshoot.Damage
                };

                MapBullet(b);
            }
        }

        public void Shoot2(ServerPlayerShootPacket spsp)
        {
            Bullet b = new Bullet
            {
                OwnerID = spsp.OwnerId,
                ID = spsp.BulletId,
                ProjectileID = -1,
                Damage = spsp.Damage
            };
            MapBullet(b);
        }

        private int PredictDamage(AoEPacket aoe)
        {
            int def = Client.PlayerData.Defense;

            if (aoe.Effects == ConditionEffectIndex.ArmorBroken)
            { ArmorBroken = true; }

            if (Armored)
            { def *= 2; }

            if (ArmorBroken)
            { def = 0; }

            return Math.Max(Math.Max(aoe.Damage - def, 0), (int) (0.15f * aoe.Damage));
        }

        private int PredictDamage(Bullet b)
        {
            int def = Client.PlayerData.Defense;

            if (EnemyTypeMap.ContainsKey(b.OwnerID) && Bullet.IsArmorBreaking(EnemyTypeMap[b.OwnerID], b.ProjectileID) && !ArmorBroken)
            {
                ArmorBroken = true;

                if (Config.Default.Debug)
                {
                    PluginUtils.Log("Auto Nexus", "{0}'s armor is broken!", Client.PlayerData.Name);
                }
            }

            if (Armored)
            { def *= 2; }

            if (ArmorBroken || EnemyTypeMap.ContainsKey(b.OwnerID) && Bullet.IsPiercing(EnemyTypeMap[b.OwnerID], b.ProjectileID))
            { def = 0; }

            int dmg = b.Damage - def;

            if (dmg < 0.15 * b.Damage)
            {
                dmg = (int) (0.15 * b.Damage);
            }

            return dmg;
            //return Math.Max(Math.Max(b.Damage - def, 0), (int)(0.15f * b.Damage));
        }

        private bool ApplyDamage(int dmg)
        {
            if (!Safe)
            { return false; }

            HP -= dmg;

            if (Config.Default.Debug)
            {
                PluginUtils.Log("Auto Nexus", "{0} was hit for {1} damage ({2}/{3})!", Client.PlayerData.Name, dmg, HP, Client.PlayerData.MaxHealth);
            }

            if (Config.Default.Enabled)
            {
                float hpPerc = (float) HP / Client.PlayerData.MaxHealth;
                if (hpPerc <= Config.Default.NexusPercent || (float) Client.PlayerData.Health / Client.PlayerData.MaxHealth <= Config.Default.NexusPercent)
                {
                    //Console.WriteLine("HP: {0} {1} {2} {3} {4}", (float)client.PlayerData.Health / client.PlayerData.MaxHealth, Config.Default.NexusPercent, client.PlayerData.Health, client.PlayerData.MaxHealth, hpPerc);
                    PluginUtils.Log("Auto Nexus", "Saved {0}'s ass at {1}/{2} HP!", Client.PlayerData.Name, HP, Client.PlayerData.MaxHealth);
                    Client.SendToServer(Packet.Create(PacketType.ESCAPE));
                    Safe = false;

                    return false;
                }
            }

            return true;
        }

        public void PlayerHit(PlayerHitPacket phit)
        {
            foreach (Bullet b in BulletMap[phit.ObjectId])
            {
                if (b.ID == phit.BulletId)
                {
                    phit.Send = ApplyDamage(PredictDamage(b));
                    break;
                }
            }
        }

        public void AoE(AoEPacket aoe)
        {
            if (Client.PlayerData.Pos.DistanceSquaredTo(aoe.Location) <= aoe.Radius * aoe.Radius)
            {
                aoe.Send = ApplyDamage(PredictDamage(aoe));
            }
        }

        public void GroundDamage(GroundDamagePacket gdamage)
        {
            ushort t = Client.GetMap().At(gdamage.Position.X, gdamage.Position.Y);
            if (GameData.Tiles.Map.ContainsKey(t))
            {
                gdamage.Send = ApplyDamage(GameData.Tiles.ByID(t).MaxDamage);
            }
        }
    }

    public class AutoNexus : IPlugin
    {
        public string GetAuthor()
        {
            return "apemanzilla";
        }

        public string[] GetCommands()
        {
            return new[]
            {
                "/autonexus",
                "/autonexus <percentage> - set the percentage to go nexus at (0-99)",
                "/autonexus <On|Off> - toggle autonexus on and off",
                "/autonexus debug <On|Off> - toggle debug messages on and off"
            };
        }

        public string GetDescription()
        {
            return "Attempts to save you from death by nexusing before a fatal blow.\n" +
                "Unlike other auto nexus systems, this one compensates for piercing attacks, broken armor, and even ground damage*.\n" +
                "This plugin will NOT make you completely invulnerable, but it will definitely help prevent you from dying!\n\n" +
                "*The exact damage value cannot be determined when taking ground damage, so for safety's sake the plugin will assume that you took the maximum damage possible for the appropriate tile.";
        }

        public string GetName()
        {
            return "Auto Nexus";
        }

        private Dictionary<Client, ClientState> _clients;

        public void Initialize(Proxy proxy)
        {
            GameData.Objects.Map
                .ForEach(enemy =>
                {
                    // armor piercing
                    if (enemy.Value.Projectiles.Any(p => p.ArmorPiercing))
                    {
                        Bullet.Piercing[enemy.Value.ID] = new List<int>();
                        enemy.Value.Projectiles.ForEach(proj =>
                        {
                            if (proj.ArmorPiercing)
                            {
                                Bullet.Piercing[enemy.Value.ID].Add(proj.ID);
                            }
                        });
                    }

                    // armor break
                    if (enemy.Value.Projectiles.Any(p => p.StatusEffects.ContainsKey("Armor Broken")))
                    {
                        Bullet.Breaking[enemy.Value.ID] = new List<int>();
                        enemy.Value.Projectiles.ForEach(proj =>
                        {
                            if (proj.StatusEffects.ContainsKey("Armor Broken"))
                            {
                                Bullet.Breaking[enemy.Value.ID].Add(proj.ID);
                            }
                        });
                    }
                });

            PluginUtils.Log("Auto Nexus", "Found {0} armor-piercing projectiles from {1} enemies.", Bullet.Piercing.Sum(e => e.Value.Count), Bullet.Piercing.Count);
            PluginUtils.Log("Auto Nexus", "Found {0} armor-breaking projectiles from {1} enemies.", Bullet.Breaking.Sum(e => e.Value.Count), Bullet.Breaking.Count);

            _clients = new Dictionary<Client, ClientState>();

            proxy.ClientConnected += OnConnect;
            proxy.ClientDisconnected += OnDisconnect;

            proxy.HookPacket(PacketType.UPDATE, OnPacket);
            proxy.HookPacket(PacketType.NEWTICK, OnPacket);
            proxy.HookPacket(PacketType.ENEMYSHOOT, OnPacket);
            proxy.HookPacket(PacketType.PLAYERHIT, OnPacket);
            proxy.HookPacket(PacketType.AOE, OnPacket);
            proxy.HookPacket(PacketType.GROUNDDAMAGE, OnPacket);
            proxy.HookPacket(PacketType.SERVERPLAYERSHOOT, OnPacket);

            proxy.HookCommand("autonexus", OnCommand);

            // force map cacher to load
            MapCacher.MapCacher.ForceLoad();
        }


        private static void OnCommand(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            {
                client.OryxMessage("Auto Nexus is {0}.", Config.Default.Enabled ? "enabled" : "disabled");
                if (Config.Default.Enabled)
                {
                    client.OryxMessage("You will be sent to the nexus at <={0}% HP.", (int) (Config.Default.NexusPercent * 100));
                }
            }
            else
            {
                switch (args[0])
                {
                    case "on":
                    case "enable":
                        Config.Default.Enabled = true;
                        Config.Default.Save();
                        client.OryxMessage("Auto Nexus now enabled.");
                        break;
                    case "off":
                    case "disable":
                        Config.Default.Enabled = false;
                        Config.Default.Save();
                        client.OryxMessage("Auto Nexus now disabled.");
                        break;
                    case "debug":
                        switch (args[1])
                        {
                            case "on":
                            case "enable":
                                Config.Default.Debug = true;
                                Config.Default.Save();
                                client.OryxMessage("Debug output enabled.");
                                break;
                            case "off":
                            case "disable":
                                Config.Default.Debug = false;
                                Config.Default.Save();
                                client.OryxMessage("Debug output disabled.");
                                break;
                            default:
                                client.OryxMessage("Unrecognized argument: {0}", args[0]);
                                client.OryxMessage("Usage:");
                                client.OryxMessage("'/autonexus on' - enable autonexus");
                                client.OryxMessage("'/autonexus off' - disable autonexus");
                                client.OryxMessage("'/autonexus 10' - set autonexus percentage to 10%");
                                break;
                        }
                        break;
                    default:
                        int percentage;
                        if (int.TryParse(args[0], out percentage))
                        {
                            if (percentage > 99 || percentage < 0)
                            {
                                client.OryxMessage("Percentage should be between 0 and 99, inclusive.");
                            }
                            else
                            {
                                Config.Default.NexusPercent = (float) percentage / 100;
                                Config.Default.Save();
                                client.OryxMessage("Auto Nexus percentage set to {0}%.", percentage);
                            }
                        }
                        else
                        {
                            client.OryxMessage("Unrecognized argument: {0}", args[0]);
                            client.OryxMessage("Usage:");
                            client.OryxMessage("'/autonexus on' - enable autonexus");
                            client.OryxMessage("'/autonexus off' - disable autonexus");
                            client.OryxMessage("'/autonexus 10' - set autonexus percentage to 10%");
                        }
                        break;
                }
            }
        }

        private void OnConnect(Client client)
        {
            _clients[client] = new ClientState(client);
        }

        private void OnDisconnect(Client client)
        {
            if (_clients.ContainsKey(client))
            { _clients.Remove(client); }
        }

        private void OnPacket(Client client, Packet p)
        {
            if (!_clients.ContainsKey(client))
            { return; }

            ClientState state = _clients[client];
            switch (p.Type)
            {
                case PacketType.UPDATE:
                    state.Update(p as UpdatePacket);
                    break;
                case PacketType.NEWTICK:
                    state.Tick(p as NewTickPacket);
                    break;
                case PacketType.ENEMYSHOOT:
                    state.EnemyShoot(p as EnemyShootPacket);
                    break;
                case PacketType.PLAYERHIT:
                    state.PlayerHit(p as PlayerHitPacket);
                    break;
                case PacketType.AOE:
                    state.AoE(p as AoEPacket);
                    break;
                case PacketType.GROUNDDAMAGE:
                    state.GroundDamage(p as GroundDamagePacket);
                    break;
                case PacketType.SERVERPLAYERSHOOT:
                    state.Shoot2(p as ServerPlayerShootPacket);
                    break;
            }
        }
    }
}
