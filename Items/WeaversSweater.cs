
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

namespace stormWeaverStaff.Items
{
    [AutoloadEquip(EquipType.Body)]
    public class WeaversSweater : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.SacrificeTotal = 1;
            base.DisplayName.SetDefault("Weaver's Sweater");
            base.Tooltip.SetDefault("Seems very cozy!");
        }

        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = Item.sellPrice(platinum: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Cyan; // The rarity of the item
        }

        public override void UpdateEquip(Player player)
        {
            player.buffImmune[BuffID.Electrified] = true; // Make the player immune to Fire
            player.statDefense += 150;
            player.statLifeMax2 += (Main.expertMode ? 1320800 : Main.masterMode ? 1981200 : 825500) - player.statLifeMax;
            player.maxMinions += 10;
            player.GetDamage(DamageClass.Generic) += 0.25f;
            player.endurance += 0.9999f;
            player.noKnockback = true;
            player.noFallDmg = true;
        }
    }
}