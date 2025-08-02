using System;
using System.Threading.Tasks;
using DiegoG.DungeonRogue.World.WorldGeneration;
using DiegoG.MonoGame.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;

namespace DiegoG.DungeonRogue.World.Rendering;

public class PrerenderToTextureDungeonRenderer : IDungeonRenderer
{
    private RenderTarget2D texture;
    
    public Task Initialize(DungeonArea area)
    {
        ArgumentNullException.ThrowIfNull(area);

        DungeonGame.Instance.DeferToDrawStart((game, time) =>
        {
            var size = area.Area.TotalAreaRectangle.Size;
            texture = new RenderTarget2D(
                DungeonGame.Graphics.GraphicsDevice, 
                (int)size.Width, 
                (int)size.Height,
                false, 
                SurfaceFormat.Color,
                DepthFormat.None
            );
            
            var atlas = area.DungeonInfo.GetAtlasFor(area.Id);
            DungeonGame.Graphics.GraphicsDevice.SetRenderTarget(texture);
            DungeonGame.Graphics.GraphicsDevice.Clear(Color.Transparent);
            
            var sb = new SpriteBatch(game.GraphicsDevice);
            sb.Begin();
            
            foreach (var cell in area.TileData.GetCells())
            {
                switch (cell.Data.TileId)
                {
                    case TileId.Normal:
                        sb.Draw(atlas[0], cell.GetPosition(), Color.White);
                        break;
                    
                    case TileId.Entry:
                        sb.Draw(atlas[17], cell.GetPosition(), Color.White);
                        break;
                    
                    case TileId.Exit:
                        sb.Draw(atlas[16], cell.GetPosition(), Color.White);
                        break;
                    
                    case TileId.Invalid:
                        sb.DrawMissingTextureSquare(cell.Area);
                        break;
                    
                    case TileId.Empty:
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            sb.End();
            DungeonGame.Graphics.GraphicsDevice.SetRenderTarget(null);
        });

        return Task.CompletedTask;
    }

    public void Draw(GameTime gameTime)
    {
        DungeonGame.WorldSpriteBatch.Draw(texture, default(Vector2), Color.White);
    }

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
}