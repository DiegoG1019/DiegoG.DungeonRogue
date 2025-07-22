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
    public UIMenuScene UIMenuScene { get; init; }
    public GameScene GameScene { get; init; }
}
