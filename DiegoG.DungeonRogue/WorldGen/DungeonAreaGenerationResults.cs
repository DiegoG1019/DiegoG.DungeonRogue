using System.Collections.Generic;

namespace DiegoG.DungeonRogue.WorldGen;

public record DungeonAreaGenerationResults(IEnumerable<DungeonRoom> Rooms);