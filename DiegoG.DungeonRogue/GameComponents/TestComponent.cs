using System.Diagnostics;
using DiegoG.DungeonRogue.Services;
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
    private Model? teapot;
    private Effect? shader;
    private GameState? State;

    protected override void LoadContent()
    {
        tex = Game.Content.Load<Texture2D>("Sprites/warrior");
        Debug.Assert(tex is not null);
        twink = new();
        twink.TweenTo(this, x => x.Position, new(20, 20), 20, 0);
        teapot = Game.Content.Load<Model>("Models/Testing/utah_teapot");
        State = Game.Services.GetService<GameState>();
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
        Debug.Assert(teapot is not null);
        //Debug.Assert(shader is not null);
        Debug.Assert(State is not null);

        var view = State.Local.GameScene.WorldCamera3D.View;
        var proj = State.Local.GameScene.WorldCamera3D.Projection;
        var world = State.Local.GameScene.WorldCamera3D.World;
        
        // /* From https://gamefromscratch.com/monogame-tutorial-beginning-3d-programming/
        //Setup Camera
        var camTarget = new Vector3(0f, 0f, 0f);
        var camPosition = new Vector3(53f, 15, -53);
        //world = Matrix.CreateWorld(camTarget, Vector3.
            //Forward, Vector3.Up);
        
        // */
        
        //DungeonGame.WorldSpriteBatch.Draw(tex, Position, Color.White);
        foreach (var mesh in teapot.Meshes)
        {
            foreach (var fx in mesh.Effects)
            {
                Debug.Assert(fx is BasicEffect);
                if (fx is BasicEffect bfx)
                {
                    bfx.EnableDefaultLighting();
                    bfx.AmbientLightColor = new Vector3(1, 0.5f, 1);
                    bfx.View = view;
                    bfx.World = world;
                    bfx.Projection = proj;
                }
            }

            mesh.Draw();
        }
    }
}