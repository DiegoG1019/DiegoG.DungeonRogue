using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.World;

public struct DungeonRoom(Rectangle area)
{
    public Rectangle Area { get; } = area;
}