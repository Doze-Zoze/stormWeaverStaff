using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Sounds;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using rail;
using ReLogic.Content;
using Steamworks;
using stormWeaverStaff.Projectiles;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Security.Permissions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace stormWeaverStaff.Items
{
    public enum FamiliarType
    {
        Naked,
        Armor,
        Auric,
        Scoria,
        DoG,
        Exo,
        Thana
    }
    public class FamiliarSpawnItem : ModItem
    {
        public override string Texture => "stormWeaverStaff/Items/Familiar/FamiliarHead";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cosmic Vortex");
            Tooltip.SetDefault("Calls a baby Storm Weaver to follow you\nProvides various buffs");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.rare = 7;
        }
        public override void UpdateEquip(Player player)
        {
            var modplayer = player.GetModPlayer<FamiliarPlayer>();
            modplayer.familiar = true;
            Item.SetNameOverride($"Cosmic Vortex\n{Math.Round(modplayer.familiarFood / (60 * 60f) * 10f, 1)}% full");
            player.statDefense += (int)MathHelper.SmoothStep(0, 50, modplayer.familiarFood / (60 * 60 * 10f));
            player.lifeRegen += (int)MathHelper.SmoothStep(0, 20, modplayer.familiarFood / (60 * 60 * 10f));
        }

        public override void UpdateVanity(Player player)
        {
            var modplayer = player.GetModPlayer<FamiliarPlayer>();
            modplayer.familiar = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
        }
        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(Item.type);
            recipe.AddIngredient(ItemID.Worm);
            recipe.AddIngredient(ItemID.FallenStar, 3);
            recipe.AddIngredient(ItemID.RainCloud, 10);
            recipe.Register();

        }

    }

    public class FamiliarPlayer : ModPlayer
    {
        public bool familiar = false;
        public int familiarFood;
        public FamiliarType familiarType;
        public List<Vector2> laserDraw = new List<Vector2>();
        public bool mandibleHat = false;
        public bool prosTail = false;
        public List<Tuple<Vector2, float>> tailPos;
        private int oldDash = 0;
        public bool stormSet = false;
        public override void ResetEffects()
        {
            familiar = false;
            foreach (var armor in Player.armor)
            {
                if (armor.type == ModContent.ItemType<FamiliarSpawnItem>())
                {
                    familiar = true;
                }
            }
            mandibleHat = false;
            stormSet = false;
            if (!(Player.armor[12].type == ModContent.ItemType<ProstheticTail>())) prosTail = false;
        }

        public override void Initialize()
        {
            familiarFood = 0;
            familiarType = FamiliarType.Naked;
        }
        public override void SaveData(TagCompound tag)
        {
            tag["WeaverFamiliarFood"] = familiarFood;
            tag["WeaverFamiliarType"] = (int)familiarType;
        }

        public override void LoadData(TagCompound tag)
        {
            familiarFood = tag.Get<int>("WeaverFamiliarFood");
            familiarType = (FamiliarType)tag.Get<int>("WeaverFamiliarType");
        }
        public override void PreUpdate()
        {
            if (Player.armor[12].type == ModContent.ItemType<ProstheticTail>()) prosTail = true;
            if (stormSet)
            {
                if (oldDash == 0 && Player.dashDelay == -1)
                {
                    Projectile.NewProjectile(Player.GetSource_FromAI(), Player.Center, Player.velocity * 0.5f, ModContent.ProjectileType<VoidVortexProj>(), 500, 0, Player.whoAmI);
                    Player.velocity.X *= 1.25f;
                    SoundEngine.PlaySound(in CommonCalamitySounds.LightningSound, Player.Center);
                }
                int buffType = ModContent.BuffType<CosmicShock>();
                float range = 1500f;
                bool dealDamage = Player.miscCounter % 30 == 0;
                for (int i = 0; i < 200; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage  || npc.damage <= 0 || !(Vector2.Distance(Player.Center, npc.Center) <= range))
                    {
                        continue;
                    }
                    if (npc.FindBuffIndex(buffType) == -1)
                    {
                        npc.AddBuff(buffType, 120);
                    }
                    if (dealDamage)
                    {
                        Projectile aura = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), 240, 0f, Player.whoAmI, i);
                        if (aura.whoAmI.WithinBounds(1000))
                        {
                            aura.DamageType = DamageClass.Generic;
                        }
                    }
                }
            }
            
            oldDash = Player.dashDelay;
            if (familiar)
            {
                var check = false;
                foreach (var projectile in Main.projectile)
                {

                    if ((projectile.type == ModContent.ProjectileType<FamiliarBody>() || projectile.type == ModContent.ProjectileType<FamiliarHead>() || projectile.type == ModContent.ProjectileType<FamiliarTail>()) && projectile.owner == Player.whoAmI && projectile.active)
                    {
                        check = true; break;
                    }
                }
                if (!check)
                {
                    Projectile.NewProjectile(Player.GetSource_Accessory(new Item(ModContent.ItemType<FamiliarSpawnItem>())), Player.Center, -Vector2.UnitY, ModContent.ProjectileType<FamiliarHead>(), 0, 0f, Player.whoAmI);
                    Projectile.NewProjectile(Player.GetSource_Accessory(new Item(ModContent.ItemType<FamiliarSpawnItem>())), Player.Center, Vector2.Zero, ModContent.ProjectileType<FamiliarTail>(), 0, 0f, Player.whoAmI);
                    for (var i = 0; i < 3; i++)
                    {
                        Projectile.NewProjectile(Player.GetSource_Accessory(new Item(ModContent.ItemType<FamiliarSpawnItem>())), Player.Center, Vector2.Zero, ModContent.ProjectileType<FamiliarBody>(), 0, 0f, Player.whoAmI);
                    }
                }
            }
            else
            {
                //familiarFood = 0;
            }
            if (familiarFood > 60 * 60 * 10)
            {
                familiarFood = 60 * 60 * 10;
            }
            if (Player.miscCounter % 1 == 0)
            {
                if (familiarFood > 0)
                {
                    familiarFood -= 1;
                }
                else
                {
                    familiarFood = 0;
                }
            }

        }

        public override void PostUpdate()
        {
            if (prosTail)
            {
                if (tailPos == null)
                {
                    tailPos = new List<Tuple<Vector2, float>>();
                    for (var i = 0; i < 3;i++)
                    {

                        tailPos.Add(new Tuple<Vector2, float>(Player.Center, 0));
                    }
                }
                for (var i = 0; i < tailPos.Count; i++)
                {
                    Vector2 nextCent = Vector2.Zero;
                    float nextRot = 0;
                    if (i == 0)
                    {
                        nextCent = Player.Center+new Vector2(0,6);
                        nextRot = new Vector2(Player.direction, -0.325f).ToRotation();
                    }
                    else
                    {
                        nextCent = tailPos[i - 1].Item1;
                        nextRot = tailPos[i - 1].Item2;
                    }

                    Vector2 destinationOffset = nextCent - tailPos[i].Item1;
                    if (nextRot != tailPos[i].Item2)
                    {
                        float angle = MathHelper.WrapAngle(nextRot - tailPos[i].Item2);
                        //destinationOffset = Projectile.Center.DirectionTo(nextSegment.Center);
                        destinationOffset = destinationOffset.RotatedBy(angle * 0.1f);
                    }
                    var rotation = destinationOffset.ToRotation();

                    var center = nextCent - destinationOffset.SafeNormalize(Vector2.Zero) * (12*0.75f);
                    if (i == tailPos.Count-1)
                    {
                        center = nextCent - destinationOffset.SafeNormalize(Vector2.Zero) * (22*0.75f);
                    }
                    tailPos[i] = new Tuple<Vector2, float>(center, rotation);

                    /*Vector2 destinationOffset = Player.position - tailPos[i].Item1;
                    if (nextRot != tailPos[i].Item2)
                    {
                        float angle = MathHelper.WrapAngle(nextRot - tailPos[i].Item2);
                        destinationOffset = destinationOffset.RotatedBy(angle * 0.1f);
                    }
                    tailPos[i] = new Tuple<Vector2, float>(destinationOffset != Vector2.Zero ? tailPos[i].Item1 : Player.position + destinationOffset.SafeNormalize(Vector2.One) * 12, destinationOffset.ToRotation());*/
                }
            }
            else
            {
                tailPos = null;
            }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            /*var tex1 = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarBody").Value;
            var tex2 = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarTail").Value;
            var pos = Player.Center - Main.screenPosition;
            if (tailPos != null) { 
                for (var i = tailPos.Count - 1; i >= 0; i--)
                {
                    pos += tailPos[i].Item1;
                    if (i == tailPos.Count-1)
                    {
                        Main.spriteBatch.Draw(tex2, tailPos[i].Item1 - Main.screenPosition, null, new Color(r, g, b, a), tailPos[i].Item2+(float)Math.PI, tex2.Size() / 2f, 0.75f, SpriteEffects.None, 0);
                    }
                    else
                    {
                        Main.spriteBatch.Draw(tex1, tailPos[i].Item1 - Main.screenPosition, null, new Color(r, g, b, a), tailPos[i].Item2 + (float)Math.PI, tex1.Size() / 2f, 0.75f, SpriteEffects.None, 0);
                    }
                }
            }*/
            if (Player.legs == ModContent.GetModItem(ModContent.ItemType<ProstheticTail>()).Item.legSlot) { 
                Player.legs = 0;
            }
        }
    }
        public class FamiliarHead : ModProjectile
        {
            public override string Texture => "stormWeaverStaff/Items/Familiar/FamiliarHead";
            public int timer = 0;
            private int cooldown = 0;
            private bool? oldMale = null;
            Dictionary<int, Projectile> segments = new Dictionary<int, Projectile>();

            public Dictionary<FamiliarType, Tuple<string, int, string, Color>> armorDict = new Dictionary<FamiliarType, Tuple<string, int, string, Color>>()
        {
            { FamiliarType.Naked, new Tuple<string,int,string,Color>("Naked", ItemID.Worm, "Naked!", new Color(220,102,254)) },
            { FamiliarType.Armor, new Tuple<string,int,string,Color>("", ModContent.ItemType<ArmoredShell>(), "Storm Armor!", new Color(118,110,151)) },
            { FamiliarType.Auric, new Tuple<string,int,string,Color>("Auric", ModContent.ItemType<AuricBar>(), "Auric Armor!", new Color(231,166,79)) },
            { FamiliarType.Scoria, new Tuple<string,int,string,Color>("Scoria", ModContent.ItemType<ScoriaBar>(), "Hydrothermic Armor!", new Color(224,108,29)) },
            { FamiliarType.DoG, new Tuple<string,int,string,Color>("Dog", ModContent.ItemType<CosmiliteBar>(), "God Slayer Armor!", new Color(153,84,176)) },
            { FamiliarType.Exo, new Tuple<string,int,string,Color>("Exo", ModContent.ItemType<MiracleMatter>(), "Miracle Armor!", new Color(135,218,237)) },
            { FamiliarType.Thana, new Tuple<string,int,string,Color>("Thanatos", ModContent.ItemType<ExoPrism>(), "Thanatos Armor!", new Color(80,89,103)) },
        };
            public override void SetStaticDefaults()
            {

                ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
                ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;

                ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
                ProjectileID.Sets.TrailCacheLength[Projectile.type] = 100;
            }

            public override void SetDefaults()
            {
                Projectile.width = 28;
                Projectile.height = 28;
                Projectile.friendly = true;
                Projectile.penetrate = -1;
                Projectile.timeLeft = 180;
                Projectile.localNPCHitCooldown = 30;
                Projectile.usesLocalNPCImmunity = true;
                Projectile.tileCollide = false;
                Projectile.extraUpdates = 0;
                Projectile.aiStyle = -1;
                Projectile.DamageType = DamageClass.Summon;
                Projectile.netImportant = true;

            }
            public override void AI()
            {

                //Main.NewText("head " + Projectile.owner);
                Player player = Main.player[Projectile.owner];
                var modplayer = player.GetModPlayer<FamiliarPlayer>();
                if (modplayer.familiar)
                {
                    Projectile.timeLeft = 10;
                }
                else
                {
                    Projectile.Kill();
                    return;
                }
                timer++;
                segments.Clear();
                if (!player.Male && Projectile.Distance(player.Center) < 16 * 10 && Main.rand.NextBool(5000))
                {

                    CombatText.NewText(Projectile.Hitbox, Color.HotPink, "b00bs! :D", true);
                }
                else
                {
                    if (oldMale == true && !player.Male)
                    {
                        CombatText.NewText(Projectile.Hitbox, Color.HotPink, "ooh! b00bs! :D", true);
                    }
                    if (oldMale == false && player.Male)
                    {
                        CombatText.NewText(Projectile.Hitbox, Color.HotPink, "bring back the b00bs... :(", true);
                    }
                }
                oldMale = player.Male;
                foreach (var projectile in Main.projectile)
                {

                    if (projectile.type == ModContent.ProjectileType<FamiliarBody>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        if (projectile.ModProjectile<FamiliarBody>().segmentIndex != -1)
                        {
                            segments.Add(projectile.ModProjectile<FamiliarBody>().segmentIndex, projectile);
                        }
                    }
                    if (projectile.type == ModContent.ProjectileType<FamiliarTail>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        segments.Add(projectile.ModProjectile<FamiliarTail>().segmentIndex, projectile);
                    }
                }
                Item goal = null;
                if (cooldown == 0)
                {
                    foreach (Item item in Main.item)
                    {
                        if ((item.type == ModContent.ItemType<BlasphemousDonut>() || (((item.type == ItemID.Silk && item.stack >= 10) || (item.type == ModContent.ItemType<Rock>()) || (item.type == ModContent.ItemType<StormlionMandible>() && item.stack >= 1) || (item.type == ItemID.MechanicalWorm)) && (modplayer.familiarType == FamiliarType.Armor || modplayer.familiarType == FamiliarType.Auric || modplayer.familiarType == FamiliarType.Exo || modplayer.familiarType == FamiliarType.Thana || modplayer.familiarType == FamiliarType.DoG)) || UpgradeNear(item)) && Projectile.Distance(item.Center) < 600 && item.active && item.timeSinceItemSpawned > 180)
                        {
                            if (goal == null)
                            {
                                goal = item;
                            }
                            else
                            {
                                if (Projectile.Distance(goal.Center) > Projectile.Distance(item.Center))
                                {
                                    goal = item;
                                }
                            }
                        }
                    }
                }
                
                if (goal != null)
                {
                    Projectile.velocity = (Projectile.velocity * 10 + Projectile.DirectionTo(goal.Center) * 10) / 11;
                    Projectile.rotation = Projectile.velocity.ToRotation();
                    if (Projectile.Distance(goal.Center) < 16)
                    {
                        if (goal.type == ModContent.ItemType<BlasphemousDonut>())
                        {
                            goal.active = false;
                            SoundEngine.PlaySound(SoundID.Item2);
                            CombatText.NewText(Projectile.Hitbox, Color.Orange, "Nom!");
                            modplayer.familiarFood += 30 * 60 * goal.stack;
                        }
                        if (goal.type == ItemID.Silk)
                        {
                            if (goal.stack > 10)
                            {
                                goal.stack -= 10;
                                goal.timeSinceItemSpawned = 60;
                            }
                            else goal.active = false;
                            Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ModContent.ItemType<WovenSweater>());
                            //SoundEngine.PlaySound(SoundID.Item2);
                            CombatText.NewText(Projectile.Hitbox, Color.HotPink, "Surprise!");
                            //modplayer.familiarFood += 30 * 60 * goal.stack;
                        }
                    if (goal.type == ModContent.ItemType<StormlionMandible>())
                    {
                        if (goal.stack > 2)
                        {
                            goal.stack -= 2;
                            goal.timeSinceItemSpawned = 60;
                        }
                        else goal.active = false;
                        Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ModContent.ItemType<MandibleReplica>());
                        //SoundEngine.PlaySound(SoundID.Item2);
                        CombatText.NewText(Projectile.Hitbox, Color.HotPink, "Surprise!");
                        //modplayer.familiarFood += 30 * 60 * goal.stack;
                    }
                    if (goal.type == ItemID.MechanicalWorm)
                    {
                        if (goal.stack > 1)
                        {
                            goal.stack -= 1;
                            goal.timeSinceItemSpawned = 60;
                        }
                        else goal.active = false;
                        Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ModContent.ItemType<ProstheticTail>());
                        //SoundEngine.PlaySound(SoundID.Item2);
                        CombatText.NewText(Projectile.Hitbox, Color.HotPink, "Surprise!");
                        //modplayer.familiarFood += 30 * 60 * goal.stack;
                    }
                    if (goal.type == ModContent.ItemType<Rock>())
                    {
                        if (goal.stack > 1)
                        {
                            goal.stack -= 1;
                            goal.timeSinceItemSpawned = 60;
                        }
                        else goal.active = false;
                        Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ModContent.ItemType<WeaversSweater>());
                        //SoundEngine.PlaySound(SoundID.Item2);
                        CombatText.NewText(Projectile.Hitbox, Color.HotPink, "Surprise!");
                        //modplayer.familiarFood += 30 * 60 * goal.stack;
                    }
                    useUpgrade(goal);
                    }
                }
                else
                {
                    BaseAI();
                }
                for (var i = 1; i <= segments.Count; i++)
                {
                    if (i < segments.Count)
                    {
                        segments[i].ModProjectile<FamiliarBody>().SegmentMove();
                    }
                    else
                    {
                        segments[i].ModProjectile<FamiliarTail>().SegmentMove();
                    }
                }
                if (cooldown > 0)
                {
                    cooldown--;
                }
            }

            private bool UpgradeNear(Item item)
            {

                Player player = Main.player[Projectile.owner];
                var modplayer = player.GetModPlayer<FamiliarPlayer>();
                foreach (var kvp in armorDict)
                {
                    if (kvp.Value.Item2 == item.type && kvp.Key != modplayer.familiarType)
                    {
                        return true;
                    }
                }
                return false;
            }

            private void useUpgrade(Item item)
            {
                Player player = Main.player[Projectile.owner];
                var modplayer = player.GetModPlayer<FamiliarPlayer>();
                foreach (var kvp in armorDict)
                {
                    if (item.type == kvp.Value.Item2)
                    {
                        if (item.stack <= 1)
                        {
                            item.active = false;
                        }
                        else
                        {
                            item.stack--;
                        }
                        SoundEngine.PlaySound(SoundID.Item2);
                        CombatText.NewText(Projectile.Hitbox, kvp.Value.Item4, kvp.Value.Item3, true);
                        Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, armorDict[modplayer.familiarType].Item2);
                        modplayer.familiarType = kvp.Key;
                        cooldown = 30;
                        return;

                    }
                }

            }
            private void BaseAI()
            {
                Player player = Main.player[Projectile.owner];
                Vector2 targetPos = player.Center + new Vector2(16 * 7, 0).RotatedBy(MathHelper.ToRadians(timer * 4f));
                //Projectile.velocity = Projectile.DirectionTo(targetPos)*10f;
                var num = 1f;
                if (Projectile.Center.Distance(player.Center) > 16 * 25) num = Projectile.Center.Distance(player.Center) / (16f * 25) + 1;
                num *= (float)(1 + Math.Pow(player.GetModPlayer<FamiliarPlayer>().familiarFood / (60 * 60 * 10), 4) * 6);
                Projectile.velocity = (Projectile.DirectionTo(targetPos) * 5f * num * Projectile.Center.Distance(player.Center) / 100f + Projectile.velocity * 100) / (100f + Projectile.Center.Distance(player.Center) / 100f);//Projectile.velocity *= Projectile.Center.Distance(targetPos)/100f;

                if (Projectile.Center.Distance(player.Center) > 16 * 45) Projectile.velocity = Projectile.DirectionTo(player.Center) * Projectile.velocity.Length();
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            private void NakedAI()
            {
                Player player = Main.player[Projectile.owner];
                var target = Main.npc[player.MinionAttackTargetNPC];
                Vector2 targetPos = target.Center;

                if (timer % 60 == 0)
                {
                    Projectile.velocity = Projectile.DirectionTo(targetPos) * 40f;
                }
                if (Projectile.velocity.Length() > 20) Projectile.velocity *= 0.98f;
                TurnTowards(targetPos);
                Projectile.rotation = MathHelper.WrapAngle(Projectile.velocity.ToRotation());
            }
            private void TurnTowards(Vector2 pos)
            {
                var rotAmount = Math.Clamp(MathHelper.WrapAngle(Projectile.rotation - Projectile.DirectionTo(pos).ToRotation()), MathHelper.ToRadians(-10), MathHelper.ToRadians(10));
                Projectile.velocity = Projectile.velocity.RotatedBy(-rotAmount);
            }
            public override bool PreDraw(ref Color lightColor)
            {
                Player player = Main.player[Projectile.owner];
                var modplayer = player.GetModPlayer<FamiliarPlayer>();
                Texture2D tex = ModContent.Request<Texture2D>(Texture + armorDict[modplayer.familiarType].Item1, (AssetRequestMode)2).Value;
                Texture2D texBody = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarBody" + armorDict[modplayer.familiarType].Item1, (AssetRequestMode)2).Value;
                Texture2D texTail = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarTail" + armorDict[modplayer.familiarType].Item1, (AssetRequestMode)2).Value;
            Texture2D texG = ModContent.Request<Texture2D>(Texture + armorDict[modplayer.familiarType].Item1 + "Glow", (AssetRequestMode)2).Value;
            Texture2D texBodyG = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarBody" + armorDict[modplayer.familiarType].Item1 + "Glow", (AssetRequestMode)2).Value;
            Texture2D texTailG = ModContent.Request<Texture2D>("stormWeaverStaff/Items/Familiar/FamiliarTail" + armorDict[modplayer.familiarType].Item1 + "Glow", (AssetRequestMode)2).Value; 
            for (var i = segments.Count; i > 0; i--)
                {
                    if (i < segments.Count)
                    {
                        Main.EntitySpriteDraw(texBody, segments[i].Center - Main.screenPosition, null, segments[i].GetAlpha(Lighting.GetColor((segments[i].Center/16).ToPoint())), segments[i].rotation + MathHelper.Pi, texBody.Size() / 2f, segments[i].scale, SpriteEffects.None, 0);
                        Main.EntitySpriteDraw(texBodyG, segments[i].Center - Main.screenPosition, null, segments[i].GetAlpha(Color.White), segments[i].rotation + MathHelper.Pi, texBody.Size() / 2f, segments[i].scale, SpriteEffects.None, 0);
                }
                    else
                {
                        Main.EntitySpriteDraw(texTail, segments[i].Center - Main.screenPosition - new Vector2(10, 0).RotatedBy(segments[i].rotation), null, segments[i].GetAlpha(Lighting.GetColor((segments[i].Center / 16).ToPoint())), segments[i].rotation + MathHelper.Pi, texTail.Size() / 2f, segments[i].scale, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(texTailG, segments[i].Center - Main.screenPosition - new Vector2(10, 0).RotatedBy(segments[i].rotation), null, segments[i].GetAlpha(Color.White), segments[i].rotation + MathHelper.Pi, texTail.Size() / 2f, segments[i].scale, SpriteEffects.None, 0);

                }
                }
                Main.EntitySpriteDraw(tex, base.Projectile.Center - Main.screenPosition, null, base.Projectile.GetAlpha(lightColor), base.Projectile.rotation + MathHelper.Pi, tex.Size() / 2f, base.Projectile.scale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texG, base.Projectile.Center - Main.screenPosition, null, base.Projectile.GetAlpha(Color.White), base.Projectile.rotation + MathHelper.Pi, tex.Size() / 2f, base.Projectile.scale, SpriteEffects.None, 0);
            return false;

            }
        }

        public class FamiliarBody : ModProjectile
        {
            public override string Texture => "stormWeaverStaff/Items/Familiar/FamiliarBody";
            public bool armored = true;
            public int segmentIndex = -1;

            public override void SetStaticDefaults()
            {

                ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
                ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
                /*CalamityLists.pierceResistExceptionList.Add(Projectile.type);
                CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);*/
            }

            public override void SetDefaults()
            {
                Projectile.timeLeft = 10;
                Projectile.width = 26;
                Projectile.height = 26;
                Projectile.friendly = true;
                Projectile.penetrate = -1;
                Projectile.timeLeft = 180;
                Projectile.idStaticNPCHitCooldown = 4;
                Projectile.usesIDStaticNPCImmunity = true;
                Projectile.tileCollide = false;
                Projectile.extraUpdates = 0;
                Projectile.aiStyle = -1;
                Projectile.netImportant = true;

            }

            public override void OnSpawn(IEntitySource source)
            {
                foreach (var projectile in Main.projectile)
                {
                    if (projectile.type == ModContent.ProjectileType<FamiliarTail>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        segmentIndex = projectile.ModProjectile<FamiliarTail>().segmentIndex;
                        projectile.ModProjectile<FamiliarTail>().segmentIndex++;
                    }
                }
            }
            public override void AI()
            {
                if (segmentIndex == -1)
                {
                    foreach (var projectile in Main.projectile)
                    {
                        if (projectile.type == ModContent.ProjectileType<FamiliarTail>() && projectile.owner == Projectile.owner && projectile.active)
                        {
                            segmentIndex = projectile.ModProjectile<FamiliarTail>().segmentIndex;
                            projectile.ModProjectile<FamiliarTail>().segmentIndex++;
                        }
                    }
                }
                Player player = Main.player[Projectile.owner];
                var modplayer = player.GetModPlayer<FamiliarPlayer>();
                if (modplayer.familiar)
                {
                    Projectile.timeLeft = 10;
                }
                else
                {
                    Projectile.Kill();
                }
            }

            internal void SegmentMove()
            {
                Player player = Main.player[Projectile.owner];
                armored = !player.HasMinionAttackTargetNPC;
                var live = false;
                Projectile nextSegment = new Projectile();
                FamiliarHead head = new FamiliarHead();
                var headOffset = 0;
                for (int i = 0; i < 1000; i++)
                {
                    var projectile = Main.projectile[i];
                    if (projectile.type == ModContent.ProjectileType<FamiliarBody>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        if (projectile.ModProjectile<FamiliarBody>().segmentIndex == segmentIndex - 1)
                        {
                            live = true;
                            nextSegment = projectile;
                        }
                    }
                    if (projectile.type == ModContent.ProjectileType<FamiliarHead>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        if (segmentIndex == 1)
                        {
                            live = true;
                            nextSegment = projectile;
                            headOffset = 6;
                        }
                        head = projectile.ModProjectile<FamiliarHead>();
                    }
                }
                if (!live) Projectile.Kill();
                Vector2 destinationOffset = nextSegment.Center + nextSegment.velocity - Projectile.Center;
                if (nextSegment.rotation != Projectile.rotation)
                {
                    float angle = MathHelper.WrapAngle(nextSegment.rotation - Projectile.rotation);
                    //destinationOffset = Projectile.Center.DirectionTo(nextSegment.Center);
                    destinationOffset = destinationOffset.RotatedBy(angle * 0.1f);
                }
                Projectile.rotation = destinationOffset.ToRotation();
                if (destinationOffset != Vector2.Zero)
                {
                    Projectile.Center = nextSegment.Center + nextSegment.velocity - destinationOffset.SafeNormalize(Vector2.Zero) * (12 + headOffset);
                }
            }

            public override bool PreDraw(ref Color lightColor)
            {
                return false;
            }
            public override void Kill(int timeLeft)
            {
                if (Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<FamiliarHead>()] > 0)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        var projectile = Main.projectile[i];
                        if (projectile.type == ModContent.ProjectileType<FamiliarHead>() && projectile.owner == Projectile.owner && projectile.active)
                        {
                            projectile.Kill();
                        }
                    }
                }
            }
        } 
    

        public class FamiliarTail : ModProjectile
        {
            public override string Texture => "stormWeaverStaff/Items/Familiar/FamiliarTail";
            public bool armored = true;
            public int segmentIndex = 1;

            public override void SetStaticDefaults()
            {

                ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
                ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
                /*CalamityLists.pierceResistExceptionList.Add(Projectile.type);
                CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);*/
            }

            public override void SetDefaults()
            {
                Projectile.timeLeft = 10;
                Projectile.width = 26;
                Projectile.height = 26;
                Projectile.friendly = true;
                Projectile.penetrate = -1;
                Projectile.timeLeft = 180;
                Projectile.localNPCHitCooldown = 30;
                Projectile.usesLocalNPCImmunity = true;
                Projectile.tileCollide = false;
                Projectile.extraUpdates = 0;
                Projectile.aiStyle = -1;
                Projectile.netImportant = true;

            }
            public override void AI()
            {
                Player player = Main.player[Projectile.owner];
                var modplayer = player.GetModPlayer<FamiliarPlayer>();
                if (modplayer.familiar)
                {
                    Projectile.timeLeft = 10;
                }
                else
                {
                    Projectile.Kill();
                }
            }
            internal void SegmentMove()
            {
                Player player = Main.player[Projectile.owner];
                armored = !player.HasMinionAttackTargetNPC;
                var live = false;
                Projectile nextSegment = new Projectile();
                FamiliarHead head = new FamiliarHead();

                for (int i = 0; i < 1000; i++)
                {
                    var projectile = Main.projectile[i];
                    if (projectile.type == ModContent.ProjectileType<FamiliarBody>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        if (projectile.ModProjectile<FamiliarBody>().segmentIndex == segmentIndex - 1)
                        {
                            live = true;
                            nextSegment = projectile;
                        }
                    }
                    if (projectile.type == ModContent.ProjectileType<FamiliarHead>() && projectile.owner == Projectile.owner && projectile.active)
                    {
                        if (segmentIndex == 1)
                        {
                            live = true;
                            nextSegment = projectile;
                        }
                        head = projectile.ModProjectile<FamiliarHead>();
                    }
                }
                if (!live) Projectile.Kill();
                Vector2 destinationOffset = nextSegment.Center - Projectile.Center;
                if (nextSegment.rotation != Projectile.rotation)
                {
                    float angle = MathHelper.WrapAngle(nextSegment.rotation - Projectile.rotation);
                    //destinationOffset = Projectile.Center.DirectionTo(nextSegment.Center);
                    destinationOffset = destinationOffset.RotatedBy(angle * 0.1f);
                }
                Projectile.rotation = destinationOffset.ToRotation();
                if (destinationOffset != Vector2.Zero)
                {
                    Projectile.Center = nextSegment.Center - destinationOffset.SafeNormalize(Vector2.Zero) * 12;
                }
                Projectile.velocity = Vector2.Zero;
            }

            public override bool PreDraw(ref Color lightColor)
            {
                return false;
            }

            public override void Kill(int timeLeft)
            {
                if (Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<FamiliarHead>()] > 0)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        var projectile = Main.projectile[i];
                        if (projectile.type == ModContent.ProjectileType<FamiliarHead>() && projectile.owner == Projectile.owner && projectile.active)
                        {
                            projectile.Kill();
                        }
                    }
                }
            }
        }

    }