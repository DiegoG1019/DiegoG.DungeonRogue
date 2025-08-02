using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tweening;

namespace DiegoG.DungeonRogue.GameComponents;

public class TestComponent(Game game) : DrawableGameComponent(game)
{
    public Vector2 Position { get; set; }
    private Texture2D? tex;
    private Tweener? twink;

    protected override void LoadContent()
    {
        tex = Game.Content.Load<Texture2D>("Sprites/warrior");
        Debug.Assert(tex is not null);
        twink = new();
        twink.TweenTo(this, x => x.Position, new(20, 20), 20, 0);
    }

    public override void Update(GameTime gameTime)
    {
        Debug.Assert(twink is not null);
        base.Update(gameTime);
        twink.Update(gameTime.GetElapsedSeconds());
    }

    public override void Draw(GameTime gameTime)
    {
        Debug.Assert(tex is not null);
        DungeonGame.WorldSpriteBatch.Draw(tex, Position, Color.White);
    }
}