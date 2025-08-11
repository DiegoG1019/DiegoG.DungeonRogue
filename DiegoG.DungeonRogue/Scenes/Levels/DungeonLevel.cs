using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using DiegoG.DungeonRogue.World;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using Serilog;

namespace DiegoG.DungeonRogue.Scenes.Levels;

public class DungeonLevel(GameScene gameScene) : LevelScene(gameScene), IDebugExplorable
{
    public DungeonInfo? CurrentDungeon
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            TryDebugGenAreas();
        }
    } 
    
    public Vector2 MiniMapPosition { get; set; } = new Vector2(16, 16);

    public float MiniMapSizePercentage { get; set; } = 1.5f;

    protected override void LoadContent()
    {
        CurrentDungeon = new(Game);
        base.LoadContent();
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        
        if (CurrentDungeon?.CurrentArea is not DungeonArea { GenerationCompleted: true } da) return;
        da.Renderer?.Draw(gameTime);
        
        /*
            foreach (var cell in da.TileData.GetCells())
                if (cell.Data.TileId == TileId.Normal)
                    DungeonGame.WorldSpriteBatch.Draw(atlas[0], cell.GetPosition(), Color.White);
                else if (cell.Data.TileId == TileId.Entry)
                    DungeonGame.WorldSpriteBatch.Draw(atlas[16], cell.GetPosition(), Color.White);
            */

        // TODO: this can be cached, and changed upon the area changed event firing
        var miniMapRect = new Rectangle(
            (int)MiniMapPosition.X,
            (int)MiniMapPosition.Y,
            (int)(da.AreaGraph.Bounds.Width * MiniMapSizePercentage),
            (int)(da.AreaGraph.Bounds.Height * MiniMapSizePercentage)  
        );    
        DungeonGame.HUDSpriteBatch.Draw(da.AreaGraph, miniMapRect, null, Color.White with { A = 255 });
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

    private void TryDebugGenAreas()
    {
        #if DEBUG

        if (DungeonGame.ParsedCommandLine.AutoGenTestLevels <= 0) return;
        var levelsToGen = DungeonGame.ParsedCommandLine.AutoGenTestLevels;

        List<int> mapdesc = [];
        {
            var levels = levelsToGen;
            while (levels > byte.MaxValue)
            {
                mapdesc.Add(byte.MaxValue);
                levels -= byte.MaxValue;
            }
            
            mapdesc.Add(levels);
        }

        var cd = new DungeonInfo(Game, mapdescription: mapdesc);
        Log.Warning("Attempting to generate {levelsToGen} areas on dungeon {dungeonHash} ({dungeonSeed}) levels for testing purposes", levelsToGen, cd.GetHashCode(), cd.Seed);
        
        for (int i = 0; i < levelsToGen; i++)
        {
            byte areaIndex = (byte)(i % (byte.MaxValue + 1));
            byte floorIndex = (byte)(i / byte.MaxValue);
            cd.Map.GetOrGenerate(new DungeonFloorId(areaIndex, floorIndex));
        }
        
        Log.Warning("Finished generating {levelsToGen} on dungeon {dungeonHash} ({dungeonSeed}) levels for testing purposes", levelsToGen, cd.GetHashCode(), cd.Seed);
        #endif
    }
}