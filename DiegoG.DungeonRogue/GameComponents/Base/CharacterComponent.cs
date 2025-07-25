using System;
using System.Text;
using DiegoG.DungeonRogue.Data;
using DiegoG.DungeonRogue.GameComponents.Controllers;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using GLV.Shared.Common.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Timers;

namespace DiegoG.DungeonRogue.GameComponents.Base;

public abstract class CharacterComponent : DrawableGameComponent, IMovable, ISpacePositionable, ISizable, IDebugExplorable
{
    public CharacterComponent(Game game) : base(game)
    {
        stoppedMovingTimer = new(StoppedMovingDelay);
    }
    
    public AnimatedSprite? Sprite { get; protected set; }

    public Vector2 Position { get; set; }

    public float Speed { get; set; } = .75f;

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
    
    public CharacterController? Controller { get; set; }
    
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
        Controller?.UpdateCharacter(this, gameTime);
        
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

    public virtual void RenderImGuiDebug()
    {
        Span<char> buffer = stackalloc char[800];
        var sb = new ValueStringBuilder(buffer);

        sb.Append("Sprite: ");
        sb.Append(Sprite?.TextureRegion.Name);
        sb.Append(" (");
        sb.Append(Sprite?.CurrentAnimation);
        sb.Append(')');
        ImGui.Text(sb.AsSpan());

        if (Sprite is not null)
        {
            if (ImGui.TreeNode(Sprite.GetHashCode().ToStringSpan(buffer), "Animation Controller"))
            {
                Sprite?.Controller.RenderImGuiDebug();
                ImGui.TreePop();
            }
        }

        var pos = Position.ToNumerics();
        if (ImGui.InputFloat2("Position", ref pos))
            Position = pos;

        var relpos = RelativePosition.ToNumerics();
        if (ImGui.InputFloat2("Relative Position", ref relpos))
            RelativePosition = relpos;

        var spd = Speed;
        if (ImGui.InputFloat("Speed", ref spd))
            Speed = spd;
        
        ImGui.LabelText("Moved Status", Enum.GetName(MovedStatus));
        
        var facdir = FacingDirection.ToNumerics();
        if (ImGui.InputFloat2("Facing Direction", ref facdir))
            FacingDirection = facdir;

        var smds = StoppedMovingDelay.TotalSeconds;
        if (ImGui.InputDouble("Stopped Moving Delay", ref smds))
            StoppedMovingDelay = TimeSpan.FromSeconds(smds);

        var size = ((Vector2)Size).ToNumerics();
        if (ImGui.InputFloat2("Size", ref size))
            Size = new(size.X, size.Y);
        
        ImGui.LabelText("Space", Space?.GetType().Name ?? "None assigned");
    }
    
    public ISpace? Space { get; set; }

    public SizeF Size { get; set; } = new(1, 1);
}