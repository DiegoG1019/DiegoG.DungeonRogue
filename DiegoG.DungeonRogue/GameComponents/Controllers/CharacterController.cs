using DiegoG.DungeonRogue.GameComponents.Base;
using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.GameComponents.Controllers;

public abstract class CharacterController
{
    public abstract void UpdateCharacter(CharacterComponent character, GameTime gameTime);
}