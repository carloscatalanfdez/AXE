using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

using AXE.Game.Entities;

namespace AXE.Game.Entities
{
    class LevelMap : bEntity
    {
        int tileWidth = 8;
        int tileHeight = 8;

        public String mapName;
        public String tsetFilename;

        public bTilemap tilemap;
        public List<bEntity> entities;

        public String name;
        public Vector2 playerStart;

        public LevelMap(String fname)
            : base(0, 0)
        {
            entities = new List<bEntity>();

            mapName = fname;
        }

        override public void init()
        {
            base.init();

            String filename = "Assets/Maps/" + mapName + ".oel";

            Stack<String> parseStack = new Stack<String>();

            int w = 0, h = 0;
            string tileset;
            string exportMode;
            string[] tiles = { "" }, solids = { "" };

            using (var stream = System.IO.File.OpenText(filename))
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (parseStack.Count > 0 && parseStack.Peek() == "Entities")
                        {
                            bEntity e = parseEntity(reader);
                            if (e != null)
                                entities.Add(e);
                        }
                        else
                        {
                            parseStack.Push(reader.Name);

                            switch (reader.Name)
                            {
                                case "level":
                                    w = int.Parse(reader.GetAttribute("width"));
                                    h = int.Parse(reader.GetAttribute("height"));
                                    name = reader.GetAttribute("name");
                                    break;
                                case "Tiles":
                                    tileset = reader.GetAttribute("tileset");
                                    break;
                                case "Solids":
                                    exportMode = reader.GetAttribute("exportMode");
                                    break;
                                case "Entities":
                                    break;
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        String current = parseStack.Pop();
                        switch (current)
                        {
                            case "level":
                                break;
                            case "Tiles":
                                string v = reader.Value;
                                tiles = v.Split('\n');
                                break;
                            case "Solids":
                                v = reader.Value;
                                solids = v.Split('\n');
                                break;
                        }
                        parseStack.Push(current);
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        parseStack.Pop();
                    }
                }
            }

            tilemap = new bTilemap(w, h, tileWidth, tileHeight, game.Content.Load<Texture2D>("Assets/Tilesets/" + tsetFilename));
            tilemap.parseTiles(tiles);

            mask = new bSolidGrid(w / tileWidth, h / tileHeight, tileWidth, tileHeight);
            mask.game = game;
            (mask as bSolidGrid).parseSolids(solids);
        }

        public bEntity parseEntity(XmlReader element)
        {
            bEntity ge = null;

            // Fetch common attributes
            int id, x, y;
            id = int.Parse(element.GetAttribute("id"));
            x = int.Parse(element.GetAttribute("x"));
            y = int.Parse(element.GetAttribute("y"));

            // Create entitiy by element name
            switch (element.Name)
            {
                case "Entry":
                    // TODO: handle entry
                    break;
                case "Exit":
                    // TODO: handle exit
                    break;
            }

            if (ge != null)
                ge.id = id;

            return ge;
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            tilemap.render(sb, pos);
            base.render(dt, sb);
        }
    }
}
