using System;
using System.Buffers;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using DiegoG.DungeonRogue.World.WorldGeneration;
using GLV.Shared.Common;
using Serilog;

namespace DiegoG.DungeonRogue.World;

public partial class DungeonArea
{
    public enum AreaGenerationStage
    {
        NotStarted,
        Layout,
        Tiles,
        Finalizing,
        InitRenderer,
        Completed
    }
    
    public delegate void ActivityChangedEventHandler(DungeonArea context, string? activityMessage, AreaGenerationStage stage);
    public delegate void ProgressChangedEventHandler(DungeonArea context, float newProgress, float delta);

    public TimeSpan GenerationTime { get; private set; }
    
    private Task? dungeonAreaTask;

    public TaskStatus GenTaskStatus => dungeonAreaTask?.Status ?? (TaskStatus)(-1);
    public bool GenerationCompleted => dungeonAreaTask?.IsCompleted is true;

    public float CurrentProgress { get; private set; }

    public AreaGenerationStage GenerationStage
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ActivityChanged?.Invoke(this, ActivityMessage, value);
        }
    }

    public string? ActivityMessage
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            ActivityChanged?.Invoke(this, value, GenerationStage);
        }
    }

    public event ActivityChangedEventHandler? ActivityChanged;
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
    
    public void ReportProgress(float currentProgress)
    {
        var p = CurrentProgress;
        CurrentProgress = currentProgress;
        ProgressChanged?.Invoke(this, currentProgress, currentProgress - p);
    }

    void IProgress<float>.Report(float value) => ReportProgress(value);

    public void WaitForGenerationCompletion()
    {
        dungeonAreaTask?.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    protected virtual ValueTask FinalizeGeneration(DungeonAreaGenerationResults results)
    {
        var len = AreaGraph.Height * AreaGraph.Width;
        uint[] colorData = ArrayPool<uint>.Shared.Rent(len);
        try
        {
            int ci = 0;
            var ck = DungeonInfo.GetColorKeyFor(Id, out var roomTint);
            foreach (var cell in TileData.GetCells())
            {
                var co = ck[cell.Data.TileId];
                if (cell.Data.TileFlags.HasFlag(TileFlags.RoomTile))
                    co = roomTint;
                colorData[ci++] = co.PackedValue;
            }
            AreaGraph.SetData(colorData, 0, len);
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(colorData);
        }

        Rooms = results.Rooms.ToImmutableArray();
        Corridors = results.Corridors.ToImmutableArray();

        if (DungeonGame.ParsedCommandLine.DumpLevelGraphs)
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Directory.CreateDirectory(Path.Combine(appdata, "debug", "dump", "level-graphs", $"{DungeonInfo.GetHashCode()} ({DungeonInfo.Seed})")).FullName;
            var file = Path.Combine(dir, $"A{Id.AreaIndex}F{Id.FloorIndex} ({Seed}).png");
            
            Log.Warning("Dumping level graph of A:{area}:F{floor} ({seed}) in dungeon {dungeonHash} ({dungeonSeed}) to {dir}", Id.AreaIndex, Id.FloorIndex, Seed, DungeonInfo.GetHashCode(), DungeonInfo.Seed, dir);
            using var filestream = File.Open(file, FileMode.Create);
            AreaGraph.SaveAsPng(filestream, AreaGraph.Width, AreaGraph.Height);
        }
        
        return ValueTask.CompletedTask;
    }

    public void BeginGeneration()
    {
        if (dungeonAreaTask is not null) return; 
        lock (Random)
        {
            if (dungeonAreaTask is not null) return;
            
            var layoutGen = DungeonInfo.GetLayoutGeneratorFor(Id);
            var tileGen = DungeonInfo.GetTileGeneratorFor(Id);
            var renderer = DungeonInfo.GetRendererFor(Id);
            var t = Task.Run(async () =>
            {
                await Task.Yield();
                var st = DateTime.Now;
                GenerationStage = AreaGenerationStage.Layout;
                var layoutContext = new DungeonAreaLayoutGenerationContext(this);
                await layoutGen.GenerateLayout(layoutContext);
                
                GenerationStage = AreaGenerationStage.Tiles;
                var results = await tileGen.GenerateTiles(layoutContext, this);
                
                GenerationStage = AreaGenerationStage.Finalizing;
                await FinalizeGeneration(results);
                
                GenerationStage = AreaGenerationStage.InitRenderer;
                await renderer.Initialize(this);
                Renderer = renderer;
                GenerationTime = DateTime.Now - st;
                
                lock (Random)
                {
                    FloorGenerated?.Invoke(this);
                    FloorGenerated = null;
                }

                ActivityMessage = "";
                GenerationStage = AreaGenerationStage.Completed;
            });
            BackgroundTaskStore.Add(t);
            this.dungeonAreaTask = t;
        }
    }
}