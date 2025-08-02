namespace DiegoG.DungeonRogue.Data;

public enum Direction
{
    Up,
    Right,
    Left,
    Down
}

public enum BoundsCheckReaction
{
    Stop,
    Bounce,
    Reset,
    ResetAndChangeDirection,
    Slide
}

public enum MovedStatus
{
    NotMoved,
    StartedMoving,
    Moved,
    Stopping
}