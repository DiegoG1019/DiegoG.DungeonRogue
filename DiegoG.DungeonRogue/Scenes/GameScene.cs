using DiegoG.DungeonRogue.Components;
using DiegoG.DungeonRogue.Data;
using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace DiegoG.DungeonRogue.Scenes;

public class GameScene : Scene
{
    public GameScene(Game game) : base(game)
    {
        inputReactor = new(game);
        WorldCamera = new(GraphicsDevice);
        SceneComponents.Add(inputReactor);
    }

    public OrthographicCamera WorldCamera { get; }
    
    public PlayerCharacterComponent? Player
    {
        get;
        set
        {
            if (value == field) return;
            
            if (field is not null) SceneComponents.Remove(field);
            SceneComponents.Add(value);
            
            inputReactor.Target = value;
            field = value;
        }
    }

    private readonly InputReactor inputReactor;

    public override void Initialize()
    {
        base.Initialize();
        Player = new(Game, PlayerClass.Warrior);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        WorldCamera.ZoomIn(DungeonGame.MouseState.GetMouseCameraZoomDelta());
    }

    public override void Draw(GameTime gameTime)
    {
        DungeonGame.WorldSpriteBatch.TransformationMatrix = WorldCamera.GetViewMatrix();
        base.Draw(gameTime);
    }
}