using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.WorldGen;

public struct DungeonRoom(Rectangle area)
{
    public Rectangle Area { get; }
}