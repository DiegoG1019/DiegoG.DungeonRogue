using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;

namespace DiegoG.DungeonRogue.Components;

public class InputReactor : GameComponent
{
    public IPositionable? Target { get; set; } 
    
    public InputReactor(Game game) : base(game)
    {
        UpdateOrder = int.MaxValue; // By default, it should update last
    }

    public override void Update(GameTime gameTime)
    {
        // TODO: Make it more configurable, and add different keybindings and controller support

        if (Target is not IPositionable positionable) return;

        Vector2 accel = default;
        
        if (DungeonGame.KeyboardState.IsKeyDown(Keys.W))
            accel += new Vector2(0, -1);
        
        if (DungeonGame.KeyboardState.IsKeyDown(Keys.A))
            accel += new Vector2(-1, 0);
        
        if (DungeonGame.KeyboardState.IsKeyDown(Keys.S))
            accel += new Vector2(0, 1);
        
        if (DungeonGame.KeyboardState.IsKeyDown(Keys.D))
            accel += new Vector2(1, 0);

        if (accel == Vector2.Zero) return;
        
        if (positionable is IMovable movable)
            movable.Move(accel);
        else
            positionable.Position += accel;
    }
}