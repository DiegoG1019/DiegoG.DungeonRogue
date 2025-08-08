using System.Diagnostics;
using DiegoG.DungeonRogue.Services;
using DiegoG.MonoGame.Extended;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DiegoG.DungeonRogue.GameComponents.Base;

public abstract class Component3D(Game game) : DrawableGameComponent(game), IDebugExplorable
{
    // TODO: Check if setting the texture each frame is necessary
    // TODO: Check if you can't make a global Shader that takes the camera's values and something else to set the world for this one
    
    private bool trans_valid;
    private GameState? state;
    
    public Vector3 Position 
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTrans();
        }
    }

    public virtual Vector3 DefaultOriginOffset => default;

    public Vector3 OriginOffset
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTrans();
        }
    }

    public Vector3? AbsoluteOrigin
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTrans();
        }
    }
    
    public Vector3 Rotation 
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTrans();
        }
    }
    
    public Vector3 Scale 
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            InvalidateTrans();
        }
    } = Vector3.One;

    public Matrix Transformation { get; private set; }
    
    public Texture2D? Texture { get; protected set; }
    public Model? Model { get; protected set; }

    protected override void LoadContent()
    {
        state = Game.GetService<GameState>();
        OriginOffset = DefaultOriginOffset;
    }

    public override void Draw(GameTime gameTime)
    {
        if (Model is null) return;

        Debug.Assert(state is not null);
        
        var cam = state.Local.GameScene.WorldCamera3D;
        var view = cam.View;
        var world = Transformation * cam.World;
        var proj = cam.Projection;
        
        foreach (var mesh in Model.Meshes)
        {
            foreach (var fx in mesh.Effects)
            {
                if (fx is BasicEffect bfx)
                {
                    bfx.Texture = Texture;
                    bfx.TextureEnabled = Texture is not null;
                    bfx.EnableDefaultLighting();
                    bfx.AmbientLightColor = new Vector3(1, 0.5f, .5f);
                    bfx.View = view;
                    bfx.World = world;
                    bfx.Projection = proj;
                }
            }

            mesh.Draw();
        }
    }

    public override void Update(GameTime gameTime)
    {
        Debug.Assert(state is not null);
        
        if (trans_valid is false)
        {
            var origin = (AbsoluteOrigin ?? Position) + OriginOffset;

            Transformation =
                Matrix.CreateTranslation(-origin) *
                Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) *
                Matrix.CreateScale(Scale) *
                Matrix.CreateTranslation(Position) *
                Matrix.CreateTranslation(origin);
            
            trans_valid = false;
        }
    }

    public void SetDefaultOrigin() => OriginOffset = DefaultOriginOffset; 

    private void InvalidateTrans()
    {
        trans_valid = false;
    }
    
    public virtual void RenderImGuiDebug()
    {
        var pos = Position.ToNumerics();
        if (ImGui.InputFloat3("Position", ref pos))
            Position = pos;

        var rotdeg = new System.Numerics.Vector3(float.RadiansToDegrees(Rotation.X), float.RadiansToDegrees(Rotation.Y), float.RadiansToDegrees(Rotation.Z));
        if (ImGui.InputFloat3("Rotation Degrees (Yaw, Pitch, Roll)", ref rotdeg))
            Rotation = new Vector3(float.DegreesToRadians(rotdeg.X), float.DegreesToRadians(rotdeg.Y), float.DegreesToRadians(rotdeg.Z));

        var rot = Rotation.ToNumerics();
        if (ImGui.InputFloat3("Rotation Radians (Yaw, Pitch, Roll)", ref rot))
            Rotation = rot;
        
        var scale = Scale.ToNumerics();
        if (ImGui.InputFloat3("Scale", ref scale))
            Scale = scale;

        ImGui.Text("Model:\t");
        ImGui.SameLine();
        if (Model is null)
            ImGui.TextColored(Color.Red.ToVector4().ToNumerics(), "null");
        else
            ImGui.Text(Model.Tag?.ToString() ?? Model.ToString() ?? "not null");

        ImGui.Text("Texture:\t");
        ImGui.SameLine();
        if (Texture is null)
            ImGui.TextColored(Color.Red.ToVector4().ToNumerics(), "null");
        else
            ImGui.Text(Texture.Name ?? "not null");
        
        var ori = OriginOffset.ToNumerics();
        if (ImGui.InputFloat3("Origin Offset", ref ori))
            OriginOffset = ori;

        ImGui.Text("Absolute Origin");
        ImGui.SameLine();
        var oriisnull = AbsoluteOrigin is null;
        if (ImGui.Checkbox("Null", ref oriisnull))
            AbsoluteOrigin = oriisnull ? null : new();

        if (AbsoluteOrigin is Vector3 absOri)
        {
            var aor = absOri.ToNumerics();
            if (ImGui.InputFloat3("Absolute Origin", ref aor))
                AbsoluteOrigin = ori;
        }
    }
}