
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
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Microsoft.Xna.Framework.Graphics;

namespace stormWeaverStaff.Items
{
    public class Boomible : RogueWeapon
    {
        public override void SetStaticDefaults()
        {
            base.SacrificeTotal = 1;
            base.DisplayName.SetDefault("Boomible");
            base.Tooltip.SetDefault("A gift from the Weaver");
        }

        public override void SetDefaults()
        {
            CalamityGlobalItem calamityGlobalItem = base.Item.Calamity();
            base.Item.damage = 250;
            base.Item.DamageType = ModContent.GetModItem(ModContent.ItemType<TrackingDisk>()).Item.DamageType;
            base.Item.width = 31;
            base.Item.height = 34;
            base.Item.useTime = 20;
            base.Item.useAnimation = Item.useTime;
            base.Item.useStyle = 1;
            base.Item.useTurn = false;
            base.Item.knockBack = 3f;
            base.Item.value = CalamityGlobalItem.Rarity3BuyPrice;
            base.Item.rare = ItemRarityID.Red;
            base.Item.noUseGraphic = true;
            base.Item.UseSound = SoundID.Item1;
            base.Item.autoReuse = true;
            base.Item.shoot = ModContent.ProjectileType<BoomibleProjectile>();
            base.Item.shootSpeed = 40f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (proj.WithinBounds(1000))
            {
                Main.projectile[proj].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
            }
            return false;
        }

        public override void AddRecipes()
        {
            base.CreateRecipe().AddIngredient<ArmoredShell>(7)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class BoomibleProjectile : ModProjectile
    {
        public override string Texture => "stormWeaverStaff/Items/Boomible";

        public List<Vector2> laserDraw = new List<Vector2>();
        public override void SetDefaults()
        {
            Projectile.width = 31;
            Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 0;
            Projectile.DamageType = ModContent.GetModItem(ModContent.ItemType<Boomible>()).Item.DamageType;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity= true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.tileCollide = false;
        }

        private int timer = 0;
        private Vector2 angle = Vector2.Zero;
        public override void OnSpawn(IEntitySource source)
        {
            var player = Main.player[Projectile.owner];
            angle = player.DirectionTo(Main.MouseWorld);
            var list = new List<Projectile>();
            foreach (var proj in Main.projectile)
            {
                if (proj.type == Projectile.type && proj.whoAmI != Projectile.whoAmI && Projectile.owner == Projectile.owner && proj.active && proj.Calamity().stealthStrike)
                    list.Add(proj);
            }
            if (list.Count >= 3)
            {
                Projectile life = Projectile;
                foreach (var proj in list)
                {
                    if (proj.timeLeft < life.timeLeft)
                    {
                        life = proj;
                    }
                }
                life.Kill();
            }
        }
        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            if (!Projectile.Calamity().stealthStrike)
            {
                if (timer > 7)
                {
                    Projectile.velocity = (Projectile.velocity * 31 + Projectile.DirectionTo(player.Center) * 45) / 32f;
                }
            }
            else
            {
                if (timer > 7)
                {
                    Projectile.velocity *= 0.9f;
                }
                if (timer % 3 == 0) {
                    List<Vector2> otherBoom = new List<Vector2>();
                    foreach (var proj in Main.projectile)
                    {
                        if (proj.type == Projectile.type && proj.whoAmI != Projectile.whoAmI && Projectile.owner == Projectile.owner && proj.active && proj.Calamity().stealthStrike && Projectile.Distance(proj.Center)< 5000)
                        otherBoom.Add(proj.Center);
                    }
                    if (Projectile.Distance(player.Center) < 5000) {
                        otherBoom.Add(player.Center + player.velocity);
                    }
                    foreach (var location in otherBoom)
                    {
                            var velo = 15f;
                            var newProj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.Center.DirectionTo(location)* velo, ModContent.ProjectileType<BoomibleLaser>(), (int)CalamityUtils.CalcDamage<RogueDamageClass>(player,135f), Projectile.knockBack, Projectile.owner,Projectile.whoAmI);
                            Main.projectile[newProj].timeLeft = (int)(Projectile.Distance(location)/ velo);
                    }
                    }

            }
            if (timer > 30 && Projectile.Center.Distance(player.Center) < 16)
            {
                Projectile.Kill();
            }
            Projectile.rotation += MathHelper.Pi/180*30;
            timer++;
            if (Projectile.timeLeft == 1)
            {
                for (var i = 0; i < 20; i++)
                {
                    Dust.NewDust(Projectile.Center, 0, 0, DustID.Electric, new Vector2(1, 1).RotatedBy(i).X, new Vector2(1, 1).RotatedBy(i).Y);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            /*var tex = ModContent.Request<Texture2D>("stormWeaverStaff/Items/BoomibleLaser").Value;
            foreach (var pos in laserDraw)
            {
                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, null, Color.LightBlue, 0, tex.Size() / 2f, 1, SpriteEffects.None, 0);
            }
            laserDraw.Clear();*/
            return true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<CosmicShock>(), 60);
        }
    }

    public class BoomibleLaser : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 999;
            Projectile.DamageType = ModContent.GetModItem(ModContent.ItemType<Boomible>()).Item.DamageType;
            Projectile.timeLeft = 600;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[1] != 0)
            {
                Projectile.damage = 0;
            }
        }
        public override void AI()
        {
            /*if (Main.projectile[(int)Projectile.ai[0]].type == ModContent.ProjectileType<BoomibleProjectile>())
            {
                Main.projectile[(int)Projectile.ai[0]].ModProjectile<BoomibleProjectile>().laserDraw.Add(Projectile.Center);
            }*/
            //Main.player[Projectile.owner].GetModPlayer<FamiliarPlayer>().laserDraw.Add(Projectile.Center);
            Dust.NewDustPerfect(Projectile.Center, DustID.Electric,Scale: 0.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Electrified, 60);
        }
    }
}