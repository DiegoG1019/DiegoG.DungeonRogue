using System;
using DiegoG.DungeonRogue.Data;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace DiegoG.DungeonRogue;

public static class AssetHelpers
{
    public static AnimatedSprite PlayerCharacterAnimation(string texture, Func<int, int> pla)
    {
        var inst = DungeonGame.Instance;
        
        Texture2D adventurerTexture = inst.Content.Load<Texture2D>(texture);
        Texture2DAtlas atlas = Texture2DAtlas.Create($"Atlas/{texture}", adventurerTexture, 12, 15);
        SpriteSheet spriteSheet = new SpriteSheet($"SpriteSheet/{texture}", atlas);

        spriteSheet.DefineAnimation(nameof(PlayerCharacterAnim.Idle), builder =>
        {
            builder.IsLooping(true)
                .AddFrame(regionIndex: 0, duration: TimeSpan.FromSeconds(5))
                .AddFrame(1, TimeSpan.FromSeconds(1));
        });

        spriteSheet.DefineAnimation(nameof(PlayerCharacterAnim.Walk), builder =>
        {
            builder.IsLooping(true)
                .AddFrame(2, TimeSpan.FromSeconds(0.1))
                .AddFrame(3, TimeSpan.FromSeconds(0.1))
                .AddFrame(4, TimeSpan.FromSeconds(0.1))
                .AddFrame(5, TimeSpan.FromSeconds(0.1))
                .AddFrame(6, TimeSpan.FromSeconds(0.1))
                .AddFrame(7, TimeSpan.FromSeconds(0.1));
        });

        spriteSheet.DefineAnimation(nameof(PlayerCharacterAnim.Dead), builder =>
        {
            builder.IsLooping(false)
                .AddFrame(8, TimeSpan.FromSeconds(0.1))
                .AddFrame(9, TimeSpan.FromSeconds(0.1))
                .AddFrame(10, TimeSpan.FromSeconds(0.1))
                .AddFrame(11, TimeSpan.FromSeconds(0.1))
                .AddFrame(12, TimeSpan.FromSeconds(0.1));
        });

        spriteSheet.DefineAnimation(nameof(PlayerCharacterAnim.Attack), builder =>
        {
            builder.IsLooping(false)
                .AddFrame(13, TimeSpan.FromSeconds(0.1))
                .AddFrame(14, TimeSpan.FromSeconds(0.1))
                .AddFrame(15, TimeSpan.FromSeconds(0.1));
        });

        spriteSheet.DefineAnimation(nameof(PlayerCharacterAnim.Use), builder =>
        {
            builder.IsLooping(false)
                .AddFrame(16, TimeSpan.FromSeconds(0.1))
                .AddFrame(17, TimeSpan.FromSeconds(0.1));
        });
        
        spriteSheet.DefineAnimation(nameof(PlayerCharacterAnim.Scroll), builder =>
        {
            builder.IsLooping(false)
                .AddFrame(18, TimeSpan.FromSeconds(0.1))
                .AddFrame(19, TimeSpan.FromSeconds(0.1))
                .AddFrame(20, TimeSpan.FromSeconds(0.1));
        });

        return new AnimatedSprite(spriteSheet, nameof(PlayerCharacterAnim.Idle));
    }
}