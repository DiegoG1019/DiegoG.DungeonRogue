using DiegoG.DungeonRogue.GameComponents.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiegoG.DungeonRogue.GameComponents;

public class TeapotComponent(Game game) : Component3D(game)
{
    protected override void LoadContent()
    {
        base.LoadContent();
        Model = Game.Content.Load<Model>("Models/Testing/utah_teapot");
    }
}