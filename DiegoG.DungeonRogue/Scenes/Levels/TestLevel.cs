using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace DiegoG.DungeonRogue.Scenes.Levels;

public class TestLevel(GameScene gameScene) : LevelScene(gameScene)
{
    public Size Area { get; protected set; }

    private Texture2DAtlas? atlas;
    private BoundedSquareGrid MapGrid = new(new SquareGrid(16, 16), 100, 100);

    protected override void LoadContent()
    {
        base.LoadContent();
        
        var tex = Game.Content.Load<Texture2D>("Environment/tiles_sewers");
        atlas = Texture2DAtlas.Create($"Atlas/Environment/tiles_sewers", tex, 16, 16);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        if (atlas is null) return;
        
        foreach(var cell in MapGrid.GetCells())
            DungeonGame.WorldSpriteBatch.Draw(atlas[0], cell, Color.White);
    }
}