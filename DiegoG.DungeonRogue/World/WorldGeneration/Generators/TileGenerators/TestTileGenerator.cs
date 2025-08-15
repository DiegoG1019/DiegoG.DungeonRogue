using System.Linq;
using System.Threading.Tasks;


namespace DiegoG.DungeonRogue.World.WorldGeneration.Generators.TileGenerators;

public class TestTileGenerator : IDungeonTileGenerator
{
    public Task<DungeonAreaGenerationResults> GenerateTiles(DungeonAreaLayoutGenerationContext context, DungeonArea area)
    {
        area.ActivityMessage = "Turning layout points directly into tiles";
        foreach (var point in context.GetLayoutPoints())
        {
            area.TileData[point].Data = TileInfo.Create(TileId.Normal, context.IsPointInRoom(point) ? TileFlags.RoomTile : TileFlags.None);
        }

        area.ActivityMessage = "Placing entry and exit tiles";
        area.TileData[context.PreviousFloorEntry].Data = TileInfo.Create(TileId.Entry);
        area.TileData[context.NextFloorExit].Data = TileInfo.Create(TileId.Exit);

        area.ActivityMessage = "Creating rooms from layout";

        var rooms = context.GetRooms().Select(x => new DungeonRoom(x));
        var corri = context.GetCorridors().Select(x => new DungeonRoom(x));
        return Task.FromResult(new DungeonAreaGenerationResults(rooms, corri));
    }
}
