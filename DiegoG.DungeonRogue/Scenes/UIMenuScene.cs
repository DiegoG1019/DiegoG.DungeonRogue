using System;
using System.Text;
using DiegoG.DungeonRogue.Services;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace DiegoG.DungeonRogue.Scenes;

public class UIMenuScene : Scene
{
    public UIMenuScene(Game game) : base(game)
    {
    }

    public bool DebugViews { get; set; } = true;

    private bool PlayerInfo;

    private readonly StringBuilder sb = new();
    
    public override void Draw(GameTime gameTime)
    {
        var gameState = Game.Services.GetService<GameState>();
        base.Draw(gameTime);

        if (DebugViews)
        {
            ImGui.Begin("Debug Tools");

            ImGui.Checkbox("Player Info", ref PlayerInfo);
            
            ImGui.End();
            
            if (PlayerInfo)
            {
                ImGui.Begin("Player Info");

                var player = gameState.Local.GameScene.Player;
                if (player is null) 
                    ImGui.Text("No player currently loaded");
                else
                {
                    sb.Clear();
                    player.DebugDump(sb, 0);

                    Span<char> buffer = ArrayPoolHelper.TrySkipRent<char>(sb.Length, out var rented)
                        ? stackalloc char[sb.Length]
                        : rented.Span;

                    try
                    {
                        sb.CopyTo(0, buffer, sb.Length);

                        int lastnl = -1;
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            if (buffer[i] == '\n')
                            {
                                var print = buffer[(lastnl + 1)..i];
                                ImGui.Text(print);
                                lastnl = i;
                            }
                        }

                        var pr = buffer[(lastnl + 1)..];
                        ImGui.Text(pr);
                    }
                    finally
                    {
                        rented.Dispose();
                    }
                }
                
                ImGui.End();
            }
        }
    }
}