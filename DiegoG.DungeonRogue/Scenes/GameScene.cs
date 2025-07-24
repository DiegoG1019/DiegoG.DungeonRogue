using System;
using DiegoG.DungeonRogue.Data;
using DiegoG.DungeonRogue.GameComponents;
using DiegoG.DungeonRogue.GameComponents.Controllers;
using DiegoG.DungeonRogue.Scenes.Levels;
using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace DiegoG.DungeonRogue.Scenes;

public class GameScene : Scene
{
    public GameScene(Game game) : base(game)
    {
        WorldCamera = new(GraphicsDevice);
        Player = new(Game, PlayerClass.Warrior)
        {
            Controller = new InputController(),
            UpdateOrder = int.MinValue
        };

        CurrentLevel = new TestLevel(this)
        {
            DrawOrder = int.MinValue
        };
    }

    public OrthographicCamera WorldCamera { get; }

    public LevelScene? CurrentLevel
    {
        get;
        set => ExchangeComponentProperty(value, ref field);
    }

    public PlayerCharacterComponent? Player
    {
        get;
        set => ExchangeComponentProperty(value, ref field);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (DungeonGame.KeyboardState.IsAltDown())
            WorldCamera.ZoomIn(DungeonGame.MouseState.GetMouseCameraZoomDelta());
    }

    public override void Draw(GameTime gameTime)
    {
        DungeonGame.WorldSpriteBatch.TransformationMatrix = WorldCamera.GetViewMatrix();
        base.Draw(gameTime);
    }
}