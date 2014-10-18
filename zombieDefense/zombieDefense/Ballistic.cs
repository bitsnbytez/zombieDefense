using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    public abstract class Ballistic : Entity
    {
        private Entity from;
        private Entity to;

        public Ballistic(ZombieDefense game, Entity from, Entity to)
            : base(game, 1)
        {
            this.from = from;
            this.to = to;
        }

        public Entity Origin { get { return from; } }
        public Entity Target { get { return to; } }
    }
}
