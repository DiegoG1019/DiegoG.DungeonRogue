using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using DiegoG.DungeonRogue.World.Rendering;
using DiegoG.DungeonRogue.World.WorldGeneration;
using DiegoG.DungeonRogue.World.WorldGeneration.Generators;
using DiegoG.DungeonRogue.World.WorldGeneration.Generators.LayoutGenerators;
using DiegoG.DungeonRogue.World.WorldGeneration.Generators.TileGenerators;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace DiegoG.DungeonRogue.World;

public class DungeonInfo : IDebugExplorable
{
    public delegate void CurrentAreaChangedEventHandler(DungeonInfo dungeon, DungeonArea? newArea);

    public DungeonInfo(int? seed = null, ICollection<int>? mapdescription = null)
    {
        var _s = seed ?? RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
        Seed = _s;
        Random = new(_s);
        Map = new(this, mapdescription ?? [5, 5, 5, 5]);
        da_field = Map.GetOrGenerate(default);
    }
    
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

    private static readonly FrozenDictionary<TileId, Color> GlobalColorKey = new Dictionary<TileId, Color>()
    {
        { TileId.Empty, Color.Transparent },
        { TileId.Normal, Color.Red },
        { TileId.Entry, Color.Green },
        { TileId.Exit, Color.Blue }
    }.ToFrozenDictionary();
    
    public IDungeonLayoutGenerator GetLayoutGeneratorFor(DungeonFloorId id) => DrunkardsWalkLayoutGenerator.Default;

    public FrozenDictionary<TileId, Color> GetColorKeyFor(DungeonFloorId id, out Color roomTint)
    {
        roomTint = Color.Yellow;
        return GlobalColorKey;
    }

    public Texture2DAtlas GetAtlasFor(DungeonFloorId id)
    {
        var tex = DungeonGame.Instance.Content.Load<Texture2D>("Environment/tiles_sewers");
        return Texture2DAtlas.Create($"Atlas/Environment/tiles_sewers", tex, 16, 16);
    }

    public void RenderImGuiDebug()
    {
        Span<char> bf = stackalloc char[20];
        
        ImGui.LabelText("Dungeon Seed", Seed.ToStringSpan(bf));
        if (ImGui.CollapsingHeader("Dungeon Map"))
            Map.RenderImGuiDebug();
    }

    public IDungeonTileGenerator GetTileGeneratorFor(DungeonFloorId id) => new TestTileGenerator();

    public IDungeonRenderer GetRendererFor(DungeonFloorId id) => new PrerenderToTextureDungeonRenderer();
}