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
using AXE.Game.Entities.Axes;
using AXE.Game.Utils;
using AXE.Game.Entities.Bosses;
using AXE.Game.Entities.Decoration;

namespace AXE.Game.Entities
{
    class LevelMap : bEntity, IReloadable
    {
        int tileWidth = 8;
        int tileHeight = 8;

        public String mapName;

        public bTilemap tilemap;
        public List<bEntity> entities;

        public List<IContraption> linkedContraptions;

        public String name;
        public int timeLimit = 200;
        public Vector2 playerStart;
        public String bgMusicName;

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
                        if (parseStack.Count > 0 && 
                            (parseStack.Peek() == "Entities" || parseStack.Peek() == "Decoration"))
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
                                    int customTimeLimit = int.Parse(reader.GetAttribute("time"));
                                    if (customTimeLimit > 0) // Only override default timeLimit if parameter makes sense
                                        timeLimit = customTimeLimit;
                                    name = reader.GetAttribute("name");
                                    bgMusicName = reader.GetAttribute("bgmusic");
                                    if (bgMusicName == null)
                                        bgMusicName = "dungeon";
                                    break;
                                case "Tiles":
                                    tileset = reader.GetAttribute("tileset");
                                    break;
                                case "Solids":
                                    exportMode = reader.GetAttribute("exportMode");
                                    break;
                                case "Entities":
                                    break;
                                case "Decoration":
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

            // TODO: Unhardcde tset!
            tilemap = new bTilemap(w, h, tileWidth, tileHeight, (game as AxeGame).res.tsetBasic);
            tilemap.parseTiles(tiles);

            mask = new bSolidGrid(w / tileWidth, h / tileHeight, tileWidth, tileHeight);
            mask.game = game;
            (mask as bSolidGrid).parseSolids(solids);

            layer = 20;
        }

        /* IReloadable implementation */
        public void reloadContent()
        {
            // TODO: unhardcode this
            tilemap.tileset.texture = (game as AxeGame).res.tsetBasic;
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
                case "ExitDoor":
                    bool open = bool.Parse(element.GetAttribute("open"));
                    ge = new ExitDoor(x, y, open ? ExitDoor.Type.ExitOpen : ExitDoor.Type.ExitClose);
                    break;
                case "PlayerStart":
                    playerStart = new Vector2(x, y);
                    ge = new ExitDoor(x, y, ExitDoor.Type.Entry);
                    break;


                case "Ladder":
                    int width = int.Parse(element.GetAttribute("width"));
                    int height = int.Parse(element.GetAttribute("height"));
                    ge = new Stairs(x, y, width, height);
                    break;
                case "OneWayPlatform":
                    string attr = element.GetAttribute("width");
                    width = attr != null ? int.Parse(attr) : 16;
                    ge = new OneWayPlatform(x, y, width);
                    break;


                case "Imp":
                    ge = new Imp(x, y);
                    break;
                case "Undead":
                    ge = new Undead(x, y);
                    break;
                case "ZombieSpawner":
                    attr = element.GetAttribute("spawnableZombies");
                    int nzombies = attr != null ? int.Parse(attr) : 1;

                    ge = new ZombieSpawner(x, y, nzombies);
                    break;
                case "Zombie":
                    ge = new Zombie(x, y);
                    break;
                case "TerritorialRapier":
                    attr = element.GetAttribute("flipped");
                    bool flipped = attr != null ? bool.Parse(attr) : false;
                    ge = new TerritorialRapier(x, y, flipped);
                    break;
                case "Dagger":
                    ge = new Dagger(x, y);
                    break;
                case "DragonBoss":
                    ge = new DragonBoss(x, y);
                    break;
                case "EvilAxeHolder":
                    ge = new EvilAxeHolder(x, y);
                    break;
                case "FlameSpirit":
                    ge = new FlameSpirit(x, y);
                    break;
                case "CorrosiveSlime":
                    ge = new CorrosiveSlime(x, y);
                    break;
                case "Gargoyle":
                    attr = element.GetAttribute("flipped");
                    flipped = attr != null ? bool.Parse(attr) : false;

                    attr = element.GetAttribute("fireDelay");
                    int fireDelay = attr != null ? int.Parse(attr) : -1;

                    if (fireDelay > 0)
                        ge = new Gargoyle(x, y, flipped, fireDelay);
                    else
                        ge = new Gargoyle(x, y, flipped);
                    break;
                case "VGargoyle":
                    attr = element.GetAttribute("flipped");
                    flipped = attr != null ? bool.Parse(attr) : false;

                    attr = element.GetAttribute("fireDelay");
                    fireDelay = attr != null ? int.Parse(attr) : -1;

                    if (fireDelay > 0)
                        ge = new VGargoyle(x, y, flipped, fireDelay);
                    else
                        ge = new VGargoyle(x, y, flipped);
                    break;
                case "SinefloaterSpawner":
                    height = int.Parse(element.GetAttribute("height"));
                    int delay = int.Parse(element.GetAttribute("delay"));
                    AXE.Game.Entities.Entity.Dir direction = (element.GetAttribute("direction") == "Left" ?
                        AXE.Game.Entities.Entity.Dir.Left :
                        AXE.Game.Entities.Entity.Dir.Right);
                    int hspeed = int.Parse(element.GetAttribute("critterspeed"));
                    float delta = float.Parse(element.GetAttribute("angledelta"));
                    
                    ge = new SineFloaterSpawner(x, y, height, delay, direction, hspeed, delta);
                    break;
                case "Coin":
                    ge = new Coin(x, y);
                    break;
                case "TreasureChest":
                    string treasure = element.GetAttribute("treasure");
                    ge = new TreasureChest(x, y, treasure != null ? treasure : "coin");
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

                case "NormalAxe":
                    ge = new NormalAxe(x, y, null);
                    break;


                case "MoveablePlatform":
                    attr = element.GetAttribute("width");
                    width = attr != null ? int.Parse(attr) : 16;
                    int steps = int.Parse(element.GetAttribute("stepsBetweenNodes"));
                    bool cycle = (element.GetAttribute("cyclic") == "True");

                    List<Vector2> nodes = new List<Vector2>();
                    XmlReader nodeReader = element.ReadSubtree();
                    while (nodeReader.Read())
                    {
                        if (nodeReader.Name == "node")
                        {
                            int xx = int.Parse(nodeReader.GetAttribute("x"));
                            int yy = int.Parse(nodeReader.GetAttribute("y"));
                            Vector2 node = new Vector2(xx, yy);
                            nodes.Add(node);
                        }
                    }
                    
                    ge = new MoveablePlatform(x, y, width, nodes, steps, cycle);
                    break;
                case "Door":
                    type = element.GetAttribute("lock");
                    DoorLock.Type lockType = DoorLock.Type.None;
                    if (type == "None")
                    {
                        lockType = DoorLock.Type.None;
                        ge = new Door(x, y, lockType);
                    }
                    else if (type == "Key")
                    {
                        int key = int.Parse(element.GetAttribute("key"));
                        lockType = DoorLock.Type.Key;
                        ge = new KeyDoor(x, y, key);
                    }
                    else if (type == "Contraption")
                    {
                        lockType = DoorLock.Type.Contraption;
                        ge = new Door(x, y, lockType);
                    }
                    break;


                case "Key":
                    ge = new Key(x, y);
                    break;
                case "KeyA":
                    ge = new _Key(x, y, 1);
                    break;
                case "KeyB":
                    ge = new _Key(x, y, 2);
                    break;
                case "KeyC":
                    ge = new _Key(x, y, 3);
                    break;
                case "HighGuardFallPowerUp":
                    ge = new PowerUpPickable(x, y, PowerUpPickable.Type.HighFallGuard);
                    break;


                case "Torch":
                    ge = new DecoTorch(x, y);
                    break;
                case "Candle":
                    ge = new DecoCandle(x, y);
                    break;
                case "Candlestick":
                    ge = new DecoCandlestick(x, y);
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
