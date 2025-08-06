using System;
using System.Text;
using DiegoG.DungeonRogue.Services;
using DiegoG.MonoGame.Extended;
using DiegoG.MonoGame.Extended.UIComponents;
using GLV.Shared.Common;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.Scenes;

public class UIMenuScene : Scene
{
    public UIMenuScene(Game game) : base(game)
    {
        
    }

    public override void Initialize()
    {
        base.Initialize();
        SceneComponents.Add(new DebugImGuiViews(Game));
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }
}