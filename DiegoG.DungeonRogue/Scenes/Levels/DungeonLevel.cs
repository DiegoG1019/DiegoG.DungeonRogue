using System;
using System.Diagnostics;
using DiegoG.DungeonRogue.WorldGen;
using GLV.Shared.Common.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace DiegoG.DungeonRogue.Scenes.Levels;

public class DungeonLevel(GameScene gameScene) : LevelScene(gameScene), IDebugExplorable
{
    public DungeonInfo? CurrentDungeon { get; set; } = new();
    private Texture2DAtlas? atlas;

    protected override void LoadContent()
    {
        base.LoadContent();
        
        var tex = Game.Content.Load<Texture2D>("Environment/tiles_sewers");
        atlas = Texture2DAtlas.Create($"Atlas/Environment/tiles_sewers", tex, 16, 16);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        Debug.Assert(atlas is not null);
        if (CurrentDungeon?.CurrentArea is DungeonArea { GenerationCompleted: true } da)
        {
            foreach (var cell in da.TileData.GetCells())
                if (cell.Data.TileId == TileId.Normal)
                    DungeonGame.WorldSpriteBatch.Draw(atlas[0], cell.GetPosition(), Color.White);
                else if (cell.Data.TileId == TileId.Entry)
                    DungeonGame.WorldSpriteBatch.Draw(atlas[13], cell.GetPosition(), Color.White);
        }
    }

    public void RenderImGuiDebug()
    {
        var d = CurrentDungeon;
        if (d is null) 
            ImGui.BulletText("No dungeon loaded");
        else if (ImGui.TreeNode("Dungeon Info"))
        {
            d.RenderImGuiDebug();
            ImGui.TreePop();
        }
    }
}