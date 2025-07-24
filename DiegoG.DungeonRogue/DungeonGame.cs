using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using DiegoG.DungeonRogue.GameComponents;
using DiegoG.DungeonRogue.Scenes;
using DiegoG.DungeonRogue.Services;
using DiegoG.MonoGame.Extended;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using MonoGame.ImGuiNet;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DiegoG.DungeonRogue;

public partial class DungeonGame : Game
{
    public static DungeonGame Instance { get; } = new();

    public static GraphicsDeviceManager Graphics => Instance.graphics;
    private readonly GraphicsDeviceManager graphics;

    public static ImGuiRenderer ImGuiRenderer => Instance.imGuiRenderer;
    private ImGuiRenderer imGuiRenderer = null!;

    public static Color ClearColor
    {
        get => Instance.clearColor;
        set => Instance.clearColor = value;
    }

    private Color clearColor = Color.CornflowerBlue;
    
    public static StatefulSpriteBatch WorldSpriteBatch => Instance.worldSpriteBatch;
    private StatefulSpriteBatch worldSpriteBatch = null!;

    public static StatefulSpriteBatch BackgroundSpriteBatch => Instance.backgroundSpriteBatch;
    private StatefulSpriteBatch backgroundSpriteBatch = null!;

    public static StatefulSpriteBatch UISpriteBatch => Instance.uiSpriteBatch;
    private StatefulSpriteBatch uiSpriteBatch = null!;

    public static StatefulSpriteBatch HUDSpriteBatch => Instance.hudSpriteBatch;
    private StatefulSpriteBatch hudSpriteBatch = null!;

    public static MouseStateMemory MouseState => Instance.mouse;
    private MouseStateMemory mouse;
    
    public static KeyboardStateExtended KeyboardState => Instance.keyboard;
    private KeyboardStateExtended keyboard;
    
    private DungeonGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Components.ComponentAdded += (sender, args) => args.GameComponent.Initialize();
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(LogEventLevel.Verbose)
            .CreateLogger();
    }

    protected override void Initialize()
    {
        base.Initialize();

        var mainScene = new GameScene(this);
        var uiScene = new UIMenuScene(this);
        Components.Add(mainScene);
        Components.Add(uiScene);

        Services.AddService(new GameState(new LocalGameState()
        {
            UIMenuScene = uiScene,
            GameScene = mainScene
        }));
        
        Services.AddPool<StringBuilder>(() => new StringBuilder(200));

        Window.AllowUserResizing = true;
        Window.Title = "Dungeon Game - By Diego García";

        //mainScene.SceneComponents.Add(new TestComponent(this));
    }

    protected override void LoadContent()
    {
        backgroundSpriteBatch = new(GraphicsDevice);
        uiSpriteBatch = new(GraphicsDevice);
        hudSpriteBatch = new(GraphicsDevice);
        worldSpriteBatch = new(GraphicsDevice)
        {
            SamplerState = SamplerState.PointClamp
        };

        imGuiRenderer = new(this);
        imGuiRenderer.RebuildFontAtlas();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        mouse = new MouseStateMemory(mouse, Mouse.GetState());
        KeyboardExtended.Update();
        keyboard = KeyboardExtended.GetState();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(clearColor);

        backgroundSpriteBatch.BeginWithState();
        worldSpriteBatch.BeginWithState();
        hudSpriteBatch.BeginWithState();
        uiSpriteBatch.BeginWithState();
        imGuiRenderer.BeginLayout(gameTime);
        
        base.Draw(gameTime);

        backgroundSpriteBatch.End();
        worldSpriteBatch.End();
        hudSpriteBatch.End();
        uiSpriteBatch.End();
        imGuiRenderer.EndLayout();
    }
}
