using System;
using System.Text;
using DiegoG.DungeonRogue.Data;
using DiegoG.DungeonRogue.GameComponents.Base;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using Serilog;

namespace DiegoG.DungeonRogue.GameComponents;

public class PlayerCharacterComponent : CharacterComponent
{
    public PlayerCharacterComponent(Game game, PlayerClass playerClass) : base(game)
    {
        Class = playerClass;
        UpdateOrder = int.MinValue;
    }
    
    public PlayerClass Class { get; }
    
    public ArmorTier ArmorTier { get; set; }

    public int AnimationIndexModifier(int index)
        => index + (20 * (int)ArmorTier);

    protected override void DirectionChanged()
    {
        if (Sprite is null) return;
        
        Sprite.Effect = FacingDirection.X switch
        {
            < 0 => SpriteEffects.FlipHorizontally,
            > 0 => default,
            _ => Sprite.Effect
        };
    }

    protected override void MovedChanged()
    {
        base.MovedChanged();

        if (Sprite is null) return;
        
        var walkAnim = PlayerCharacterAnim.Walk.GetName();
        var idleAnim = PlayerCharacterAnim.Idle.GetName();
            
        switch (MovedStatus)
        {
            case MovedStatus.Moved when Sprite.CurrentAnimation != walkAnim:
                Sprite.SetAnimation(walkAnim);
                break;
                
            case MovedStatus.Stopping or MovedStatus.NotMoved when Sprite.CurrentAnimation != idleAnim:
                Sprite.SetAnimation(idleAnim);
                break;
        }
    }

    protected override void LoadContent()
    {
        var texname = Class switch
        {
            PlayerClass.Warrior => "Sprites/warrior",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        Sprite = AssetHelpers.PlayerCharacterAnimation(texname, AnimationIndexModifier);
    }

    public override void RenderImGuiDebug()
    {
        base.RenderImGuiDebug();
        
        ImGui.LabelText("Player Class", Enum.GetName(Class));
        ImGui.LabelText("Armor Tier", Enum.GetName(ArmorTier));
    }
}