//The MIT License (MIT)
//
//Copyright (c) 2015 Fabian Fischer
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using Lib_K_Relay.GameData;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.Networking.Packets.Server;
using Newtonsoft.Json;

namespace MapRipper
{
    public class JsonMap
    {
        public string Name { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int[][] Tiles { get; private set; }
        public Entity[][][] Entities { get; private set; }

        private int _currentTiles;

        public delegate void TilesChangedDelegate(int currentTiles);

        public event TilesChangedDelegate TilesAdded;

        public void Init(int w, int h, string name)
        {
            Width = w;
            Height = h;
            Tiles = new int[w][];
            Name = name;
            for (int i = 0; i < w; i++)
            {
                Tiles[i] = new int[h];
            }

            for (int width = 0; width < w; width++)
            {
                for (int height = 0; height < h; height++)
                {
                    Tiles[width][height] = -1;
                }
            }

            Entities = new Entity[w][][];
            for (int i = 0; i < w; i++)
            {
                Entities[i] = new Entity[h][];
                for (int j = 0; j < h; j++)
                {
                    Entities[i][j] = new Entity[0];
                }
            }
        }

        private struct Obj
        {
            public string Name;
            public string ID;
        }
        private struct Loc
        {
            public string Ground;
            public Obj[] Objs;
            /*
                        private Obj[] _regions;
            */
        }
        private struct JsonDat
        {
            public byte[] Data;
            public int Width;
            public int Height;
            public Loc[] Dict;
        }

        public void Update(UpdatePacket packet)
        {
            foreach (Tile t in packet.Tiles)
            {
                Tiles[t.X][t.Y] = t.Type;
                _currentTiles++;
                TilesAdded?.Invoke(_currentTiles);
            }

            foreach (Entity tileDef in packet.NewObjs)
            {
                Entity def = (Entity) tileDef.Clone();

                if (!IsMapObject(def.ObjectType))
                { continue; }

                def.Status.Position.X -= 0.5F;
                def.Status.Position.Y -= 0.5F;

                int x = (int) def.Status.Position.X;
                int y = (int) def.Status.Position.Y;
                Array.Resize(ref Entities[x][y], Entities[x][y].Length + 1);
                Entity[] arr = Entities[x][y];

                arr[arr.Length - 1] = def;
            }
        }

        private static bool IsMapObject(short objType)
        {
            return true; // Todo: check if player or pet or all that stuff you dont place on the map with the editor
        }

        public string ToJson()
        {
            JsonDat obj = new JsonDat
            {
                Width = Width,
                Height = Height
            };
            List<Loc> locs = new List<Loc>();
            MemoryStream ms = new MemoryStream();
            using (PacketWriter wtr = new PacketWriter(ms))
            {
                for (int y = 0; y < obj.Height; y++)
                {
                    for (int x = 0; x < obj.Width; x++)
                    {
                        Loc loc = new Loc
                        {
                            Ground = Tiles[x][y] != -1 ? GetTileID((ushort) Tiles[x][y]) : null,
                            Objs = new Obj[Entities[x][y].Length]
                        };
                        for (int i = 0; i < loc.Objs.Length; i++)
                        {
                            Entity en = Entities[x][y][i];
                            Obj o = new Obj
                            {
                                ID = GetEntityID(en.ObjectType)
                            };
                            string s = "";
                            Dictionary<StatsType, object> vals = new Dictionary<StatsType, object>();
                            foreach (StatData z in en.Status.Data)
                            {
                                vals.Add(z.Id, z.IsStringData() ? z.StringValue : (object) z.IntValue);
                            }

                            if (vals.ContainsKey(StatsType.Name))
                            { s += ";Name:" + vals[StatsType.Name]; }
                            if (vals.ContainsKey(StatsType.Size))
                            { s += ";size:" + vals[StatsType.Size]; }
                            if (vals.ContainsKey(StatsType.ObjectConnection))
                            { s += ";conn:0x" + ((int) vals[StatsType.ObjectConnection]).ToString("X8"); }
                            if (vals.ContainsKey(StatsType.MerchandiseType))
                            { s += ";mtype:" + vals[StatsType.MerchandiseType]; }
                            if (vals.ContainsKey(StatsType.MerchandiseRemainingCount))
                            { s += ";mcount:" + vals[StatsType.MerchandiseRemainingCount]; }
                            if (vals.ContainsKey(StatsType.MerchandiseRemainingMinutes))
                            { s += ";mtime:" + vals[StatsType.MerchandiseRemainingMinutes]; }
                            if (vals.ContainsKey(StatsType.RankRequired))
                            { s += ";nstar:" + vals[StatsType.RankRequired]; }

                            o.Name = s.Trim(';');
                            loc.Objs[i] = o;
                        }

                        int ix = -1;
                        for (int i = 0; i < locs.Count; i++)
                        {
                            if (locs[i].Ground != loc.Ground)
                            { continue; }
                            if (!(locs[i].Objs != null && loc.Objs != null ||
                              locs[i].Objs == null && loc.Objs == null))
                            { continue; }

                            if (locs[i].Objs != null)
                            {
                                if (locs[i].Objs.Length != loc.Objs.Length)
                                { continue; }

                                bool b = false;
                                for (int j = 0; j < loc.Objs.Length; j++)
                                {
                                    if (locs[i].Objs[j].ID != loc.Objs[j].ID ||
                                        locs[i].Objs[j].Name != loc.Objs[j].Name)
                                    {
                                        b = true;
                                        break;
                                    }
                                }

                                if (b)
                                { continue; }
                            }

                            ix = i;
                            break;
                        }
                        if (ix == -1)
                        {
                            ix = locs.Count;
                            locs.Add(loc);
                        }
                        wtr.Write((short) ix);
                    }
                }
            }

            obj.Data = ZlibStream.CompressBuffer(ms.ToArray());
            obj.Dict = locs.ToArray();
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(obj, settings);
        }

        private static string GetEntityID(short type)
        {
            if (GameData.Tiles.Map.ContainsKey((ushort) type))
            { return GameData.Tiles.ByID((ushort) type).Name; }
            if (GameData.Objects.Map.ContainsKey((ushort) type))
            { return GameData.Objects.ByID((ushort) type).Name; }

            throw new Exception("Invalid value: " + type);
        }

        private static string GetTileID(ushort type)
        {
            return GameData.Tiles.ByID(type).Name;
        }
    }
}