
using CalamityMod.Items;
using CalamityMod;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Rarities;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using CalamityMod.EntitySources;
using System;

namespace stormWeaverStaff.Items
{
    [AutoloadEquip(EquipType.Legs)]
    public class ProstheticTail : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.SacrificeTotal = 1;
            base.DisplayName.SetDefault("Prosthetic Tail");
            base.Tooltip.SetDefault("Immunity to Electrified\n10% increased damage\n50% increased move speed\nIncreased airborne mobility\nDon't ask how it works.\n'Baby Weaver loves you!'");
        }

        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = Item.sellPrice(platinum: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Cyan; // The rarity of the item
            Item.defense = 15;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.1f;
            player.buffImmune[BuffID.Electrified] = true;
            player.moveSpeed += 0.5f;
            //player.runAcceleration += 0.5f;
            //player.accRunSpeed *= 2f; 
            player.GetModPlayer<FamiliarPlayer>().prosTail = true;
            player.jumpSpeedBoost += 1f;
            player.maxFallSpeed *= 1.25f;
            player.noFallDmg = true;
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1f;
            ascentWhenRising = 0.175f;
            maxAscentMultiplier = 3f;
            maxCanAscendMultiplier = 1.1f;
            constantAscend = 0.2f;
        }

        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<FamiliarPlayer>().prosTail = true;
        }
    }

    public class TailDraw : PlayerDrawLayer
    {
        private Asset<Texture2D> texture;

        // Returning true in this property makes this layer appear on the minimap player head icon.
        public override bool IsHeadLayer => true;

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            // The layer will be visible only if the player is holding an ExampleItem in their hands. Or if another modder forces this layer to be visible.
            return (drawInfo.drawPlayer.GetModPlayer<FamiliarPlayer>().prosTail ? true : false);//drawInfo.drawPlayer.armor[12].type == ModContent.ItemType<ProstheticTail>());

            // If you'd like to reference another PlayerDrawLayer's visibility,
            // you can do so by getting its instance via ModContent.GetInstance<OtherDrawLayer>(), and calling GetDefaultVisiblity on it
        }

        // This layer will be a 'child' of the head layer, and draw before (beneath) it.
        // If the Head layer is hidden, this layer will also be hidden.
        // If the Head layer is moved, this layer will move with it.
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Wings);
        // If you want to make a layer which isn't a child of another layer, use `new Between(Layer1, Layer2)` to specify the position.
        // If you want to make a 'mobile' layer which can render in different locations depending on the drawInfo, use a `Multiple` position.

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            // The following code draws ExampleItem's texture behind the player's head.

            /*if (texture == null)
            {
                texture = ModContent.Request<Texture2D>("stormWeaverStaff/Items/MandibleReplica_Back");
            }
            int b = 0;
            List<int> ints = new List<int>()
            {
                1, 2, 3, 8, 9, 10
            };
            if (ints.Contains(drawInfo.hairBackFrame.Top / 56))
            {
                b = 2;
            }
            var position = drawInfo.Center + new Vector2(9f * drawInfo.drawPlayer.direction, -19f - b) - Main.screenPosition;
            position = new Vector2((int)position.X, (int)position.Y); // You'll sometimes want to do this, to avoid quivering.

            // Queues a drawing of a sprite. Do not use SpriteBatch in drawlayers!
            drawInfo.DrawDataCache.Add(new DrawData(
                texture.Value, // The texture to render.
                position + new Vector2(0, (player.gravDir == -1 ? -2 : 0)), // Position to render at.
                null, // Source rectangle.
                Color.White, // Color.
                0f, // Rotation.
                texture.Size() * 0.5f, // Origin. Uses the texture's center.
                1f, // Scale.
                drawInfo.drawPlayer.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, // SpriteEffects.
                0 // 'Layer'. This is always 0 in Terraria.
            ));*/
            float colorvalue = 0;
            if (drawInfo.drawPlayer.Calamity().rogueStealth > 0f && drawInfo.drawPlayer.Calamity().rogueStealthMax > 0f && drawInfo.drawPlayer.townNPCs < 3f && CalamityConfig.Instance.StealthInvisibility)
            {
                colorvalue = drawInfo.drawPlayer.Calamity().rogueStealth / drawInfo.drawPlayer.Calamity().rogueStealthMax * 0.9f;
            }
            var modplayer = drawInfo.drawPlayer.GetModPlayer<FamiliarPlayer>();
            var tex1 = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarBody").Value;
            var tex2 = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarTail").Value;
            var tex1G = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarBodyGlow").Value;
            var tex2G = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarTailGlow").Value;


            if (modplayer.tailPos != null)
            {
                for (var i = modplayer.tailPos.Count - 1; i >= 0; i--)
                {
                    var color = Lighting.GetColor((modplayer.tailPos[i].Item1/16).ToPoint());
                    if (i == modplayer.tailPos.Count - 1)
                    {
                        drawInfo.DrawDataCache.Add(new DrawData(tex2, modplayer.tailPos[i].Item1 - Main.screenPosition, null, new Color((int)(color.R * (1f - colorvalue * 0.89f)), (int)(color.G * (1f - colorvalue)), (int)(color.B * (1f - colorvalue * 0.89f)), (int)(color.A * (1f - colorvalue * 0.89f))) * drawInfo.drawPlayer.stealth, modplayer.tailPos[i].Item2 + (float)Math.PI, tex2.Size() / 2f, 0.75f, SpriteEffects.None, 0));
                        drawInfo.DrawDataCache.Add(new DrawData(tex2G, modplayer.tailPos[i].Item1 - Main.screenPosition, null, new Color(((1f - colorvalue * 0.89f)), ((1f - colorvalue)), ((1f - colorvalue * 0.89f)), ((1f - colorvalue * 0.89f))) * drawInfo.drawPlayer.stealth, modplayer.tailPos[i].Item2 + (float)Math.PI, tex2.Size() / 2f, 0.75f, SpriteEffects.None, 0));
                    }
                    else
                    {
                        drawInfo.DrawDataCache.Add(new DrawData(tex1, modplayer.tailPos[i].Item1 - Main.screenPosition, null, new Color((int)(color.R * (1f - colorvalue * 0.89f)), (int)(color.G * (1f - colorvalue)), (int)(color.B * (1f - colorvalue * 0.89f)), (int)(color.A * (1f - colorvalue * 0.89f))) * drawInfo.drawPlayer.stealth, modplayer.tailPos[i].Item2 + (float)Math.PI, tex1.Size() / 2f, 0.75f, SpriteEffects.None, 0));
                        drawInfo.DrawDataCache.Add(new DrawData(tex1G, modplayer.tailPos[i].Item1 - Main.screenPosition, null, new Color(((1f - colorvalue * 0.89f)), ((1f - colorvalue)), ((1f - colorvalue * 0.89f)), ((1f - colorvalue * 0.89f))) * drawInfo.drawPlayer.stealth, modplayer.tailPos[i].Item2 + (float)Math.PI, tex1.Size() / 2f, 0.75f, SpriteEffects.None, 0));
                    }
                }
            }
        }
    }
}