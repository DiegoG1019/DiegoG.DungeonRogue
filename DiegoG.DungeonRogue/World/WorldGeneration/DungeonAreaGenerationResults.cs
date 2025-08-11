using System;
using System.Collections.Generic;
using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.World.WorldGeneration;

public sealed class DungeonAreaLayoutGenerationContext(DungeonArea area)
{
    private readonly List<Rectangle> roomsRegions = [];
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

    public bool IsPointInRoom(Point point)
    {
        for (int i = 0; i < roomsRegions.Count; i++)
            if (roomsRegions[i].Contains(point)) return true;
        return false;
    }

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

public record DungeonAreaGenerationResults(IEnumerable<DungeonRoom> Rooms)
{
    
}