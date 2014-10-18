using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    /// <summary>
    /// Mortar tower class. Manages sight distance, textures, and gutter detail.
    /// </summary>
    public class MortarTower : BaseTurret
    {
        public class MortarTowerGutterDetail : GutterDetail
        {
            private ZombieDefense game;
            private int numPlayed;

            public MortarTowerGutterDetail(ZombieDefense game)
            {
                this.game = game;
            }

            public override int Available { get { return 0; } }

            public override void Played(GameTime gameTime)
            {
                numPlayed++;
            }

            public override int MaxConcurrent { get { return 4; } }

            public override int CostToUse 
            {
                get { return (1000 * (numPlayed + 1)); }
            }
        }

        private static MortarTowerGutterDetail gutterDetail;

        public MortarTower(ZombieDefense game, bool inGutter)
            : base(game, 10, 10, "entity/mortar/base1", "entity/mortar/base2", "entity/mortar/barrel")
        {
            if (inGutter)
                gutterDetail = new MortarTowerGutterDetail(game);
        }

        public override GutterDetail GutterDetail { get { return gutterDetail; } }

        public override Entity Clone()
        {
            MortarTower t = new MortarTower(game, false);
            t.position = position;
            t.barrelAngle = barrelAngle;
            return t;
        }

        public override Ballistic CreateBallistic(ZombieDefense game, Entity from, Entity to)
        {
            return new MortarBallistic(game, from, to);
        }

        public override int MinSightDistance { get { return 200; } }
        public override int MaxSightDistance { get { return 300 + hp * 10; } }
        public override int NextShotInterval { get { return 7500; } }
        public override int NumShotDamage { get { return 1; } }

        public override void Damage(GameTime gameTime, int dmg, Entity dmgBy)
        {
            base.Damage(gameTime, dmg, null);
        }
    }
}
