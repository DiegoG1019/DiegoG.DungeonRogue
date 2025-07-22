using System;
using System.Diagnostics;

namespace DiegoG.DungeonRogue.Data;

public enum PlayerClass
{
    Warrior
}

public enum ArmorTier : int
{
    None = 0,
    Cloth,
    Leather,
    Chainmail,
    Scale,
    Plate,
    Heroic
}

public static class DataEnumExtensions
{
    public static string GetName(this PlayerCharacterAnim anim)
        => Enum.GetName(anim)!;
}