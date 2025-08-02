using DiegoG.DungeonRogue.Scenes;

namespace DiegoG.DungeonRogue.Services;

public class GameState(
    LocalGameState localState
)
{
    public LocalGameState Local { get; } = localState;
}

public class LocalGameState
{
    public required UIMenuScene UIMenuScene { get; init; }
    public required GameScene GameScene { get; init; }
}
