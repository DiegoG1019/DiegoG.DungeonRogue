using System;
using System.Collections.ObjectModel;
using DiegoG.DungeonRogue.Data;
using DiegoG.DungeonRogue.GameComponents;
using DiegoG.DungeonRogue.GameComponents.Base;
using DiegoG.DungeonRogue.GameComponents.Controllers;
using DiegoG.DungeonRogue.Scenes.Levels;
using DiegoG.DungeonRogue.Services;
using DiegoG.MonoGame.Extended;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        WorldCamera3D = new(game);

        //SceneComponents.Add(new TestComponent(game));
        SceneComponents.Add(new TeapotComponent(game));
        SceneComponents.Add(WorldCamera3D);
    }

    public Vector3 CameraTarget
    {
        get => WorldCamera3D.TargetPosition;
        set => WorldCamera3D.TargetPosition = value;
    }

    public PerspectiveProjectionCamera WorldCamera3D { get; }

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

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Player is not null)
        {
            var pos = (Player.Position - WorldCamera3D.Center2D);
            WorldCamera3D.MoveTo(new(pos.X, 12, pos.Y));
        }
        
        Vector2 accel = default;
        
        if (DungeonGame.KeyboardState.IsKeyDown(Keys.Up))
            accel += new Vector2(0, -1);
        
        if (DungeonGame.KeyboardState.IsKeyDown(Keys.Left))
            accel += new Vector2(-1, 0);
        
        if (DungeonGame.KeyboardState.IsKeyDown(Keys.Down))
            accel += new Vector2(0, 1);
        
        if (DungeonGame.KeyboardState.IsKeyDown(Keys.Right))
            accel += new Vector2(1, 0);

        if (DungeonGame.KeyboardState.IsAltDown())
            WorldCamera3D.Move(new(default, DungeonGame.MouseState.GetMouseCameraZoomDelta()));

        //WorldCamera3D.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        DungeonGame.WorldSpriteBatch.TransformationMatrix = WorldCamera3D.World2D;
        base.Draw(gameTime);
    }

    public void RenderImGuiDebug()
    {
        if (Player is null) 
            ImGui.Text("No Player currently loaded");
        
        if (CurrentLevel is null) 
            ImGui.Text("No level is currently loaded");

        //if (ImGui.CollapsingHeader("Camera Info")) 
            //WorldCamera3D.RenderImGuiDebug();
    }
}