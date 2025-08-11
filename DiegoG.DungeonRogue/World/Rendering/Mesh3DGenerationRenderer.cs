using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using DiegoG.DungeonRogue.World.WorldGeneration;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Framework.Utilities;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace DiegoG.DungeonRogue.World.Rendering;

public class Mesh3DGenerationRenderer(Game game) : IDungeonRenderer, IDebugExplorable
{
    private readonly PrerenderToTextureDungeonRenderer rend = new();
    
    private RenderTarget2D texture;
    private DungeonArea? renderedArea;
    
    private VertexPositionTexture[]? triangleStripVertices;
    private ushort[]? triangleStripIndices;
    private VertexBuffer? vbuff;
    private IndexBuffer? ibuff;
    private int triangleCount;
    private RasterizerState? rasterizerState;
    private Effect? fx;

    #if DEBUG

    private bool showDebugScanResults = true;
    private readonly StringBuilder showDebugScanResultsStringBuilder = new();
    
    #endif 
    
    public int DrawOrder
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
            DrawOrderChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool Visible
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
            VisibleChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler<EventArgs>? DrawOrderChanged;

    public event EventHandler<EventArgs>? VisibleChanged;

    public Effect Effect
    {
        get
        {
            Debug.Assert(fx is not null);
            return fx;
        }
        set => fx = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public Task Initialize(DungeonArea area)
    {
        if (renderedArea is not null)
            throw new InvalidOperationException("Cannot initialize twice");
        ArgumentNullException.ThrowIfNull(area);

        renderedArea = area;
        rend.Initialize(area);

        area.ActivityMessage = "Scanning the generated surface";
        ScanSurface();

        area.ActivityMessage = "Creating 3D Resources";
        if (PlatformInfo.GraphicsBackend is GraphicsBackend.OpenGL)
            game.DeferToDrawStart((_, _) => CreateGraphicResources());
        else
            CreateGraphicResources();

        return Task.CompletedTask;
    }

    public void Draw(GameTime gameTime)
    {
        rend.Draw(gameTime);
        
#if DEBUG

        if (showDebugScanResults && triangleStripIndices is not null && triangleStripVertices is not null)
        {
            var vertices = triangleStripVertices;

            var color = Color.Orange with { A = 80 };
            float thicc = 4;
            Vector2 a;
            Vector2 b;
            
            color = Color.Chocolate;// with { A = 255 };
            thicc = 2;

            int ii = 0;
            a = ToVec2(vertices[triangleStripIndices[ii]].Position);
            for (; ii < triangleStripIndices.Length; ii++)
            {
                b = a;
                a = ToVec2(vertices[triangleStripIndices[ii]].Position);
                
                DungeonGame.WorldSpriteBatch.DrawLine(a, b, color, thicc);
            }
            
            for (int i = 0; i < vertices.Length; i++)
                PrintIndex(ToVec2(vertices[i].Position), Color.Orange, i, 0);

            for (ii = 0; ii < triangleStripIndices.Length; ii++)
                PrintIndex(ToVec2(vertices[triangleStripIndices[ii]].Position), Color.Chocolate, ii, 12);
            
            Vector2 ToVec2(in Vector3 vec) => new(vec.X, vec.Z);

            void PrintIndex(in Vector2 position, in Color clr, int index, int yoffset)
            {
                showDebugScanResultsStringBuilder.Clear();
                showDebugScanResultsStringBuilder.Append(index);
                DungeonGame.WorldSpriteBatch.DrawString(DungeonGame.DebugFont, showDebugScanResultsStringBuilder, position with { Y = position.Y + yoffset }, clr);
            }
        }

#endif

        /*

        Debug.Assert(Effect is not null);

        Debug.Assert(vbuff is not null);
        Debug.Assert(triangleStripVertices is not null);
        Debug.Assert(triangleStripVertices.Length > 0);

        Debug.Assert(ibuff is not null);
        Debug.Assert(triangleStripIndices is not null);
        Debug.Assert(triangleStripIndices.Length > 0);

        Debug.Assert(triangleCount > 0);

        game.GraphicsDevice.RasterizerState = rasterizerState;
        game.GraphicsDevice.SetVertexBuffer(vbuff);
        game.GraphicsDevice.Indices = ibuff;

        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            game.GraphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleStrip,
                0,
                0,
                triangleCount
            );
        }

        */
    }

    private void ScanSurface()
    {
        Debug.Assert(renderedArea is not null);
        
        List<VertexPositionTexture> scannedVertices = new(renderedArea.Area.TotalCells);
        Dictionary<Point, int> points = [];
        
        // Scan all corners of a cell
        foreach (var cell in renderedArea.TileData.GetCells())
        {
            if (cell.Data.TileId == TileId.Empty) continue;
            AddPoint(cell.GetPosition().ToPoint());
            AddPoint(cell.GetPosition(GridPositionOffset.CellSize).ToPoint());
            AddPoint(cell.GetPosition(GridPositionOffset.CellSize, GridPositionOffset.CellSize).ToPoint());
            AddPoint(cell.GetPosition(GridPositionOffset.None, GridPositionOffset.CellSize).ToPoint());
        }

        // Discard corners that don't represent an edge of the larger figure
        foreach (var (pt, c) in points)
            if (c > 1 && c != 3)
                points.Remove(pt);
        
        // Polygonize the surface
        
        // Create proper vertices out of the polygons
        foreach (var (p, _) in points)
            scannedVertices.Add(new(new(p.X, 0, p.Y), default));
        
        // Triangulate the vertices
        List<ushort> scannedIndices = new(scannedVertices.Count * 3); // Currently triangulatingn't
        for (int i = 0; i < scannedVertices.Count; i++) 
            scannedIndices.Add((ushort)i);

        // Finalize and output
        triangleStripIndices = scannedIndices.ToArray();
        triangleStripVertices = scannedVertices.ToArray();
        triangleCount = 10;

        void AddPoint(Point point)
        {
            if (points.TryGetValue(point, out var x) is false)
            {
                x = 0;
                points[point] = 0;
            }
            
            points[point] = x + 1;
        }
    }

    private void CreateGraphicResources()
    {
        Debug.Assert(triangleStripVertices is not null);
        Debug.Assert(triangleStripVertices.Length > 0);
        
        Debug.Assert(triangleStripIndices is not null);
        Debug.Assert(triangleStripIndices.Length > 0);
        
        Debug.Assert(triangleCount > 0);

        fx ??= new BasicEffect(game.GraphicsDevice);
        
        rasterizerState ??= new RasterizerState();
        rasterizerState.CullMode = CullMode.None;

        var vbuffer = new VertexBuffer(
            game.GraphicsDevice, 
            typeof(VertexPositionTexture), 
            triangleStripVertices.Length, 
            BufferUsage.WriteOnly
        );

        var ibuffer = new IndexBuffer(
            game.GraphicsDevice,
            IndexElementSize.SixteenBits,
            triangleStripIndices.Length,
            BufferUsage.WriteOnly
        );
        
        vbuffer.SetData(triangleStripVertices);
        ibuffer.SetData(triangleStripIndices);

        ibuff = ibuffer;
        vbuff = vbuffer;
    }

    public void RenderImGuiDebug()
    {
        Span<char> buff = stackalloc char[20];
        
        if (ImGui.Button("Rescan Surface and Reload 3D Mesh"))
        {
            ScanSurface();
            CreateGraphicResources();
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Rescan Surface Only"))
            ScanSurface();
        
        ImGui.TextColored(Color.IndianRed.ToVector4().ToNumerics(), "* Both of these can stall the game for a bit");
        
        ImGui.LabelText("Triangle count", triangleCount.ToStringSpan(buff));
        
        if (triangleStripVertices is null)
            ImGui.LabelText("Vertices", "null");
        else
            ImGui.LabelText("Vertices", triangleStripVertices.Length.ToStringSpan(buff));
        
        if (triangleStripIndices is null)
            ImGui.LabelText("Indices", "null");
        else
            ImGui.LabelText("Indices", triangleStripIndices.Length.ToStringSpan(buff));

#if DEBUG
        ImGui.Checkbox("Show Visual Scan Results", ref showDebugScanResults);
#endif
    }
}