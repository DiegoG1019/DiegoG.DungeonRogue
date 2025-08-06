using System;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using GLV.Shared.Common.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace DiegoG.DungeonRogue.GameComponents;

public class CameraAnchorComponent(Game game) : GameComponent(game), IDebugExplorable, IPositionable
{
    public void RenderImGuiDebug()
    {
        Span<char> sbb = stackalloc char[50];
        Span<char> bf = stackalloc char[18];
        ValueStringBuilder sb = new(sbb);
        ImGui.SameLine();
        
        sb.Append(" @ X: ");
        sb.Append(Position.X.ToStringSpan(bf, "0.000"));
        sb.Append(" | Y: ");
        sb.Append(Position.Y.ToStringSpan(bf, "0.000"));
        ImGui.Text(sb.AsSpan());
    }

    public Vector2 Position { get; set; }
}