using System;
using System.Runtime.InteropServices;

namespace DiegoG.DungeonRogue.WorldGen;

public readonly record struct DungeonFloorId(byte AreaIndex, byte FloorIndex);

public enum AreaAttributes : int
{
    
}

public enum TiledId : byte
{
    Normal = 1,
    
    Invalid = 32
}

[Flags]
public enum TileFlags : byte
{
    A = 1,
    B = 1 << 1,
    C = 1 << 2
}

[StructLayout(LayoutKind.Explicit)]
public readonly record struct TileInfo(ushort CondensedInformation)
{
    [FieldOffset(0)]
    private readonly byte floordat;
    
    [field: FieldOffset(0)]
    public ushort CondensedInformation { get; } = CondensedInformation;

    public TiledId TileId => (TiledId)(floordat & 0xF8);
    public TileFlags TileFlags => (TileFlags)(floordat & 0x07); 
}
