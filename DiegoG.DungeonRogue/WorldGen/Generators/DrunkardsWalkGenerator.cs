using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DiegoG.MonoGame.Extended;

namespace DiegoG.DungeonRogue.WorldGen.Generators;

public class DrunkardsWalkGenerator : IDungeonGenerator
{
    public float TurnChance { get; init; } = 1;

    public Task GenerateArea(DungeonArea area)
    {
        int x = 0, y = 0;
        x = area.Random.Next(area.Area.XCells / 4, area.Area.XCells / 2);
        y = area.Random.Next(area.Area.YCells / 4, area.Area.YCells / 2);
        int ix = x, iy = y;
        int dir;
        ChangeDirection();
        
        area.TileData[x, y].Data = TileInfo.Create(TileId.Entry);
        for (int i = 0; i < area.Area.TotalCells / 6; i++)
        {
            ref var tile = ref area.TileData[x, y].Data;
            if (tile.TileId is not (TileId.Entry or TileId.Exit))
            {
                tile = TileInfo.Create(TileId.Normal);
            }

            if (area.Random.NextSingle() > .7)//TurnChance)
                ChangeDirection();

            switch (dir)
            {
                case 0:
                    x++;
                    break;
                case 1:
                    x--;
                    break;
                case 2:
                    y++;
                    break;
                case 3:
                    y--;
                    break;
            }
            
            var mx = area.Area.CompareHorizontalBounds(x);
            if (mx != 0) { x = ix; ChangeDirection(); }

            mx = area.Area.CompareVerticalBounds(y);
            if (mx != 0) { y = iy; ChangeDirection(); }
        }

        return Task.CompletedTask;

        void ChangeDirection()
        {
            dir = area.Random.Next(0, 7) % 4;
        }
    }

    public static DrunkardsWalkGenerator Default { get; } = new();
}