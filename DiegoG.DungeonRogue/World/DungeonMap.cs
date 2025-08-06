using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using GLV.Shared.Common.Text;
using ImGuiNET;

namespace DiegoG.DungeonRogue.World;

public sealed class DungeonMap : IDebugExplorable
{
    private readonly record struct InternalDungeonAreaInfo(
        DungeonArea? Area,
        int RoomSeed
    );
    
    private readonly ImmutableArray<InternalDungeonAreaInfo[]> array;
    public DungeonInfo DungeonInfo { get; }

    internal DungeonMap(DungeonInfo dungeon, ICollection<int> mapdescription)
    {
        DungeonInfo = dungeon;
        var x = new InternalDungeonAreaInfo[mapdescription.Count][];
        int totalac = 0;
        int i = 0;
        foreach (var floorAreaCount in mapdescription)
        {
            var areaArr = new InternalDungeonAreaInfo[floorAreaCount];
            x[i++] = areaArr;
            for (int aidx = 0; aidx < areaArr.Length; aidx++)
            {
                areaArr[aidx] = new(null, dungeon.Random.Next());
            }
            totalac += floorAreaCount;
        }

        array = [..x];
        FloorCount = array.Length;
        TotalAreaCount = totalac;
    }

    public Success<DungeonArea> this[byte areaIndex, byte floorIndex] => this[new(areaIndex, floorIndex)];

    public Success<DungeonArea> this[DungeonFloorId id]
    {
        get
        {
            var area = GetAreaInfo(id);
            return area.Area is null ? Success<DungeonArea>.Failure : new(area.Area);
        }
    }

    private ref InternalDungeonAreaInfo GetAreaInfo(DungeonFloorId id)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id.AreaIndex, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(id.FloorIndex, 0);
            
        if (id.FloorIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(id.FloorIndex), id.FloorIndex, $"Invalid floor index, there are only {array.Length} floors");
        var floor = array[id.FloorIndex];
            
        if (id.AreaIndex > floor.Length) throw new ArgumentOutOfRangeException(nameof(id.AreaIndex), id.AreaIndex, $"Invalid area index, there are only {floor.Length} areas in floor {id.FloorIndex}");

        return ref floor[id.AreaIndex];
    }

    public DungeonArea GetOrGenerate(byte areaIndex, byte floorIndex) => GetOrGenerate(new(areaIndex, floorIndex));

    public DungeonArea GetOrGenerate(DungeonFloorId id)
    {
        var areaInfo = GetAreaInfo(id);
        if (areaInfo.Area is not null)
            return areaInfo.Area;

        var newArea = new DungeonArea(DungeonInfo, id);
        GetAreaInfo(id) = areaInfo with { Area = newArea };
        newArea.BeginGeneration();
        return newArea;
    }

    public int GetSeedFor(DungeonFloorId id) => GetAreaInfo(id).RoomSeed;
    
    public int TotalAreaCount { get; }
    public int FloorCount { get; }

    public int AreaCount(byte floorIndex)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(floorIndex, 0);
            
        if (floorIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(floorIndex), floorIndex, $"Invalid floor index, there are only {array.Length} floors");
        return array[floorIndex].Length;
    }

    public void RenderImGuiDebug()
    {
        Span<char> strbf = stackalloc char[100];
        Span<char> bf = stackalloc char[20];
        var sb = new ValueStringBuilder(strbf);
        
        ImGui.LabelText("Total Area Count", TotalAreaCount.ToStringSpan(bf));
        ImGui.LabelText("Floor Count", FloorCount.ToStringSpan(bf));

        if (!ImGui.CollapsingHeader("Area Info")) return;
        
        for (int i = 0; i < array.Length; i++)
        {
            var finfo = array[i];
                
            sb.Clear();
            sb.Append("Floor ");
            sb.Append(i.ToStringSpan(bf));
            sb.Append(", ");
            sb.Append(finfo.Length.ToStringSpan(bf));
            sb.Append(" areas");

            if (!ImGui.TreeNode(sb.AsSpan())) continue;
                
            for (int aidx = 0; aidx < finfo.Length; aidx++)
            {
                var ainfo = finfo[aidx];
                sb.Clear();
                sb.Append("F[");
                sb.Append(i.ToStringSpan(bf));
                sb.Append("][");
                sb.Append(aidx.ToStringSpan(bf));
                if (ainfo.Area is null)
                {
                    sb.Append("] Gen: Not created");
                    ImGui.BulletText(sb.AsSpan());
                }
                else
                {
                    var na = ainfo.Area;
                    Debug.Assert(na is not null);
                    
                    sb.Append("] Gen: ");
                    sb.Append(Enum.GetName(na.GenTaskStatus));
                    if (ImGui.TreeNode(sb.AsSpan()))
                    {
                        na.RenderImGuiDebug();
                        
                        sb.Clear();
                        sb.Append("Renderer: ");
                        sb.Append(na.Renderer?.GetType().Name ?? "null");
                        if (na.Renderer is not IDebugExplorable deren)
                            ImGui.BulletText(sb.AsSpan());
                        else
                        {
                            if (ImGui.TreeNode(sb.AsSpan()))
                            {
                                deren.RenderImGuiDebug();
                                ImGui.TreePop();
                            }
                        }
                        
                        ImGui.TreePop();
                    }
                }
                
                sb.Clear();
                sb.Append("Load Area F[");
                sb.Append(i.ToStringSpan(bf));
                sb.Append("][");
                sb.Append(aidx.ToStringSpan(bf));
                sb.Append(']');
                ImGui.SameLine();
                if (ImGui.Button(sb.AsSpan()))
                    DungeonInfo.CurrentFloorId = new((byte)aidx, (byte)i);
                
            }
                    
            ImGui.TreePop();
        }
    }
}