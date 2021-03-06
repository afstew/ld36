﻿using System;

namespace LD36Quill18
{
    public enum TileType { FLOOR, WALL, DOOR_CLOSED, DOOR_OPENED, UPSTAIR, DOWNSTAIR, DEBRIS, DOOR_LOCKED }

    public class Tile
    {
        const string TILE_GLYPHS = @" #+-<>x+";

        public Tile(int x, int y, Floor floor, char textChar)
        {
            this.X = x;
            this.Y = y;
            this.Floor = floor;
            Item item;

            TileType = TileType.FLOOR;

            switch(textChar)
            {
                case ' ':
                    break;
                case '#':
                    TileType = TileType.WALL;
                    break;
                case 'x':
                case 'X':
                    TileType = TileType.DEBRIS;
                    Chixel.ForegroundColor = ConsoleColor.Gray;
                    break;
                case '+':
                    TileType = TileType.DOOR_CLOSED;
                    break;
                case '-':
                    TileType = TileType.DOOR_OPENED;
                    break;
                case '<':
                    TileType = TileType.UPSTAIR;
                    Floor.Upstair = this;
                    break;
                case '>':
                    TileType = TileType.DOWNSTAIR;
                    Floor.Downstair = this;
                    break;
                case '*':
                    TileType = TileType.DOOR_LOCKED;
                    break;
                case '@':

                    // Spawn a character (only if one doesn't exist)
                    Game.Instance.DebugMessage("Character spawned!");

                    if (Game.Instance.PlayerCharacter != null)
                    {
                        throw new Exception("Already have a player character!");
                    }
                    Game.Instance.PlayerCharacter = new PlayerCharacter( this, this.Floor, new Chixel('@', ConsoleColor.DarkYellow) );
                    break;
                case '»':
                case '«':
                    item = new Item();
                    item.Name = "Conveyor Belt";
                    item.Description = "Part of an old manufacturing system. Non-functional.";
                    item.Chixel = new Chixel(textChar, ConsoleColor.Gray);
                    item.Static = true;
                    this.Item = item;
                    break;
                case '{':
                case '}':
                    item = new Item();
                    item.Name = "Fabricator Casing";
                    item.Description = "Stand on the '~' to interact.";
                    item.Chixel = new Chixel(textChar, ConsoleColor.Gray);
                    item.Static = true;
                    this.Item = item;
                    break;
                case '~':
                    item = new Item();
                    item.Name = "Upgrade Station";
                    item.Description = "Permanently repair/upgrade some of your systems!";
                    item.Chixel = new Chixel(textChar, ConsoleColor.Green);
                    item.Static = true;
                    item.IsFabricator = true;
                    this.Item = item;
                    break;
                default:
                    // Everything else is either a monster or item
                    // so do that??
                    TileType = TileType.FLOOR;

                    if (textChar >= '0' && textChar <= '9')
                    {
                        if (FloorMaps.ItemSpawner[Floor.FloorIndex, (int)textChar - '0'] == null)
                        {
                            throw new Exception(string.Format("No item spawner for FloorIndex={0} and ID={1}", floor.FloorIndex, textChar));
                        }
                        FloorMaps.ItemSpawner[floor.FloorIndex, (int)textChar - '0'](this);
                        return;
                    }
                    else if (MonsterList.Monsters.ContainsKey(textChar))
                    {
                        // Yup, it's a monster.
                        MonsterCharacter mc = new MonsterCharacter(MonsterList.Monsters[textChar]);
                        mc.Tile = this;

                        // Buff the monsters as we go down in level.
                        mc.Health = mc.MaxHealth += Floor.FloorIndex;
                        mc.RangedDamage += (Floor.FloorIndex - 1) / 2;
                        mc.MeleeDamage += Floor.FloorIndex / 2;
                        mc.DamageReduction += Floor.FloorIndex / 3;
                        mc.ToHitBonus += (Floor.FloorIndex) / 2;
                        mc.DodgeBonus += Floor.FloorIndex / 3;

                        return;
                    }
                    else if (ItemList.Items.ContainsKey(textChar))
                    {
                        // It's an item
                        this.Item = new Item(ItemList.Items[textChar]);
                        return;
                    }

                    //throw new Exception("No character entry for: " + textChar);
                    Game.Instance.DebugMessage("No character entry for: " + textChar);

                    item = new Item();
					item.Name = "Furniture";
					item.Description = "There is a word from some dead, long forgotten language engraved on it.";
					item.Chixel = new Chixel(textChar, ConsoleColor.Gray);
                    item.Static = true;
                    this.Item = item;


                    break;
            }

        }

        public TileType TileType { 
            get
            {
                return _TileType; 
            }
            set
            {
                _TileType = value;
                Chixel = new Chixel( TILE_GLYPHS[(int)_TileType] );
                if (_TileType == TileType.DOOR_LOCKED)
                {
                    Chixel.ForegroundColor = ConsoleColor.Red;
                }
            }
        }

        public string Description
        {
            get
            {
                switch (TileType)
                {
                    case TileType.FLOOR:
                        return "Empty floor.";
                    case TileType.WALL:
                        return "Sturdy steel wall.";
                    case TileType.DOOR_CLOSED:
                        return "A closed, but unlocked door.";
                    case TileType.DOOR_OPENED:
                        return "An open door.";
                    case TileType.UPSTAIR:
                        return "Stairs leading up.";
                    case TileType.DOWNSTAIR:
                        return "Stairs leading down.";
                    case TileType.DEBRIS:
                        return "Debris -- impassable, but you can shoot over it.";
                    case TileType.DOOR_LOCKED:
                        return "Locked door. Requires an access card.";
                }
                return "";
            }
        }

        public int X { get; protected set; }
        public int Y { get; protected set; }
        public Floor Floor { get; protected set; }
        public Chixel Chixel { get; protected set; }
        public Character Character { get; set; }
        public Item Item { get; set; }
        public bool WasSeen { get; set; }

        private TileType _TileType;

        public void Unlock()
        {
            if (TileType != TileType.DOOR_LOCKED)
            {
                return;
            }

            TileType = TileType.DOOR_CLOSED;
            Game.Instance.Message("You unlock the door.");
        }

        public bool IsWalkable()
        {
            return TileType != TileType.DOOR_LOCKED &&
                                       TileType != TileType.WALL &&
                                       TileType != TileType.DEBRIS;
            
        }

        public bool IsLookable()
        {
            return TileType != TileType.WALL && 
                                       TileType != TileType.DOOR_CLOSED && 
                                       TileType != TileType.DOOR_LOCKED;
        }


        public void Draw(int viewOffsetX, int viewOffsetY)
        {
            if (WasSeen == false)
            {
                FrameBuffer.Instance.SetChixel(X + viewOffsetX, Y + viewOffsetY, '\u2591');
                return;
            }

            if (Item != null)
            {
                // Draw item
                Item.Draw(X+viewOffsetX, Y+viewOffsetY);
                return;
            }

            FrameBuffer.Instance.SetChixel(X+viewOffsetX, Y+viewOffsetY, Chixel);
        }
    }
}

