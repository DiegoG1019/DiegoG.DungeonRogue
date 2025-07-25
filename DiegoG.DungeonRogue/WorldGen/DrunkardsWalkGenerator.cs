using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiegoG.DungeonRogue.WorldGen;

public class DrunkardsWalkGenerator : IDungeonGenerator
{
    private DrunkardsWalkGenerator() { }
    
    public Task<DungeonArea> GenerateArea(DungeonArea context)
    {
        throw new System.NotImplementedException();
        //TODO No rendering has been prepared
    }

    public static DrunkardsWalkGenerator Instance { get; } = new();
}