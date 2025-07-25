using System;
using DiegoG.DungeonRogue.WorldGen;
using GLV.Shared.Common.Text;
using ImGuiNET;

namespace DiegoG.DungeonRogue.Scenes.Levels;

public class DungeonLevel(GameScene gameScene) : LevelScene(gameScene), IDebugExplorable
{
    public DungeonInfo? CurrentDungeon { get; set; } = new();

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