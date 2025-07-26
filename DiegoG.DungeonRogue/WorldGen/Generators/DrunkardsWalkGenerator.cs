using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.WorldGen.Generators;

public class DrunkardsWalkGenerator : IDungeonGenerator
{
    public float TurnChance { get; init; } = .16f;

    public Task<DungeonAreaGenerationResults> GenerateArea(DungeonArea area)
    {
        int x = 0, y = 0;
        x = area.Random.Next((area.Area.XCells / 2) - (area.Area.XCells / 10), (area.Area.XCells / 2) + (area.Area.XCells / 10));
        y = area.Random.Next((area.Area.YCells / 2) - (area.Area.YCells / 10), (area.Area.YCells / 2) + (area.Area.YCells / 10));
        int ix = x, iy = y;
        int dir;
        ChangeDirection();
        
        area.TileData[x, y].Data = TileInfo.Create(TileId.Entry);

        var rooms = new List<DungeonRoom>();
        for (int i = 0; i < area.Area.TotalCells / 6; i++)
        {
            ref var tile = ref area.TileData[x, y].Data;
            if (tile.TileId is TileId.Empty)
            {
                tile = TileInfo.Create(TileId.Normal);
            }
            else if (tile is { TileId: TileId.Normal } && tile.TileFlags.HasFlag(TileFlags.RoomTile) is false)
            {
                var darw = area.Random.Next(3, 10);
                var darh = area.Random.Next(3, 10);
                darw = int.Min(darw, area.Area.XCells - x);
                darh = int.Min(darh, area.Area.YCells - y);
                
                var dar = new Rectangle(x, y, darw, darh);
                for (int darx = 0; darx < dar.Width; darx++)
                for (int dary = 0; dary < dar.Height; dary++)
                    area.TileData[darx + x, dary + y].Data = TileInfo.Create(TileId.Normal, TileFlags.RoomTile);
                
                rooms.Add(new (dar));
            }

            if (area.Random.NextSingle() < TurnChance)
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

        return Task.FromResult(new DungeonAreaGenerationResults(rooms));

        void ChangeDirection()
        {
            dir = area.Random.Next() % 4;
        }
    }

    public static DrunkardsWalkGenerator Default { get; } = new();
}