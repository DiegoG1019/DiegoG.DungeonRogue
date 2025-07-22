using System;
using System.Text;
using DiegoG.DungeonRogue.Data;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Timers;

namespace DiegoG.DungeonRogue.Components;

public abstract class CharacterComponent : DrawableGameComponent, IMovable, ISpacePositionable, ISizable
{
    public CharacterComponent(Game game) : base(game)
    {
        stoppedMovingTimer = new(StoppedMovingDelay);
    }
    
    public AnimatedSprite? Sprite { get; protected set; }

    public Vector2 Position { get; set; }

    public float Speed { get; set; } = 1;

    public Vector2 FacingDirection
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            DirectionChanged();
        }
    }
    
    public MovedStatus MovedStatus { get; private set; }

    public TimeSpan StoppedMovingDelay
    {
        get;
        set
        {
            stoppedMovingTimer.Interval = value;
            field = value;
        }
    }

    private readonly CountdownTimer stoppedMovingTimer;
    
    public virtual void Move(Vector2 direction)
    {
        Position += direction * Speed;
        FacingDirection = direction;

        if (MovedStatus is not MovedStatus.StartedMoving and not MovedStatus.Moved)
        {
            MovedStatus = MovedStatus.Moved;
            MovedChanged();
        } 
        
        MovedStatus = MovedStatus.StartedMoving;
    }
    
    protected virtual void DirectionChanged(){}

    protected virtual void MovedChanged(){}
    
    public Vector2 RelativePosition
    {
        get => ISpacePositionable.GetRelativePosition(this);
        set => Position = ISpacePositionable.ConvertToRelativePosition(this, value);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Sprite?.Update(gameTime);
        stoppedMovingTimer.Update(gameTime);
        
        switch (MovedStatus)
        {
            // This only serves to prevent a transition into Stopping the same frame movement starts
            case MovedStatus.StartedMoving:
                MovedStatus = MovedStatus.Moved;
                break;
            
            case MovedStatus.Moved:
                MovedChanged();
                MovedStatus = MovedStatus.Stopping;
                if (StoppedMovingDelay > TimeSpan.Zero) 
                    stoppedMovingTimer.Start();
                break;
            
            case MovedStatus.Stopping when stoppedMovingTimer.State != TimerState.Started:
                MovedChanged();
                MovedStatus = MovedStatus.NotMoved;
                break;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        Sprite?.Draw(DungeonGame.WorldSpriteBatch, RelativePosition, 0, Size);
    }

    protected internal virtual void DebugDump(StringBuilder sb, int tabs)
    {
        Span<char> buffer = stackalloc char[800];

        sb.AppendTabs(tabs).Append("Sprite: ").Append(Sprite?.TextureRegion.Name).Append(" (").Append(Sprite?.CurrentAnimation)
            .Append(")\n");

        sb.AppendTabs(tabs).AppendLine("Anim: ");
        Sprite?.Controller.DebugDump(sb, 1);
        sb.AppendTabs(tabs).AppendLine();

        sb.AppendTabs(tabs).Append("Position: ").Append(Position.ToStringSpan(buffer)).AppendLine();
        sb.AppendTabs(tabs).Append("RelativePosition: ").Append(RelativePosition.ToStringSpan(buffer)).AppendLine();
        sb.AppendTabs(tabs).Append("Speed: ").Append(Speed.ToStringSpan(buffer)).AppendLine();
        sb.AppendTabs(tabs).Append("MovedStatus: ").Append(Enum.GetName(MovedStatus)).AppendLine();
        sb.AppendTabs(tabs).Append("FacingDirection: ").Append(FacingDirection.ToStringSpan(buffer)).AppendLine();
        sb.AppendTabs(tabs).Append("StoppedMovingDelay: ").Append(StoppedMovingDelay.ToStringSpan(buffer)).AppendLine();
        sb.AppendTabs(tabs).Append("Size: ").Append(Size.ToStringSpan(buffer)).AppendLine();
        
        sb.AppendTabs(tabs).Append("Space: ").Append(Space?.GetType().Name).AppendLine();
    }
    
    public ISpace? Space { get; set; }

    public SizeF Size { get; set; } = new(10, 10);
}