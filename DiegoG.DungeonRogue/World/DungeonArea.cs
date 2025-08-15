using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using DiegoG.DungeonRogue.World.WorldGeneration;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using GLV.Shared.Common.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DiegoG.DungeonRogue.World;

public partial class DungeonArea : IDebugExplorable, IProgress<float>
{
    public const int CellSize = 16;
    public const int MaximumAreaSize = 170;
    public const int MinimumAreaSize = 60;
    public const int MaximumLargeAreaSize = 350;
    public const int MinimumLargeAreaSize = 150;
    public static readonly int MaximumGridSizeInBytes = Unsafe.SizeOf<TileInfo>() * MaximumLargeAreaSize * MaximumLargeAreaSize;

    internal DungeonArea(DungeonInfo dungeon, DungeonFloorId id, Size? area = null, AreaAttributes? attributes = null)
    {
        DungeonInfo = dungeon;
        Id = id;
        Seed = DungeonInfo.Map.GetSeedFor(Id);
        Random = new Random(Seed);

        var _a = area ?? new(Random.Next(MinimumAreaSize, MaximumAreaSize), Random.Next(MinimumAreaSize, MaximumAreaSize));
        Area = new BoundedSquareGrid(new(CellSize, CellSize), _a.Width, _a.Height);
        AreaAttributes = attributes ?? 0;
        TileData = new(Area);
        AreaGraph = new Texture2D(DungeonGame.Instance.GraphicsDevice, Area.XCells, Area.YCells);
        areaGraphPtr = DungeonGame.ImGuiRenderer.BindTexture(AreaGraph);
        AreaGraph.Disposing += (sender, args) => DungeonGame.ImGuiRenderer.UnbindTexture(areaGraphPtr);
    }

    public Texture2D AreaGraph { get; }
    private IntPtr areaGraphPtr;
    public AreaAttributes AreaAttributes { get; }
    public DataGrid<TileInfo> TileData { get; }
    public BoundedSquareGrid Area { get; }
    public DungeonInfo DungeonInfo { get; }
    public DungeonFloorId Id { get; }
    public int Seed { get; }
    public Random Random { get; }
    public IDungeonRenderer? Renderer { get; private set; }
    public ImmutableArray<DungeonRoom> Rooms { get; private set; }
    public ImmutableArray<DungeonRoom> Corridors { get; private set; }
    
    public virtual void RenderImGuiDebug()
    {
        ImGui.ProgressBar(CurrentProgress, new(-1, 0), "Terrain Generation Progress");
        RenderImGuiBounds();
        ImGui.LabelText("Activity", ActivityMessage ?? "Idle");
        ImGui.LabelText("Generation Stage", Enum.GetName(GenerationStage));
        ImGui.LabelText("Generation Completed", GenerationCompleted ? "Generated" : "Not yet generated");
        RenderImGuiSeed();
        if (GenerationCompleted)
            ImGui.Image(areaGraphPtr, new(AreaGraph.Width, AreaGraph.Height));
        else
            ImGui.Text("Not generated yet...");
    }

    private void RenderImGuiSeed()
    {
        Span<char> b = stackalloc char[20];
        ImGui.LabelText("Seed", Seed.ToStringSpan(b));
    }

    private void RenderImGuiBounds()
    {
        Span<char> buffer = stackalloc char[100];
        Span<char> smallbuffer = stackalloc char[20];
        var sb = new ValueStringBuilder(buffer);
        
        sb.Append("W: ");
        sb.Append(Area.Grid.XScale.ToStringSpan(smallbuffer));
        sb.Append("H: ");
        sb.Append(Area.Grid.YScale.ToStringSpan(smallbuffer));
        sb.Append("X cells: ");
        sb.Append(Area.XCells.ToStringSpan(smallbuffer));
        sb.Append("Y cells: ");
        sb.Append(Area.YCells.ToStringSpan(smallbuffer));
        sb.Append(" (");
        sb.Append((Area.XCells * Area.Grid.XScale).ToStringSpan(smallbuffer));
        sb.Append(" x ");
        sb.Append((Area.YCells * Area.Grid.YScale).ToStringSpan(smallbuffer));
        sb.Append(')');
        ImGui.LabelText("Terrain Bounds", sb.AsSpan());
    }
}