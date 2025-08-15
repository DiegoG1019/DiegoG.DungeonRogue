using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiegoG.DungeonRogue.Services;
using DiegoG.DungeonRogue.World.WorldGeneration;
using DiegoG.MonoGame.Extended;
using GLV.Shared.Common;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Framework.Utilities;

using Vertex = Microsoft.Xna.Framework.Graphics.VertexPosition; 

namespace DiegoG.DungeonRogue.World.Rendering;

public class Mesh3DGenerationRenderer(Game game) : IDungeonRenderer, IDebugExplorable
{
    private readonly PrerenderToTextureDungeonRenderer rend = new();
    
    private RenderTarget2D texture;
    private DungeonArea? renderedArea;
    
    private Vertex[]? triangleVertices;
    private ushort[]? triangleIndices;
    private VertexBuffer? vbuff;
    private IndexBuffer? ibuff;
    private int triangleCount;
    private RasterizerState? rasterizerState;
    private BasicEffect? fx;
    private GameState? state;

    #if DEBUG

    private bool show2dRendition = true;
    private bool showDebugScanResults = true;
    private bool showTriangulationLines = true;
    private bool showVertexLines = true;
    private bool showRooms = true;
    private bool showCorridors = true;
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

    public BasicEffect Effect
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

        state = game.Services.GetService<GameState>();

        return Task.CompletedTask;
    }

    public void Draw(GameTime gameTime)
    {   
#if DEBUG
        if (triangleIndices is not null && triangleVertices is not null && show2dRendition)
        {
            rend.Draw(gameTime);
            
            Debug.Assert(renderedArea is not null);
            
            if (showRooms)
            {
                var xs = renderedArea.Area.Grid.XScale;
                var ys = renderedArea.Area.Grid.YScale;
                var color = Color.YellowGreen with { A = 10 };
                foreach (var room in renderedArea.Rooms)
                    DungeonGame.WorldSpriteBatch.FillRectangle(
                        new RectangleF(
                            room.Area.X * xs,
                            room.Area.Y * ys,
                            room.Area.Width * xs,
                            room.Area.Height * ys
                        ), 
                        color
                    );
            }

            if (showCorridors)
            {
                var xs = renderedArea.Area.Grid.XScale;
                var ys = renderedArea.Area.Grid.YScale;
                var color = Color.Red with { A = 10 };
                foreach (var corr in renderedArea.Corridors)
                    DungeonGame.WorldSpriteBatch.FillRectangle(
                        new RectangleF(
                            corr.Area.X * xs,
                            corr.Area.Y * ys,
                            corr.Area.Width * xs,
                            corr.Area.Height * ys
                        ), 
                        color
                    );
            }
            
            var vertices = triangleVertices;

            if (showVertexLines)
            {
                var color = Color.IndianRed with { A = 80 };
                float thicc = 5;
                
                Vector2 a;
                Vector2 b;
                Debug.Assert(triangleIndices is not null);
                int ii = 0;
                a = ToVec2(vertices[ii++].Position);
                for (; ii < vertices.Length; ii++)
                {
                    b = a;
                    a = ToVec2(vertices[ii].Position);
                    
                    DungeonGame.WorldSpriteBatch.DrawLine(a, b, color, thicc);
                }
            }
            
            if (showTriangulationLines)
            {
                var color = Color.Chocolate with { A = 80 };
                float thicc = 2;
                
                DrawDebugViewTriangleList();
                
                
                void DrawDebugViewTriangleList()
                {
                    Vector2 a;
                    Vector2 b;
                    Vector2 c;
                    Debug.Assert(triangleIndices is not null);
                    Debug.Assert(triangleIndices.Length % 3 == 0);
                    int ii = 0;
                    while (ii < triangleIndices.Length) // It should always be a multiple of three
                    {
                        a = ToVec2(vertices[triangleIndices[ii++]].Position);
                        b = ToVec2(vertices[triangleIndices[ii++]].Position);
                        c = ToVec2(vertices[triangleIndices[ii++]].Position);

                        DungeonGame.WorldSpriteBatch.DrawLine(a, b, color, thicc);
                        DungeonGame.WorldSpriteBatch.DrawLine(b, c, color, thicc);
                        DungeonGame.WorldSpriteBatch.DrawLine(c, a, color, thicc);
                    }
                }
                
                void DrawDebugViewTriangleStrip()
                {
                    Vector2 a;
                    Vector2 b;
                    Debug.Assert(triangleIndices is not null);
                    int ii = 0;
                    a = ToVec2(vertices[triangleIndices[ii]].Position);
                    for (; ii < triangleIndices.Length; ii++)
                    {
                        b = a;
                        a = ToVec2(vertices[triangleIndices[ii]].Position);
                    
                        DungeonGame.WorldSpriteBatch.DrawLine(a, b, color, thicc);
                    }
                }
            }

            if (showDebugScanResults)
            {
                for (int i = 0; i < vertices.Length; i++)
                    PrintIndex(ToVec2(vertices[i].Position), Color.Orange, i, 0);

                for (int ii = 0; ii < triangleIndices.Length; ii++)
                    PrintIndex(ToVec2(vertices[triangleIndices[ii]].Position), Color.Chocolate, ii, 12);
                
                void PrintIndex(in Vector2 position, in Color clr, int index, int yoffset)
                {
                    showDebugScanResultsStringBuilder.Clear();
                    showDebugScanResultsStringBuilder.Append(index);
                    DungeonGame.WorldSpriteBatch.DrawString(DungeonGame.DebugFont, showDebugScanResultsStringBuilder, position with { Y = position.Y + yoffset }, clr);
                }
            }

            Vector2 ToVec2(in Vector3 vec) => new(vec.X, vec.Z);
        }

#endif

        Debug.Assert(Effect is not null);

        Debug.Assert(vbuff is not null);
        Debug.Assert(triangleVertices is not null);
        Debug.Assert(triangleVertices.Length > 0);

        Debug.Assert(ibuff is not null);
        Debug.Assert(triangleIndices is not null);
        Debug.Assert(triangleIndices.Length > 0);

        Debug.Assert(triangleCount > 0);

        game.GraphicsDevice.RasterizerState = rasterizerState;
        game.GraphicsDevice.SetVertexBuffer(vbuff);
        game.GraphicsDevice.Indices = ibuff;

        Debug.Assert(state is not null);
        
        var cam = state.Local.GameScene.WorldCamera3D;
        Effect.View = cam.View;
        Effect.World = cam.World;
        Effect.Projection = cam.Projection;

        foreach (var pass in Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            game.GraphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                triangleCount
            );
        }

        
    }

    private void ScanSurface()
    { 
        Debug.Assert(renderedArea is not null);

        List<Vertex> scannedVertices = new(renderedArea.Area.TotalCells);
        List<ushort> scannedIndices = new(scannedVertices.Count * 5);
        triangleCount = 0;

        /* // Rectangle triangulation

        0          1
        3          2

        triangle list
        indices: 0 1 3 1 2 3

        */

        foreach (var room in renderedArea.Rooms)
            AddRectangle(room.Area);
        
        foreach (var corr in renderedArea.Corridors)
            AddRectangle(corr.Area);
        
        #region Polygonization and triangulation // Commented out
        /*
         
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
        var pointList = points.Keys.ToList();
        
        // Polygonize the surface
        
        Point? a = null;
        Point? b = null;
        
        foreach (var p in pointList)
        {
            if (a == null)
                a = p;
            
            else if (b == null)
            {
                b = p;
                
                Point? ia = null;
                Point? ib = null;
                foreach (var ip in pointList)
                {
                    if (ia is null)
                    {
                        if (ip.X == a.Value.X)
                            ia = ip;
                    }
                    
                    else if (ib is null && ip.X == b.Value.X)
                    {
                        ib = ip;
                        var st = scannedVertices.Count;
                        scannedVertices.Add(ToVertex(a.Value));
                        scannedVertices.Add(ToVertex(b.Value));
                        scannedVertices.Add(ToVertex(ib.Value));
                        scannedVertices.Add(ToVertex(ia.Value));
                        
                        //indices: 0 1 3 1 2 3
                        scannedIndices.Add((ushort)(st + 0));
                        scannedIndices.Add((ushort)(st + 1));
                        scannedIndices.Add((ushort)(st + 3));
                        scannedIndices.Add((ushort)(st + 1));
                        scannedIndices.Add((ushort)(st + 2));
                        scannedIndices.Add((ushort)(st + 3));
                    }
                }

                a = b = null;
            }
            
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
        */
        #endregion
        
        // Finalize and output
        triangleIndices = scannedIndices.ToArray();
        triangleVertices = scannedVertices.ToArray();

        void AddRectangle(Rectangle rect)
        {
            var x = (int)renderedArea.Area.Grid.XScale;
            
            int i = AddTileVertex(rect.TopLeft(x));
            AddTileVertex(rect.TopRight(x));
            AddTileVertex(rect.BottomRight(x));
            AddTileVertex(rect.BottomLeft(x));
            
            scannedIndices.Add((ushort)(i + 0));
            scannedIndices.Add((ushort)(i + 1));
            scannedIndices.Add((ushort)(i + 3));
            scannedIndices.Add((ushort)(i + 1));
            scannedIndices.Add((ushort)(i + 2));
            scannedIndices.Add((ushort)(i + 3));

            triangleCount += 2;
        }
        
        int AddTileVertex(Point vec)
        {
            var c = scannedVertices.Count;
            scannedVertices.Add(new(new(vec.X, 0, vec.Y)));
            return c;
        }
    }

    private void CreateGraphicResources()
    {
        Debug.Assert(triangleVertices is not null);
        Debug.Assert(triangleVertices.Length > 0);
        
        Debug.Assert(triangleIndices is not null);
        Debug.Assert(triangleIndices.Length > 0);
        
        Debug.Assert(triangleCount > 0);

        fx ??= new BasicEffect(game.GraphicsDevice);
        
        rasterizerState ??= new RasterizerState();
        rasterizerState.CullMode = CullMode.None;

        var vbuffer = new VertexBuffer(
            game.GraphicsDevice, 
            typeof(Vertex), 
            triangleVertices.Length, 
            BufferUsage.WriteOnly
        );

        var ibuffer = new IndexBuffer(
            game.GraphicsDevice,
            IndexElementSize.SixteenBits,
            triangleIndices.Length,
            BufferUsage.WriteOnly
        );
        
        vbuffer.SetData(triangleVertices);
        ibuffer.SetData(triangleIndices);

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
        
        if (triangleVertices is null)
            ImGui.LabelText("Vertices", "null");
        else
            ImGui.LabelText("Vertices", triangleVertices.Length.ToStringSpan(buff));
        
        if (triangleIndices is null)
            ImGui.LabelText("Indices", "null");
        else
            ImGui.LabelText("Indices", triangleIndices.Length.ToStringSpan(buff));

#if DEBUG
        ImGui.Checkbox("Show 2D Version", ref show2dRendition);
        ImGui.BeginDisabled(show2dRendition is false);
        ImGui.Checkbox("\tShow Visual Scan Results", ref showDebugScanResults);
        ImGui.Checkbox("\tShow Visual Triangulation Lines", ref showTriangulationLines);
        ImGui.Checkbox("\tShow Visual Vertex Order", ref showVertexLines);
        ImGui.Checkbox("\tShow Room areas", ref showRooms);
        ImGui.Checkbox("\tShow Corridor areas", ref showCorridors);
        ImGui.EndDisabled();
#endif
    }
}