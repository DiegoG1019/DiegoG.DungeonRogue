using System;
using System.Collections.Generic;
using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.World.WorldGeneration;

public sealed class DungeonAreaLayoutGenerationContext(DungeonArea area)
{
    private readonly HashSet<Rectangle> roomsRegions = [];
    private readonly HashSet<Rectangle> corridorRegions = [];
    private readonly HashSet<Point> pointsSet = [];

    public string? ActivityMessage
    {
        get => area.ActivityMessage;
        set => area.ActivityMessage = value;
    }
    
    public BoundedSquareGrid Area { get; } = area.Area;
    public Random Random { get; } = area.Random;

    public bool this[int x, int y]
    {
        get => this[new(x, y)];
        set => this[new(x, y)] = value;
    }

    public bool this[Point point]
    {
        get => pointsSet.Contains(point);
        set
        {
            if (value)
                pointsSet.Add(point);
            else
                pointsSet.Remove(point);
        }
    }

    public IEnumerable<Point> GetLayoutPoints()
        => pointsSet;

    public bool IsPointInCorridor(Point point, out Rectangle rect)
    {
        foreach (var test in corridorRegions)
        {
            if (test.Contains(point))
            {
                rect = test;
                return true;
            }
        }

        rect = default;
        return false;
    }

    public bool IsPointInCorridor(Point point)
    {
        foreach (var test in corridorRegions)
            if (test.Contains(point)) return true;
        return false;
    }

    public bool IsPointInRoom(Point point, out Rectangle rect)
    {
        foreach (var test in roomsRegions)
        {
            if (test.Contains(point))
            {
                rect = test;
                return true;
            }
        }

        rect = default;
        return false;
    }

    public bool IsPointInRoom(Point point)
    {
        foreach (var test in roomsRegions)
            if (test.Contains(point)) return true;
        return false;
    }

    public void AddCorridor(Rectangle corridorArea)
    {   
        for (int darx = 0; darx < corridorArea.Width; darx++)
        for (int dary = 0; dary < corridorArea.Height; dary++)
            this[darx + corridorArea.X, dary + corridorArea.Y] = true;
        corridorRegions.Add(corridorArea);
    }

    public IEnumerable<Rectangle> GetCorridors() => corridorRegions;

    public void AddRoom(Rectangle roomArea)
    {   
        for (int darx = 0; darx < roomArea.Width; darx++)
        for (int dary = 0; dary < roomArea.Height; dary++)
            this[darx + roomArea.X, dary + roomArea.Y] = true;
        roomsRegions.Add(roomArea);
    }

    public IEnumerable<Rectangle> GetRooms() => roomsRegions;
    
    public Point PreviousFloorEntry { get; set; }
    public Point NextFloorExit { get; set; }
    
    //TODO: Add a list of doors for the other floors. Map out area positions in a previous phase that checks out all floors
}

public record DungeonAreaGenerationResults(IEnumerable<DungeonRoom> Rooms, IEnumerable<DungeonRoom> Corridors)
{
    
}