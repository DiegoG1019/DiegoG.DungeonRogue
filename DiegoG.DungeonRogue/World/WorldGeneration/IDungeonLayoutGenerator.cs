using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.World.WorldGeneration;

public interface IDungeonLayoutGenerator
{
    public Task GenerateLayout(DungeonAreaLayoutGenerationContext context);
}

public interface IDungeonTileGenerator
{
    public Task<DungeonAreaGenerationResults> GenerateTiles(DungeonAreaLayoutGenerationContext context, DungeonArea area);
}

public interface IDungeonRenderer : IDrawable
{
    public Task Initialize(DungeonArea area);
}