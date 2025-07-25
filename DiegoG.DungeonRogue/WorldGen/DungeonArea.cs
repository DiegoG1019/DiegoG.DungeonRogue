using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using GLV.Shared.Common.Text;
using ImGuiNET;
using MonoGame.Extended;

namespace DiegoG.DungeonRogue.WorldGen;

public class DungeonArea : IDebugExplorable, IProgress<float>
{
    public const int CellSize = 16;
    public const int MaximumAreaSize = 170;
    public const int MinimumAreaSize = 60;
    public const int MaximumLargeAreaSize = 350;
    public const int MinimumLargeAreaSize = 150;
    public static readonly int MaximumGridSizeInBytes = Unsafe.SizeOf<TileInfo>() * MaximumLargeAreaSize * MaximumLargeAreaSize;

    internal DungeonArea(DungeonInfo dungeon, DungeonFloorId id, Size? area = null)
    {
        DungeonInfo = dungeon;
        Id = id;
        Seed = DungeonInfo.Map.GetSeedFor(Id);
        Random = new Random(Seed);

        var _a = area ?? new(Random.Next(MinimumAreaSize, MaximumAreaSize), Random.Next(MinimumAreaSize, MaximumAreaSize));
        Area = new BoundedSquareGrid(new(CellSize, CellSize), _a.Width, _a.Height);
        TileData = new(Area);
    }

    public delegate void ActivityMessageChangedEventHandler(DungeonArea context, string? activityMessage);
    public delegate void ProgressChangedEventHandler(DungeonArea context, float newProgress, float delta);

    public DataGrid<TileInfo> TileData { get; }
    public BoundedSquareGrid Area { get; }
    public DungeonInfo DungeonInfo { get; }
    public DungeonFloorId Id { get; }
    public int Seed { get; }
    public Random Random { get; }
    
    private Task? dungeonAreaTask;

    public TaskStatus GenTaskStatus => dungeonAreaTask?.Status ?? TaskStatus.WaitingForActivation;
    public bool GenerationCompleted => dungeonAreaTask?.IsCompleted is true;

    public void WaitForGenerationCompletion()
    {
        dungeonAreaTask?.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public string? ActivityMessage
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ActivityMessageChanged?.Invoke(this, value);
        }
    }

    public event ActivityMessageChangedEventHandler? ActivityMessageChanged;
    public event ProgressChangedEventHandler? ProgressChanged;
    private event Action<DungeonArea>? FloorGenerated;
    
    public bool TrySubscribeToGeneratedEvent(Action<DungeonArea> handler)
    {
        if (dungeonAreaTask?.IsCompleted is true)
            return false;
        lock (Random)
        {
            if (dungeonAreaTask?.IsCompleted is true)
                return false;
            FloorGenerated += handler;
            return true;
        }
    }
    
    public float CurrentProgress { get; private set; }
    
    public virtual void RenderImGuiDebug()
    {
        ImGui.ProgressBar(CurrentProgress, new(-1, 0), "Terrain Generation Progress");
        RenderImGuiBounds();
        ImGui.LabelText("Activity", ActivityMessage);
        RenderImGuiSeed();
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
        ValueStringBuilder sb = new(buffer);
        
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

    public void ReportProgress(float currentProgress)
    {
        var p = CurrentProgress;
        CurrentProgress = currentProgress;
        ProgressChanged?.Invoke(this, currentProgress, currentProgress - p);
    }

    void IProgress<float>.Report(float value) => ReportProgress(value);

    public void BeginGeneration()
    {
        if (dungeonAreaTask is not null) return; 
        lock (Random)
        {
            if (dungeonAreaTask is not null) return;
            
            var gen = DungeonInfo.GetGeneratorFor(Id);
            var t = Task.Run(() =>
            {
                gen.GenerateArea(this);
                lock (Random)
                {
                    FloorGenerated?.Invoke(this);
                    FloorGenerated = null;
                }
            }); 
            this.dungeonAreaTask = t;
        }
    }
}