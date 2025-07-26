using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DiegoG.MonoGame.Extended;

namespace DiegoG.DungeonRogue.WorldGen;

public interface IDungeonGenerator
{
    public Task GenerateArea(DungeonArea area);
}