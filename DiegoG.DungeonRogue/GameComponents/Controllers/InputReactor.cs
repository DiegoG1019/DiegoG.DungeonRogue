using System;
using System.Diagnostics;
using DiegoG.DungeonRogue.GameComponents.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace DiegoG.DungeonRogue.GameComponents.Controllers;

public class InputController : CharacterController
{
    public override void UpdateCharacter(CharacterComponent character, GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(character);
        Debug.Assert(gameTime is not null);
        
        // TODO: Make it more configurable, and add different keybindings and controller support

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
        
        character.Move(accel.NormalizedCopy());
    }
}