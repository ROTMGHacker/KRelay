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

namespace TraderBot
{

    public class TradeItem
    {
        public Item Idata;
        public bool Selected;

        public TradeItem()
        {
            Idata = null;
            Selected = false;
        }
        public TradeItem(Item i)
        {
            Idata = i;
            Selected = false;
        }
        public string GetName()
        {
            try
            {
                return GameData.Items.ByID((ushort) Idata.ItemItem).Name;
            }
            catch
            {
                return "(Empty)";
            }
        }
    }

    public class Trade
    {
        public List<TradeItem> MyItems;
        public List<TradeItem> YourItems;

        public List<ItemStruct> MasterBuying;
        public List<ItemStruct> MasterSelling;

        public bool IsTrading;
        public Client C;


        public Trade()
        {
            MyItems = new List<TradeItem>();
            YourItems = new List<TradeItem>();

            IsTrading = false;
        }

        public Trade(Client cli, TradeStartPacket tsp)
        {
            C = cli;

            MyItems = new List<TradeItem>();
            YourItems = new List<TradeItem>();

            MasterBuying = TraderBot.Buying[C].Select(item => new ItemStruct(item.Name, item.Qty, item.Alt, item.AltQty)).ToList();
            MasterSelling = TraderBot.Selling[C].Select(item => new ItemStruct(item.Name, item.Qty)).ToList();

            IsTrading = true;

            foreach (Item item in tsp.YourItems)
            {
                YourItems.Add(new TradeItem(item));
            }
            foreach (Item item in tsp.MyItems)
            {
                MyItems.Add(new TradeItem(item));
            }

            /*foreach (var item in YourItems)
            {
                Console.WriteLine(item.GetName());
            }*/
        }

        public void UpdateTrade(bool[] o)
        {
            for (int i = 0; i < o.Length; i++)
            {
                YourItems[i].Selected = o[i];
            }
        }

        public void AcceptOffers(bool[] o)
        {
            for (int i = 0; i < o.Length; i++)
            {
                YourItems[i].Selected = o[i];
            }

            if (Good())
            {
                Accept();
            }
        }

        public void Accept()
        {
            bool[] yo = new bool[12];
            bool[] mo = new bool[12];

            for (int i = 0; i < 12; i++)
            {
                if (MyItems[i].Selected)
                { mo[i] = true; }
                else
                { mo[i] = false; }

                if (YourItems[i].Selected)
                { yo[i] = true; }
                else
                { yo[i] = false; }
            }

            //Console.WriteLine(_c.PlayerData.Name + " Sending AcceptTrade");
            AcceptTradePacket atp = (AcceptTradePacket) Packet.Create(PacketType.ACCEPTTRADE);
            atp.YourOffers = yo;
            atp.MyOffers = mo;
            C.SendToServer(atp);
        }

        public bool Good()
        {
            int x = 0;
            // Check if they selected all the items we are buying and if they did 2x or 3x or 4x
            List<ItemStruct> tmpYList = MasterBuying.Select(itemb => new ItemStruct(itemb.Name, itemb.Qty, itemb.Alt, itemb.AltQty)).ToList();
            foreach (TradeItem item in YourItems)
            {
                if (item.Selected)
                {
                    for (int i = 0; i < tmpYList.Count; i++)
                    {
                        if (tmpYList[i].Name == item.GetName())
                        {
                            if (tmpYList[i].Qty > 0)
                            { tmpYList[i].Qty--; }

                            break;
                        }
                        if (tmpYList[i].Alt == item.GetName())
                        {
                            if (tmpYList[i].AltQty > 0)
                            { tmpYList[i].AltQty--; }

                            break;
                        }
                    }
                }

                bool all = true;
                for (int i = 0; i < tmpYList.Count; i++)
                {
                    if (tmpYList[i].Qty > 0)
                    {
                        if (tmpYList[i].AltQty == -1)
                        {
                            all = false;
                        }
                        else
                        {
                            if (tmpYList[i].AltQty > 0)
                            {
                                all = false;
                            }
                        }
                    }
                }
                if (!all)
                { continue; }

                x++;
                // Reset the Qty of whichever one just hit 0
                for (int i = 0; i < tmpYList.Count; i++)
                {
                    if (tmpYList[i].Qty == 0)
                    {
                        tmpYList[i] = new ItemStruct(MasterBuying[i].Name, MasterBuying[i].Qty, MasterBuying[i].Alt, tmpYList[i].AltQty);
                    }
                    else if (tmpYList[i].AltQty == 0)
                    {
                        tmpYList[i] = new ItemStruct(MasterBuying[i].Name, tmpYList[i].Qty, MasterBuying[i].Alt, MasterBuying[i].AltQty);
                    }
                }
                //tmpYList = MasterBuying.Select(itemb => new ItemStruct(itemb.Name, itemb.Qty, itemb.Alt, itemb.AltQty)).ToList();
            }

            if (x == 0)
            { return false; }

            bool changes = false;

            List<ItemStruct> tmpMList = MasterSelling.Select(itemb => new ItemStruct(itemb.Name, itemb.Qty)).ToList();

            // Change the quantity to match the multiplier
            foreach (ItemStruct item in tmpMList)
            { item.Qty *= x; }

            // Check if we selected the right number of items
            foreach (TradeItem item in MyItems)
            {
                if (!item.Selected)
                { continue; }

                bool found = false;
                foreach (ItemStruct itemStruct in tmpMList)
                {
                    if (itemStruct.Name == item.GetName())
                    {
                        if (itemStruct.Qty > 0)
                        {
                            found = true;
                            itemStruct.Qty--;
                        }
                        break;
                    }
                }
                if (found)
                { continue; }

                // This item shouldnt of been selected?
                item.Selected = false;
                changes = true;
            }

            if (changes)
            {
                bool[] offer = new bool[12];
                for (int i = 0; i < 12; i++)
                {
                    if (MyItems[i].Selected)
                    { offer[i] = true; }
                    else
                    { offer[i] = false; }
                }

                //Console.WriteLine(_c.PlayerData.Name + " Sending ChangeTrade");
                // Send the change
                ChangeTradePacket ctp = (ChangeTradePacket) Packet.Create(PacketType.CHANGETRADE);
                ctp.Offers = offer;
                C.SendToServer(ctp);

                return false;
            }

            return true;
        }

        public void SelectItems()
        {
            int x = 0;
            // Check if they selected all the items we are buying and if they did 2x or 3x or 4x
            List<ItemStruct> tmpYList = MasterBuying.Select(itemb => new ItemStruct(itemb.Name, itemb.Qty, itemb.Alt, itemb.AltQty)).ToList();
            for (int j = 4; j < YourItems.Count; j++)
            {
                if (YourItems[j].Selected)
                {
                    foreach (ItemStruct itemStruct in tmpYList)
                    {
                        if (itemStruct.Name == YourItems[j].GetName())
                        {
                            if (itemStruct.Qty > 0)
                            { itemStruct.Qty--; }

                            break;
                        }
                        if (itemStruct.Alt == YourItems[j].GetName())
                        {
                            if (itemStruct.AltQty > 0)
                            { itemStruct.AltQty--; }

                            break;
                        }
                    }
                }

                bool all = true;
                foreach (ItemStruct itemStruct in tmpYList)
                {
                    if (itemStruct.Qty > 0)
                    {
                        if (itemStruct.AltQty == -1)
                        {
                            all = false;
                        }
                        else
                        {
                            if (itemStruct.AltQty > 0)
                            {
                                all = false;
                            }
                        }
                    }
                }

                if (all)
                {
                    x++;
                    for (int i = 0; i < tmpYList.Count; i++)
                    {
                        if (tmpYList[i].Qty == 0)
                        {
                            tmpYList[i] = new ItemStruct(MasterBuying[i].Name, MasterBuying[i].Qty, MasterBuying[i].Alt, tmpYList[i].AltQty);
                        }
                        else if (tmpYList[i].AltQty == 0)
                        {
                            tmpYList[i] = new ItemStruct(MasterBuying[i].Name, tmpYList[i].Qty, MasterBuying[i].Alt, MasterBuying[i].AltQty);
                        }
                    }
                    //tmpYList = MasterBuying.Select(itemb => new ItemStruct(itemb.Name, itemb.Qty, itemb.Alt, itemb.AltQty)).ToList();
                }
            }

            // Clear all the selected items from our list
            for (int j = 4; j < MyItems.Count; j++)
            { MyItems[j].Selected = false; }

            // Select all the items we are trading
            List<ItemStruct> tmpMList = MasterSelling.Select(itemb => new ItemStruct(itemb.Name, itemb.Qty)).ToList();
            // Change the quantity to match the multiplier
            foreach (ItemStruct itemStruct in tmpMList)
            { itemStruct.Qty *= x; }

            for (int j = 4; j < MyItems.Count; j++)
            {
                if (!MyItems[j].Selected)
                {
                    foreach (ItemStruct itemStruct in tmpMList)
                    {
                        if (itemStruct.Name == MyItems[j].GetName())
                        {
                            if (itemStruct.Qty > 0)
                            {
                                itemStruct.Qty--;
                                MyItems[j].Selected = true;
                            }
                            break;
                        }
                    }
                }
            }

            bool[] offer = new bool[12];
            for (int i = 0; i < 12; i++)
            {
                if (MyItems[i].Selected)
                { offer[i] = true; }
                else
                { offer[i] = false; }
            }

            //Console.WriteLine(_c.PlayerData.Name + " Sending ChangeTrade");
            // Send the change
            ChangeTradePacket ctp = (ChangeTradePacket) Packet.Create(PacketType.CHANGETRADE);
            ctp.Offers = offer;
            C.SendToServer(ctp);
        }

        public bool HasItems()
        {
            // Check if they have all the items we are trading for
            List<ItemStruct> tmpYList = MasterBuying.Select(itemb => new ItemStruct(itemb.Name, itemb.Qty, itemb.Alt, itemb.AltQty)).ToList();
            foreach (TradeItem item in YourItems)
            {
                foreach (ItemStruct itemStruct in tmpYList)
                {
                    if (itemStruct.Name == item.GetName())
                    {
                        if (itemStruct.Qty > 0)
                        { itemStruct.Qty--; }

                        break;
                    }
                    if (itemStruct.Alt == item.GetName())
                    {
                        if (itemStruct.AltQty > 0)
                        { itemStruct.AltQty--; }

                        break;
                    }
                }
            }

            foreach (ItemStruct itemStruct in tmpYList)
            {
                if (itemStruct.Qty > 0)
                {
                    if (itemStruct.AltQty == -1)
                    {
                        return false;
                    }
                    if (itemStruct.AltQty > 0)
                    {
                        return false;
                    }
                }
            }

            // Check if we have all the items we are trading as well
            List<ItemStruct> tmpMList = MasterSelling.Select(itemb => new ItemStruct(itemb.Name, itemb.Qty, itemb.Alt, itemb.AltQty)).ToList();

            foreach (TradeItem item in MyItems)
            {
                foreach (ItemStruct itemStruct in tmpMList)
                {
                    if (itemStruct.Name == item.GetName())
                    {
                        if (itemStruct.Qty > 0)
                        { itemStruct.Qty--; }

                        break;
                    }
                }
            }

            return tmpMList.All(itemStruct => itemStruct.Qty <= 0);
        }
    }

    public class TraderBot : IPlugin
    {
        public static Dictionary<Client, List<ItemStruct>> Buying = new Dictionary<Client, List<ItemStruct>>();
        public static Dictionary<Client, List<ItemStruct>> Selling = new Dictionary<Client, List<ItemStruct>>();
        public static Dictionary<Client, string> SpamMsg = new Dictionary<Client, string>();

        public static Dictionary<Client, bool> Enabled = new Dictionary<Client, bool>();
        public static Dictionary<Client, bool> Started = new Dictionary<Client, bool>();
        public static Dictionary<Client, Trade> CurrentTrades = new Dictionary<Client, Trade>();

        //public Dictionary<Client, Stopwatch> idelTimer = new Dictionary<Client, Stopwatch>();
        //public Dictionary<Client, bool> hasMoved = new Dictionary<Client, bool>();
        //public Dictionary<Client, bool> blockGotoAck = new Dictionary<Client, bool>();
        //public Stopwatch idleTimer = new Stopwatch();
        //public bool moved;

        public string GetAuthor()
        { return "Todddddd"; }

        public string GetName()
        { return "TraderBot"; }

        public string GetDescription()
        { return "Handles trading for you."; }

        public string[] GetCommands()
        {
            return new[] 
            {
                "/trader <Enable|Disable>",
                "/trader settings",
                "/trader <start|stop>"
            };
        }

        public void Initialize(Proxy proxy)
        {
            proxy.ClientDisconnected += OnDisconnect;
            proxy.ClientConnected += OnConnect;

            proxy.HookPacket(PacketType.UPDATE, OnUpdate);
            proxy.HookPacket(PacketType.NEWTICK, OnTick);
            //proxy.HookPacket(PacketType.GOTOACK, OnGotoAck);

            proxy.HookPacket(PacketType.TRADEREQUESTED, OnTradeRequested);
            proxy.HookPacket(PacketType.TRADEACCEPTED, OnTradeAccepted);
            proxy.HookPacket(PacketType.TRADECHANGED, OnTradeChanged);
            proxy.HookPacket(PacketType.TRADESTART, OnTradeStart);
            proxy.HookPacket(PacketType.TRADEDONE, OnTradeDone);
            proxy.HookPacket(PacketType.CANCELTRADE, OnCancelTrade);

            proxy.HookCommand("trader", OnCommand);
        }

        public void OnTradeRequested(Client client, Packet packet)
        {
            TradeRequestedPacket request = (TradeRequestedPacket) packet;
            if (Enabled[client] && Started[client] && !CurrentTrades[client].IsTrading)
            {
                Console.WriteLine(@"Trade Request from """ + request.Name + "\"");
                RequestTradePacket req = (RequestTradePacket) Packet.Create(PacketType.REQUESTTRADE);
                req.Name = request.Name;
                client.SendToServer(req);
            }
        }

        public void OnTradeStart(Client client, Packet packet)
        {
            TradeStartPacket start = (TradeStartPacket) packet;
            if (Enabled[client] && Started[client])
            {
                Console.WriteLine(@"Trade Started with """ + start.YourName + "\"");
                //CurrentTrade = new Trade(client, start);
                CurrentTrades[client] = new Trade(client, start);

                //if (CurrentTrade.HasItems())
                if (CurrentTrades[client].HasItems())
                {
                    Console.WriteLine(@"Have Item!");
                    // Wait 30 seconds and then cancel the trade if it is still going
                    PluginUtils.Delay(30000, () =>
                    {
                        if (CurrentTrades[client].IsTrading)
                        {
                            client.SendToServer(Packet.Create(PacketType.CANCELTRADE));
                            CurrentTrades[client].IsTrading = false;
                            Console.WriteLine("Canceling Trade Due To Time!");
                        }
                    });
                }
                else
                {
                    Console.WriteLine(@"No Have Item!");
                    // Wait 4 seconds and then cancel the trade
                    PluginUtils.Delay(4000, () =>
                    {
                        if (CurrentTrades[client].IsTrading)
                        {
                            client.SendToServer(Packet.Create(PacketType.CANCELTRADE));
                            CurrentTrades[client].IsTrading = false;
                            Console.WriteLine(@"Canceling Trade Due To Missing Items!");
                        }
                    });
                }
            }
        }

        public void OnTradeChanged(Client client, Packet packet)
        {
            TradeChangedPacket change = (TradeChangedPacket) packet;
            if (Enabled[client] && Started[client])
            {
                CurrentTrades[client].UpdateTrade(change.Offers);
                CurrentTrades[client].SelectItems();

                if (CurrentTrades[client].Good())
                {
                    CurrentTrades[client].Accept();
                }
            }
        }

        public void OnTradeAccepted(Client client, Packet packet)
        {
            TradeAcceptedPacket tap = (TradeAcceptedPacket) packet;
            if (Enabled[client] && Started[client])
            {
                CurrentTrades[client].AcceptOffers(tap.YourOffers);
            }
        }
        public void OnTradeDone(Client client, Packet packet)
        {
            if (Enabled[client] && Started[client])
            {
                Console.WriteLine(@"Trade Done!");
                CurrentTrades[client].IsTrading = false;
            }
        }

        public void OnCancelTrade(Client client, Packet packet)
        {
            if (Enabled[client] && Started[client])
            {
                Console.WriteLine(@"Cancel Trade!");
                CurrentTrades[client].IsTrading = false;
            }
        }

        public void OnCommand(Client client, string command, string[] args)
        {
            if (args.Length == 0)
            { return; }

            switch (args[0])
            {
                case "on":
                case "enable":
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "TraderBot Enabled!"));
                    Enabled[client] = true;
                    break;
                case "off":
                case "disable":
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "TraderBot Disabled!"));
                    Enabled[client] = false;
                    //idelTimer[client].Reset();
                    break;
                case "settings":
                    PluginUtils.ShowGUI(new FrmTraderBot(client));
                    break;
                case "start":
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "TraderBot Started!"));
                    if (Enabled[client])
                    {
                        Started[client] = true;
                        //idelTimer[client].Start();
                    }
                    break;
                case "stop":
                    client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "TraderBot Stopped!"));
                    if (Enabled[client])
                    {
                        Started[client] = false;
                        //idelTimer[client].Reset();
                    }
                    break;
                case "preset":
                    if (args[1] == "1")
                    {
                        Selling[client].Add(new ItemStruct("Potion of Speed", 8));
                        Buying[client].Add(new ItemStruct("Potion of Life", 1));
                        SpamMsg[client] = "B> LIFE 1:8 SPD :name:";
                        client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "TraderBot Using Preset 1!"));
                    }
                    else if (args[1] == "2")
                    {
                        Selling[client].Add(new ItemStruct("Potion of Defense", 6));
                        Buying[client].Add(new ItemStruct("Potion of Life", 1));
                        SpamMsg[client] = "B> LIFE (6 DEF) :name:";
                        client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "TraderBot Using Preset 2!"));
                    }
                    else if (args[1] == "3")
                    {
                        Selling[client].Add(new ItemStruct("Potion of Attack", 6));
                        Buying[client].Add(new ItemStruct("Potion of Life", 1));
                        SpamMsg[client] = "B> LIFE (6 ATK) :name:";
                        client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "TraderBot Using Preset 3!"));
                    }
                    break;
                case "login":
                    break;
                default:
                    client.SendToClient(PluginUtils.CreateOryxNotification("TraderBot", "Unrecognized command: " + args[0]));
                    break;
            }
        }

        private static void OnConnect(Client client)
        {
            Enabled.Add(client, false);
            Started.Add(client, false);
            Buying[client] = new List<ItemStruct>();
            Selling[client] = new List<ItemStruct>();
            //idelTimer.Add(client, new Stopwatch());
            //hasMoved.Add(client, false);
            //blockGotoAck.Add(client, false);
            CurrentTrades.Add(client, new Trade());
            SpamMsg.Add(client, "");
        }

        private static void OnDisconnect(Client client)
        {
            Buying.Remove(client);
            Selling.Remove(client);
            SpamMsg.Remove(client);
            Enabled.Remove(client);
            Started.Remove(client);
            CurrentTrades.Remove(client);
            //idelTimer.Remove(client);
            //hasMoved.Remove(client);
            //blockGotoAck.Remove(client);
        }

        private static void OnUpdate(Client client, Packet packet)
        {
        }

        private void OnTick(Client client, Packet packet)
        {
            NewTickPacket ntp = (NewTickPacket) packet;
            if (Enabled[client] && Started[client])
            {
                if (ntp.TickId % 4 == 0 && !CurrentTrades[client].IsTrading)
                {
                    SendMessage(client);
                }

                // If its been more then 60 seconds
                //if (idelTimer[client].ElapsedMilliseconds > 60000)
                //{
                //blockGotoAck[client] = true;
                //GotoPacket go = (GotoPacket)Packet.Create(PacketType.GOTO);
                //go.ObjectID = client.ObjectID;
                //go.Location = new Location();
                //if (hasMoved[client])
                //{
                //	go.Location.X = client.PlayerData.Pos.X - 0.83f;
                //	go.Location.Y = client.PlayerData.Pos.Y - 0.13f;
                //}
                //else
                //{
                //	go.Location.X = client.PlayerData.Pos.X + 0.83f;
                //	go.Location.Y = client.PlayerData.Pos.Y + 0.13f;
                //}
                //client.SendToClient(go);
                //hasMoved[client] = !hasMoved[client];
                //idelTimer[client].Restart();
                //}
            }
        }

        //public void OnGotoAck(Client client, Packet packet)
        //{
        //	GotoAckPacket gap = (GotoAckPacket)packet;
        //	if (blockGotoAck[client])
        //	{
        //		gap.Send = false;
        //		blockGotoAck[client] = false;
        //		return;
        //	}
        //}

        public void SendMessage(Client client)
        {
            // Check if we have all the items we are selling
            List<ItemStruct> tmpMList = Selling[client].Select(itemb => new ItemStruct(itemb.Name, itemb.Qty)).ToList();
            for (int j = 4; j < 12; j++)
            {
                foreach (ItemStruct itemStruct in tmpMList)
                {
                    if (client.PlayerData.Slot[j] > 0 && itemStruct.Name ==
                        GameData.Items.ByID((ushort) client.PlayerData.Slot[j]).Name)
                    {
                        if (itemStruct.Qty > 0)
                        {
                            itemStruct.Qty--;
                        }

                        break;
                    }
                }
            }

            bool send = true;
            foreach (ItemStruct itemStruct in tmpMList)
            {
                if (itemStruct.Qty > 0)
                {
                    send = false;
                }
            }

            if (!send || SpamMsg[client] == "")
            { return; }

            PlayerTextPacket ptp = (PlayerTextPacket) Packet.Create(PacketType.PLAYERTEXT);
            ptp.Text = SpamMsg[client].Replace(":name:", "@" + client.PlayerData.Name);
            client.SendToServer(ptp);
        }
    }

    public class ItemStruct
    {
        public string Name;
        public int Qty;
        public string Alt;
        public int AltQty = -1;

        public ItemStruct(string n, int q)
        {
            Name = n;
            Qty = q;
            Alt = "";
            AltQty = -1;
        }
        public ItemStruct(string n, int q, string a, int aq)
        {
            Name = n;
            Qty = q;
            Alt = a;
            AltQty = aq;
        }
    }
}
