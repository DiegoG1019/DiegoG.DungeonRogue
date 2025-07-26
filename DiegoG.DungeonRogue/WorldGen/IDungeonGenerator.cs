using System;
using System.Collections;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DiegoG.MonoGame.Extended;

namespace DiegoG.DungeonRogue.WorldGen;

public interface IDungeonGenerator
{
    public Task<DungeonAreaGenerationResults> GenerateArea(DungeonArea area);
}