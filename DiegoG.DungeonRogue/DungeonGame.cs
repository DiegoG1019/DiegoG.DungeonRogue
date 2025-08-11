using System;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using DiegoG.DungeonRogue.Data;
using DiegoG.DungeonRogue.GameComponents;
using DiegoG.DungeonRogue.Scenes;
using DiegoG.DungeonRogue.Services;
using DiegoG.MonoGame.Extended;
using DiegoG.MonoGame.Extended.Tasks;
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

    public static SpriteFont DebugFont => Instance.debugFont;
    private SpriteFont debugFont = null!;
    
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

    public static CLArgs ParsedCommandLine => Instance.clargs;
    private readonly CLArgs clargs;
    private readonly CallDeferrer deferrer;

    private bool initialized;
    
    private DungeonGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Components.ComponentAdded += (sender, args) =>
        {
            if (initialized)
                args.GameComponent.Initialize();
        };
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(LogEventLevel.Verbose)
            .CreateLogger();

        clargs = CommandLine.Parser.Default.ParseArguments<CLArgs>(Environment.GetCommandLineArgs()).Value;
        deferrer = Services.AddCallDeferrerService(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        var mainScene = new GameScene(this);
        var uiScene = new UIMenuScene(this);
        Components.Add(mainScene);
        Components.Add(uiScene);
        Components.Add(new BackgroundTasksWorkerComponent(this));

        Services.AddService(new GameState(new LocalGameState()
        {
            UIMenuScene = uiScene,
            GameScene = mainScene
        }));
        
        Services.AddPool<StringBuilder>(() => new StringBuilder(200));

        Window.AllowUserResizing = true;
        Window.Title = "Dungeon Game - By Diego García";

        //mainScene.SceneComponents.Add(new TestComponent(this));

        var count = Components.Count;
        var comparray = ArrayPool<IGameComponent>.Shared.Rent(count);
        try
        {
            Components.CopyTo(comparray, 0);
            Debug.Assert(count is 0 || count < comparray.Length);
            initialized = true;
            for (int i = 0; i < count; i++)
                comparray[i].Initialize();
        }
        finally
        {
            ArrayPool<IGameComponent>.Shared.Return(comparray);
        }
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

        debugFont = Content.Load<SpriteFont>("Fonts/Consolas");
        
        Components.Add(new ViewableFPSCounter(this, "Fonts/Consolas", uiSpriteBatch));
    }

    protected override void Update(GameTime gameTime)
    {
        deferrer.ExecuteUpdateStartDeferredCalls(gameTime);
        base.Update(gameTime);
        deferrer.ExecuteUpdateEndDeferredCalls(gameTime);
        mouse = new MouseStateMemory(mouse, Mouse.GetState());
        KeyboardExtended.Update();
        keyboard = KeyboardExtended.GetState();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(clearColor);
        deferrer.ExecuteDrawStartDeferredCalls(gameTime);

        backgroundSpriteBatch.BeginWithState();
        worldSpriteBatch.BeginWithState();
        hudSpriteBatch.BeginWithState();
        uiSpriteBatch.BeginWithState();
        imGuiRenderer.BeginLayout(gameTime);
        
        base.Draw(gameTime);
        
        deferrer.ExecuteDrawEndDeferredCalls(gameTime);
        
        backgroundSpriteBatch.End();
        worldSpriteBatch.End();
        hudSpriteBatch.End();
        uiSpriteBatch.End();
        imGuiRenderer.EndLayout();
    }
}
