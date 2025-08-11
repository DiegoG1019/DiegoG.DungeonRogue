using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DiegoG.DungeonRogue.Data;
using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.World.WorldGeneration.Generators.LayoutGenerators;

public class DrunkardsWalkLayoutGenerator : IDungeonLayoutGenerator
{
    public float TurnChance { get; set; } = .16f;
    public int CellAmountDivider { get; set; } = 10;
    public BoundsCheckReaction BoundsCheckReaction { get; set; } = BoundsCheckReaction.ResetAndChangeDirection; 

    public Task GenerateLayout(DungeonAreaLayoutGenerationContext context)
    {
        var turnChance = TurnChance;
        var cellAmountDivider = CellAmountDivider;
        var boundsCheckReaction = BoundsCheckReaction; 
        
        DirectionMovementState movementState = new(
            context.Random.Next((context.Area.XCells / 2) - (context.Area.XCells / 10), (context.Area.XCells / 2) + (context.Area.XCells / 10)),
            context.Random.Next((context.Area.YCells / 2) - (context.Area.YCells / 10), (context.Area.YCells / 2) + (context.Area.YCells / 10))
        );
        
        movementState.RandomizeDirection();

        context.PreviousFloorEntry = movementState.Position;
        var cellsToTry = context.Area.TotalCells / cellAmountDivider;
        double exitSetChance = 0;
        double exitSetChanceIncrement = 100.0 / cellsToTry + 10;

        context.ActivityMessage = "Walking a path to generate a map";
        for (int i = 0; i < cellsToTry; i++)
        {
            if (context.Random.NextSingle() < turnChance)
                movementState.RandomizeDirection();
            
            movementState.MoveWithinBounds(context.Area.TotalCellsRectangle, boundsCheckReaction);

            if (context[movementState.Position] is false)
            {
                if (exitSetChance >= 0 &&
                    context.Random.NextSingle() < (exitSetChance += exitSetChanceIncrement))
                {
                    context.NextFloorExit = movementState.Position;
                    exitSetChance = -1;
                }

                context[movementState.Position] = true;
            }
                
            else if (context.IsPointInRoom(movementState.Position) is false)
            {
                var darw = context.Random.Next(3, 10);
                var darh = context.Random.Next(3, 10);
                darw = int.Min(darw, context.Area.XCells - movementState.X);
                darh = int.Min(darh, context.Area.YCells - movementState.Y);
                
                var dar = new Rectangle(movementState.X, movementState.Y, darw, darh);
                
                context.AddRoom(dar);
            }
        }

        return Task.CompletedTask;
    }

    public static DrunkardsWalkLayoutGenerator Default { get; } = new();
}