using System;
using System.Text;
using DiegoG.DungeonRogue.Data;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using Serilog;

namespace DiegoG.DungeonRogue.Components;

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
        Sprite.Effect = FacingDirection.X < 0 ? SpriteEffects.FlipHorizontally : default;
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

    protected internal override void DebugDump(StringBuilder sb, int tabs)
    {
        base.DebugDump(sb, tabs);
        
        sb.AppendTabs(tabs).Append("Class: ").Append(Enum.GetName(Class)).AppendLine();
        sb.AppendTabs(tabs).Append("ArmorTier: ").Append(Enum.GetName(ArmorTier)).AppendLine();
    }
}