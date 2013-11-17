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
using AXE.Game.Entities.Contraptions;
using AXE.Game.Entities.Base;
using AXE.Game.Entities.Enemies;

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

        public List<IContraption> linkedContraptions;

        public String name;
        public Vector2 playerStart;

        public LevelMap(String fname)
            : base(0, 0)
        {
            entities = new List<bEntity>();
            linkedContraptions = new List<IContraption>();
            playerStart = Vector2.Zero;
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

            // Link contraptions with their entities
            foreach (IContraption contraption in linkedContraptions)
            {
                // No ids means no worth trying
                ContraptionRewardData contraptionRewardData = contraption.getContraptionRewardData();
                foreach (bEntity entity in entities)
                {
                    if (entity.id == contraptionRewardData.rewarderId
                        && entity is IRewarder)
                    {
                        contraption.setRewarder(entity as IRewarder);
                    }
                    else if (entity.id == contraptionRewardData.targetId)
                    {
                        contraptionRewardData.target = entity;
                    }
                }
            }

            tsetFilename = "basic";

            tilemap = new bTilemap(w, h, tileWidth, tileHeight, game.Content.Load<Texture2D>("Assets/Tilesets/" + tsetFilename));
            tilemap.parseTiles(tiles);

            mask = new bSolidGrid(w / tileWidth, h / tileHeight, tileWidth, tileHeight);
            mask.game = game;
            (mask as bSolidGrid).parseSolids(solids);

            layer = 20;
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
                case "ExitDoor":
                    bool open = bool.Parse(element.GetAttribute("open"));
                    ge = new Door(x, y, open ? Door.Type.ExitOpen : Door.Type.ExitClose);
                    break;
                case "PlayerStart":
                    playerStart = new Vector2(x, y);
                    ge = new Door(x, y, Door.Type.Entry);
                    break;
                case "Ladder":
                    int width = int.Parse(element.GetAttribute("width"));
                    int height = int.Parse(element.GetAttribute("height"));
                    ge = new Stairs(x, y, width, height);
                    break;
                case "OneWayPlatform":
                    ge = new OneWayPlatform(x, y);
                    break;
                case "Imp":
                    ge = new Imp(x, y);
                    break;
                case "EvilAxeHolder":
                    ge = new EvilAxeHolder(x, y);
                    break;
                case "FlameSpirit":
                    ge = new FlameSpirit(x, y);
                    break;
                case "Coin":
                    ge = new Coin(x, y);
                    break;
                case "FinishLevelContraption":
                    ge = new FinishLevelContraption();
                    break;
                case "ItemGenerator":
                    string type = element.GetAttribute("type");
                    ge = new ItemGenerator(type);
                    break;
                case "TrapDoor":
                    bool trapdoorOpen = bool.Parse(element.GetAttribute("open"));
                    ge = new TrapDoor(x, y, trapdoorOpen);
                    break;
                case "Lever":
                    ge = new Lever(x, y);
                    break;
                case "HighGuardFallPowerUp":
                    ge = new PowerUpPickable(x, y, PowerUpPickable.Type.HighFallGuard);
                    break;
            }

            string rewarderElement = element.GetAttribute("rewarder");
            int rewarder = 0;
            if (rewarderElement != null)
                rewarder = int.Parse(rewarderElement);
            if (ge != null && rewarder != 0)
            {
                // Entity is linked to another entity
                IContraption contraption = ge as IContraption;
                ContraptionRewardData contraptionRewardData = contraption.getContraptionRewardData();
                contraptionRewardData.rewarderId = rewarder;
                string targetElement = element.GetAttribute("target");
                if (targetElement != null)
                    contraptionRewardData.targetId = int.Parse(targetElement);
                    
                string targetXPos = element.GetAttribute("targetX");
                string targetYPos = element.GetAttribute("targetY");
                if (targetXPos != null && targetYPos != null)
                {
                    int targetX = int.Parse(targetXPos);
                    int targetY = int.Parse(targetYPos);
                    contraptionRewardData.targetPos = new Vector2(targetX, targetY);
                }

                string valueElement = element.GetAttribute("value");
                if (targetElement != null)
                    contraptionRewardData.value = int.Parse(valueElement);

                contraption.setContraptionRewardData(contraptionRewardData);

                // If they don't link to any entity then we won't bother
                linkedContraptions.Add(contraption);
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
