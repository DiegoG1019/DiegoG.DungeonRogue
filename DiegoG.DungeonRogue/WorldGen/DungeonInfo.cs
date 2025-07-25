using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using GLV.Shared.Common;
using GLV.Shared.Common.Text;
using ImGuiNET;

namespace DiegoG.DungeonRogue.WorldGen;

public class DungeonInfo : IDebugExplorable
{
    public delegate void CurrentAreaChangedEventHandler(DungeonInfo dungeon, DungeonArea? newArea);

    public DungeonInfo(int? seed = null)
    {
        var _s = seed ?? RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
        Seed = _s;
        Random = new(_s);
        Map = new(this, [5, 5, 5, 5]);
        da_field = Map.GetOrGenerate(default);
    }
    
    //TODO: Preallocate all the room seeds, so that generation is done regardless of player entry order.
    //TODO: I want there to be several routes a player can take, and to be able to mix between them. Maybe add dungeon size?
    //public ImmutableArray<int> RoomSeeds { get; }

    public int Seed { get; }
    public DungeonMap Map { get; }
    public Random Random { get; }

    private DungeonArea da_field;
    public DungeonArea CurrentArea => da_field;

    [MemberNotNull(nameof(da_field))]
    public DungeonFloorId CurrentFloorId
    {
        get;
        set
        {
            if (value == field)
            {
                Debug.Assert(da_field is not null);
                return;
            }

            field = value;
            da_field = Map.GetOrGenerate(value);
            CurrentAreaChanged?.Invoke(this, da_field);
        }
    }

    public event CurrentAreaChangedEventHandler? CurrentAreaChanged;

    public IDungeonGenerator GetGeneratorFor(DungeonFloorId id) => DrunkardsWalkGenerator.Instance;

    public void RenderImGuiDebug()
    {
        Span<char> bf = stackalloc char[20];
        
        ImGui.LabelText("Dungeon Seed", Seed.ToStringSpan(bf));
        if (ImGui.CollapsingHeader("Dungeon Map"))
            Map.RenderImGuiDebug();
    }
}