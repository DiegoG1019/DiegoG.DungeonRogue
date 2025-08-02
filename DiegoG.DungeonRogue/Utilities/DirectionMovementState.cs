using System;
using DiegoG.DungeonRogue.Data;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace DiegoG.DungeonRogue.Utilities;

public struct DirectionMovementState(int x, int y)
{
    public int InitialX { get; set; } = x;
    public int InitialY { get; set; } = y;
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public Point Position => new(X, Y);
    public Direction Direction { get; set; }

    public void Move()
    {
        switch (Direction)
        {
            case Direction.Right:
                X++;
                break;
            case Direction.Left:
                X--;
                break;
            case Direction.Down:
                Y++;
                break;
            case Direction.Up:
                Y--;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void MoveWithinBounds(RectangleF rectangle, BoundsCheckReaction boundsCheck = BoundsCheckReaction.Stop)
        => MoveWithinBounds(new((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height), boundsCheck);

    public void MoveWithinBounds(Rectangle rectangle, BoundsCheckReaction boundsCheck = BoundsCheckReaction.Stop)
    {
        if (boundsCheck is BoundsCheckReaction.Stop)
        {
            if (Direction is Direction.Left && X >= rectangle.Left 
                || Direction is Direction.Right && X < rectangle.Right
                || Direction is Direction.Up && Y >= rectangle.Top
                || Direction is Direction.Down && Y < rectangle.Bottom)
                Move();
        }

        if (Direction is Direction.Left or Direction.Right && X == rectangle.Left || X + 1 >= rectangle.Right)
        {
            if (X >= rectangle.Right) X = rectangle.Right - 1;
            else if (X < 0) X = 0;
            
            switch (boundsCheck)
            {
                case BoundsCheckReaction.Bounce:
                    Direction = Direction switch
                    {
                        Direction.Right => Direction.Left,
                        Direction.Left => Direction.Right,
                        _ => throw new InvalidOperationException()
                    };
                    break;
                case BoundsCheckReaction.Slide:
                    Direction = Y - rectangle.Top > rectangle.Bottom - Y ? Direction.Up : Direction.Down;
                    break;
                case BoundsCheckReaction.Reset or BoundsCheckReaction.ResetAndChangeDirection:
                {
                    X = InitialX;
                    Y = InitialY;
                    if (boundsCheck is BoundsCheckReaction.ResetAndChangeDirection) 
                        RandomizeDirection();
                    break;
                }
            }
        }
        else if (Direction is Direction.Up && Y <= rectangle.Top || Direction is Direction.Down && Y + 1 >= rectangle.Bottom)
        {
            if (Y >= rectangle.Bottom) Y = rectangle.Bottom - 1;
            else if (Y < 0) Y = 0;
            
            switch (boundsCheck)
            {
                case BoundsCheckReaction.Bounce:
                    Direction = Direction switch
                    {
                        Direction.Up => Direction.Down,
                        Direction.Down => Direction.Up,
                        _ => throw new InvalidOperationException()
                    };
                    break;
                case BoundsCheckReaction.Slide:
                    Direction = X - rectangle.Left > rectangle.Right - X ? Direction.Left : Direction.Right;
                    break;
                case BoundsCheckReaction.Reset or BoundsCheckReaction.ResetAndChangeDirection:
                {
                    X = InitialX;
                    Y = InitialY;
                    if (boundsCheck is BoundsCheckReaction.ResetAndChangeDirection) 
                        RandomizeDirection();
                    break;
                }
            }
        }
            //TODO: This looks ugly as hell. Revisit, please
        Move();
    }

    public void RandomizeDirection(Random? random = null)
    {
        Direction = (Direction)((random ?? Random.Shared).Next() % 4);
    }
}