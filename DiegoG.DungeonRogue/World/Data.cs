using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DiegoG.DungeonRogue.World;

public readonly record struct DungeonFloorId(byte AreaIndex, byte FloorIndex);

public enum AreaAttributes : int
{
    
}

public enum TileId : byte
{
    Empty = 0,
    Normal = 1,
    Entry = 2,
    Exit = 3,
    
    Invalid = 32
}

[Flags]
public enum TileFlags : byte
{
    None = 0,
    RoomTile = 1,
    B = 1 << 1,
    C = 1 << 2,
    
    Invalid = 8
}

[StructLayout(LayoutKind.Explicit)]
public readonly record struct TileInfo(ushort CondensedInformation)
{
    [FieldOffset(0)]
    private readonly byte floordat;
    
    [field: FieldOffset(0)]
    public ushort CondensedInformation { get; } = CondensedInformation;

    public TileId TileId => (TileId)((floordat & 0xF8) >> 3);
    public TileFlags TileFlags => (TileFlags)(floordat & 0x07);

    public TileInfo CopyWith(TileId tileId)
        => Create(tileId, TileFlags);

    public TileInfo CopyWith(TileFlags flags)
        => Create(TileId, flags);

    public static TileInfo Create(TileId tileId, TileFlags flags = TileFlags.None)
    {
        Debug.Assert(tileId < TileId.Invalid);
        Debug.Assert(flags < TileFlags.Invalid);

        ushort info = (ushort)((int)tileId << 3);
        info |= (ushort)((int)flags);
        
        var ti = new TileInfo(info);
        return ti;
    }
}
