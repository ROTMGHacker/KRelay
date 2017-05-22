using System.Collections.Generic;
using System.Xml.Linq;
using Lib_K_Relay;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Utilities;

namespace DyeSkinFaker
{
    public class DyeSkinFaker : IPlugin
    {
        public static Dictionary<int, string> LargeDyes = new Dictionary<int, string>();
        public static Dictionary<int, string> SmallDyes = new Dictionary<int, string>();
        public static Dictionary<int, string> Skins = new Dictionary<int, string>();

        public string GetAuthor()
        { return "Todddddd"; }

        public string GetName()
        { return "Dye/Skin Faker"; }

        public string GetDescription()
        {
            return "Fake your dye or skin.\nUse \"/dyefaker\" to open the settings.";
        }

        public string[] GetCommands()
        {
            return new[]
            {
                "/dyefaker",
                "/dyefaker <Enable|Disable>"
            };
        }

        public void Initialize(Proxy proxy)
        {
            // Add empty values to all the dictionary's so that way there is a blank option on the config drop downs
            if (!LargeDyes.ContainsKey(0))
            { LargeDyes.Add(0, ""); }

            if (!SmallDyes.ContainsKey(0))
            { SmallDyes.Add(0, ""); }

            if (!Skins.ContainsKey(0))
            { Skins.Add(0, ""); }

            // Go through the RAW xml from the objects file and get all the skin and dyes
            XDocument doc = XDocument.Parse(GameData.RawObjectsXML);
            XElement xElement = doc.Element("Objects");
            xElement?.Elements("Object")
                .ForEach(obj =>
                {
                    string className = obj.ElemDefault("Class", "");
                    string name = obj.AttrDefault("id", "");

                    // Check if the class is a Dye
                    if (className == "Dye")
                    {
                        if (obj.HasElement("Tex1"))
                        {
                            // Large Dye
                            XElement element = obj.Element("Tex1");
                            if (element != null)
                            {
                                int id = element.Value.ParseHex();
                                if (!LargeDyes.ContainsKey(id))
                                {
                                    LargeDyes.Add(id, name);
                                }
                            }
                        }
                        else if (obj.HasElement("Tex2"))
                        {
                            // Small Dye
                            XElement element = obj.Element("Tex2");
                            if (element != null)
                            {
                                int id = element.Value.ParseHex();
                                if (!SmallDyes.ContainsKey(id))
                                {
                                    SmallDyes.Add(id, name);
                                }
                            }
                        }
                    }

                    // Check if the class is Skin
                    if (className == "Skin" /*&& obj.HasElement("Skin")*/)
                    {
                        int id = obj.AttrDefault("type", "0x0").ParseHex();
                        if (!Skins.ContainsKey(id))
                        {
                            Skins.Add(id, name);
                        }
                    }

                    // Double check if we have all the skins by checking the equipment with "skinType" attributes
                    if (obj.HasElement("Activate"))
                    {
                        IEnumerable<XAttribute> attributes = obj.Element("Activate")?.Attributes();
                        if (attributes == null)
                        { return; }

                        foreach (XAttribute attr in attributes)
                        {
                            if (attr.Name == "skinType")
                            {
                                if (!Skins.ContainsKey(attr.Value.ParseInt()))
                                {
                                    Skins.Add(attr.Value.ParseInt(), name);
                                }

                                break;
                            }
                        }
                    }
                });

            //proxy.ClientConnected += OnConnect;

            proxy.HookPacket(PacketType.UPDATE, OnUpdate);
            //proxy.HookPacket(PacketType.UPDATEPET, OnUpdatePet);
            //proxy.HookPacket(PacketType.RESKIN, OnReskin);
            //proxy.HookPacket(PacketType.CLIENTSTAT, OnClientStat);
            //proxy.HookPacket(PacketType.RESKINUNLOCK, OnReskinUnlock);

            proxy.HookCommand("dyefaker", OnCommand);
        }

        /*public void OnReskinUnlock(Client client, Packet packet)
        {
            ReskinUnlock ru = (ReskinUnlock)packet;
            Console.WriteLine("ReskinUnlock: {0}", ru.SkinID);
        }*/


        public void OnCommand(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            {
                PluginUtils.ShowGUI(new FrmConfig(client));
            }
            else
            {
                switch (args[0])
                {
                    case "enable":
                    case "on":
                        Config.Default.Enabled = true;
                        break;
                    case "disable":
                    case "off":
                        Config.Default.Enabled = false;
                        break;
                    default:
                        client.SendToClient(PluginUtils.CreateOryxNotification("DyeSkinFaker", "Unrecognized command: " + args[0]));
                        break;
                }

                Config.Default.Save();
            }
        }

        /*public void OnReskin(Client client, Packet packet)
        {
            ReskinPacket rp = (ReskinPacket)packet;
            Console.WriteLine("Reskin: {0}", rp.SkinID);
        }
        
        public void OnUpdatePet(Client client, Packet packet)
        {
            UpdatePetPacket upp = (UpdatePetPacket)packet;
            //Console.WriteLine("PetId: {0}", upp.PetId);
            //petid = upp.PetId;
        }*/

        /*public void OnConnect(Client client)
        {
        }

        public static void OnChange(Client client)
        {
            ReskinUnlock reskin = (ReskinUnlock)Packet.Create(PacketType.RESKINUNLOCK);
            reskin.SkinID = Config.Default.Skin;
            client.SendToClient(reskin);
        }*/

        public void OnUpdate(Client client, Packet packet)
        {
            UpdatePacket up = (UpdatePacket) packet;
            if (!Config.Default.Enabled)
            { return; }

            foreach (Entity ent in up.NewObjs)
            {
                if (ent.Status.ObjectId != client.ObjectId)
                { continue; }

                foreach (StatData data in ent.Status.Data)
                {
                    switch ((int) data.Id)
                    {
                        case 32:
                            if (Config.Default.LargeDye > 0)
                            {
                                data.IntValue = Config.Default.LargeDye;
                            }
                            break;
                        case 33:
                            if (Config.Default.SmallDye > 0)
                            {
                                data.IntValue = Config.Default.SmallDye;
                            }
                            break;
                        case 80:
                            if (Config.Default.Skin > 0)
                            {
                                data.IntValue = Config.Default.Skin;
                            }
                            break;
                    }
                }
            }
        }
    }
}
