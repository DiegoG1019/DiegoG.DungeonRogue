using System.Linq;
using System.Threading.Tasks;

namespace DiegoG.DungeonRogue.World.WorldGeneration.Generators.TileGenerators;

public class TestTileGenerator : IDungeonTileGenerator
{
    public Task<DungeonAreaGenerationResults> GenerateTiles(DungeonAreaLayoutGenerationContext context, DungeonArea area)
    {
        foreach (var point in context.GetLayoutPoints())
            area.TileData[point].Data = TileInfo.Create(TileId.Normal, context.IsPointInRoom(point) ? TileFlags.RoomTile : TileFlags.None);

        area.TileData[context.PreviousFloorEntry].Data = TileInfo.Create(TileId.Entry);
        area.TileData[context.NextFloorExit].Data = TileInfo.Create(TileId.Exit);
        
        return Task.FromResult(new DungeonAreaGenerationResults(context.GetRooms().Select(x => new DungeonRoom(x))));
    }
}
