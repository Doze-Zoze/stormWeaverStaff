
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
using System.Security.Policy;
using System.Collections.Generic;
using CalamityMod.NPCs;
using CalamityMod.Items.Armor.OmegaBlue;
using CalamityMod.CalPlayer;

namespace stormWeaverStaff.Items
{
    [AutoloadEquip(EquipType.Head)]
    public class MandibleReplica : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.SacrificeTotal = 1;
            base.DisplayName.SetDefault("Mandible Replica");
            base.Tooltip.SetDefault("Immunity to Electrified\n+3 max minions\n50% increased damage\n25% critical strike chance and 10% increased summon damage\n+80 max mana\n'Baby Weaver loves you!'");
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true; // Draw all hair as normal. Used by Mime Mask, Sunglasses

        }

        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = Item.sellPrice(platinum: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Cyan; // The rarity of the item
            Item.defense = 10;
        }

        public override void UpdateEquip(Player player)
        {
            player.buffImmune[BuffID.Electrified] = true; // Make the player immune to Fire
            player.maxMinions += 3;
            player.GetDamage(DamageClass.Generic) += 0.5f;
            player.GetDamage(DamageClass.Summon) += 0.1f;
            player.GetCritChance(DamageClass.Generic) += 25f;
            player.statManaMax2 += 80;
            player.GetModPlayer<FamiliarPlayer>().mandibleHat = true;
        }
        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<FamiliarPlayer>().mandibleHat = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.GetModPlayer<FamiliarPlayer>().stormSet = true;
            player.setBonus = "Increases dash length and dashes summon lighting\nCauses nearby enemies to be battered by a cosmic tempest.\n+100 max stealth while holding a Rogue weapon";
            player.Calamity().wearingRogueArmor= true;
            if ((player.HeldItem.DamageType == ModContent.GetModItem(ModContent.ItemType<Boomible>()).Item.DamageType || player.Calamity().rogueStealthMax > 0))
            player.Calamity().rogueStealthMax += 1;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            if (body.type == ModContent.ItemType<WovenSweater>())
            {
                return legs.type == ModContent.ItemType<ProstheticTail>();
            }
            return false;
        }
    }

    public class MandibleDrawFront : PlayerDrawLayer
    {
        private Asset<Texture2D> texture;

        // Returning true in this property makes this layer appear on the minimap player head icon.
        public override bool IsHeadLayer => true;

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            // The layer will be visible only if the player is holding an ExampleItem in their hands. Or if another modder forces this layer to be visible.
            return drawInfo.drawPlayer.head == ModContent.GetModItem(ModContent.ItemType<MandibleReplica>()).Item.headSlot;

            // If you'd like to reference another PlayerDrawLayer's visibility,
            // you can do so by getting its instance via ModContent.GetInstance<OtherDrawLayer>(), and calling GetDefaultVisiblity on it
        }

        // This layer will be a 'child' of the head layer, and draw before (beneath) it.
        // If the Head layer is hidden, this layer will also be hidden.
        // If the Head layer is moved, this layer will move with it.
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
        // If you want to make a layer which isn't a child of another layer, use `new Between(Layer1, Layer2)` to specify the position.
        // If you want to make a 'mobile' layer which can render in different locations depending on the drawInfo, use a `Multiple` position.

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            // The following code draws ExampleItem's texture behind the player's head.

            if (texture == null)
            {
                texture = ModContent.Request<Texture2D>("stormWeaverStaff/Items/MandibleReplica_Front");
            }
            int b = 0;
            List<int> ints= new List<int>()
            {
                1, 2, 3, 8, 9, 10
            };
            if (ints.Contains(drawInfo.hairBackFrame.Top/56))
            {
                b = 2;
            }
            var position = drawInfo.Center + new Vector2(-9f * drawInfo.drawPlayer.direction, -19f - b) - Main.screenPosition;
            position = new Vector2((int)position.X, (int)position.Y); // You'll sometimes want to do this, to avoid quivering.
            float colorvalue = 0;
            if (drawInfo.drawPlayer.Calamity().rogueStealth > 0f && drawInfo.drawPlayer.Calamity().rogueStealthMax > 0f && drawInfo.drawPlayer.townNPCs < 3f && CalamityConfig.Instance.StealthInvisibility)
            {
                colorvalue = drawInfo.drawPlayer.Calamity().rogueStealth / drawInfo.drawPlayer.Calamity().rogueStealthMax * 0.9f;
            }
            // Queues a drawing of a sprite. Do not use SpriteBatch in drawlayers!
            drawInfo.DrawDataCache.Add(new DrawData(
                texture.Value, // The texture to render.
                position, // Position to render at.
                null, // Source rectangle.
                new Color(1f - colorvalue * 0.89f, 1f - colorvalue, 1f - colorvalue * 0.89f, 1f - colorvalue) * drawInfo.drawPlayer.stealth, // Color.
                0f, // Rotation.
                texture.Size() * 0.5f, // Origin. Uses the texture's center.
                1f, // Scale.
                drawInfo.drawPlayer.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, // SpriteEffects.
                0 // 'Layer'. This is always 0 in Terraria.
            ));
        }
    }


    public class MandibleDrawBack : PlayerDrawLayer
    {
        private Asset<Texture2D> texture;

        // Returning true in this property makes this layer appear on the minimap player head icon.
        public override bool IsHeadLayer => true;

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            // The layer will be visible only if the player is holding an ExampleItem in their hands. Or if another modder forces this layer to be visible.
            return drawInfo.drawPlayer.head == ModContent.GetModItem(ModContent.ItemType<MandibleReplica>()).Item.headSlot;

            // If you'd like to reference another PlayerDrawLayer's visibility,
            // you can do so by getting its instance via ModContent.GetInstance<OtherDrawLayer>(), and calling GetDefaultVisiblity on it
        }

        // This layer will be a 'child' of the head layer, and draw before (beneath) it.
        // If the Head layer is hidden, this layer will also be hidden.
        // If the Head layer is moved, this layer will move with it.
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Head);
        // If you want to make a layer which isn't a child of another layer, use `new Between(Layer1, Layer2)` to specify the position.
        // If you want to make a 'mobile' layer which can render in different locations depending on the drawInfo, use a `Multiple` position.

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            // The following code draws ExampleItem's texture behind the player's head.

            if (texture == null)
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
            float colorvalue = 0;
            if (drawInfo.drawPlayer.Calamity().rogueStealth > 0f && drawInfo.drawPlayer.Calamity().rogueStealthMax > 0f && drawInfo.drawPlayer.townNPCs < 3f && CalamityConfig.Instance.StealthInvisibility)
            {
                colorvalue = drawInfo.drawPlayer.Calamity().rogueStealth / drawInfo.drawPlayer.Calamity().rogueStealthMax * 0.9f;
            }
            // Queues a drawing of a sprite. Do not use SpriteBatch in drawlayers!
            drawInfo.DrawDataCache.Add(new DrawData(
                texture.Value, // The texture to render.
                position, // Position to render at.
                null, // Source rectangle.
                new Color(1f- colorvalue*0.89f,1f - colorvalue, 1f - colorvalue * 0.89f, 1f - colorvalue) * drawInfo.drawPlayer.stealth, // Color.
                0f, // Rotation.
                texture.Size() * 0.5f, // Origin. Uses the texture's center.
                1f, // Scale.
                drawInfo.drawPlayer.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, // SpriteEffects.
                0 // 'Layer'. This is always 0 in Terraria.
            ));
        }
    }


    public class CosmicShock : ModBuff
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Cosmic Shock");
            base.Description.SetDefault("Tossed and shocked by a cosmic tempest.");
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            BuffID.Sets.LongerExpertDebuff[base.Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            //player.Calamity().gsInferno = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.GetGlobalNPC<debuffNPC>().cosmicShock < npc.buffTime[buffIndex])
            {
                npc.GetGlobalNPC<debuffNPC>().cosmicShock = npc.buffTime[buffIndex];
            }
            npc.DelBuff(buffIndex);
            buffIndex--;
        }
    }

    public class debuffNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int cosmicShock = 0;
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (cosmicShock > 0)
            {
                bool wormBoss = CalamityLists.DesertScourgeIDs.Contains(npc.type) || CalamityLists.EaterofWorldsIDs.Contains(npc.type) || CalamityLists.PerforatorIDs.Contains(npc.type) || CalamityLists.AquaticScourgeIDs.Contains(npc.type) || CalamityLists.AstrumDeusIDs.Contains(npc.type) || CalamityLists.StormWeaverIDs.Contains(npc.type);
                bool increasedElectricityDamage = npc.wet || npc.honeyWet || npc.lavaWet || npc.dripping;
                double electricityDamageMult = ((!increasedElectricityDamage) ? 1.0 : (wormBoss ? 1.5 : 2.0));
                if (npc.GetGlobalNPC<CalamityGlobalNPC>().IncreasedElectricityEffects_Transformer)
                {
                    electricityDamageMult += 0.5;
                }
                if (npc.GetGlobalNPC<CalamityGlobalNPC>().VulnerableToElectricity.HasValue)
                {
                    electricityDamageMult = ((!npc.GetGlobalNPC<CalamityGlobalNPC>().VulnerableToElectricity.Value) ? (electricityDamageMult * (increasedElectricityDamage ? 0.2 : 0.5)) : (electricityDamageMult * ((!increasedElectricityDamage) ? (wormBoss ? 1.5 : 2.0) : (wormBoss ? 1.25 : 1.5))));
                }
                int dot = (int)(250.0 * electricityDamageMult);
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= dot;
                if (damage < dot / 5)
                {
                    damage = dot / 5;
                }
            }
        }

            public override void PostAI(NPC npc)
        {
            if (cosmicShock > 0)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric, Scale: 0.5f);
                cosmicShock--;
            }
        }
    }
}