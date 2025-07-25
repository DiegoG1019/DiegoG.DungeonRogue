using System;
using System.Collections.ObjectModel;
using DiegoG.DungeonRogue.Data;
using DiegoG.DungeonRogue.GameComponents;
using DiegoG.DungeonRogue.GameComponents.Controllers;
using DiegoG.DungeonRogue.Scenes.Levels;
using DiegoG.DungeonRogue.UIComponents;
using DiegoG.MonoGame.Extended;
using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tweening;

namespace DiegoG.DungeonRogue.Scenes;

public class GameScene : Scene, IDebugExplorable
{
    public GameScene(Game game) : base(game)
    {
        Player = new(Game, PlayerClass.Warrior)
        {
            Controller = new InputController(),
            UpdateOrder = int.MinValue
        };

        CurrentLevel = new DungeonLevel(this)
        {
            DrawOrder = int.MinValue
        };

        WorldCamera = new(GraphicsDevice)
        {
            Zoom = 1,
            MinimumZoom = .25f,
            MaximumZoom = 8
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

        if (Player is not null)
            WorldCamera.Position += ((Player.Position - WorldCamera.Center) - WorldCamera.Position) * gameTime.GetElapsedSeconds();
    }

    public override void Draw(GameTime gameTime)
    {
        DungeonGame.WorldSpriteBatch.TransformationMatrix = WorldCamera.GetViewMatrix();
        base.Draw(gameTime);
    }

    public void RenderImGuiDebug()
    {
        if (Player is null) 
            ImGui.Text("No Player currently loaded");
        
        if (CurrentLevel is null) 
            ImGui.Text("No level is currently loaded");

        if (ImGui.CollapsingHeader("Camera Info")) 
            WorldCamera.RenderImGuiDebug();
    }
}