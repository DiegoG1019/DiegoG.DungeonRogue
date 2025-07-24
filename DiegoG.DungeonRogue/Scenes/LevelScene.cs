using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.Scenes;

public abstract class LevelScene(GameScene gameScene) : Scene(gameScene.Game)
{
    public GameScene GameScene { get; } = gameScene;
}