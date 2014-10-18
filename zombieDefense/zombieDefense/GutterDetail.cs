using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace zombieDefense
{
    public abstract class GutterDetail
    {
        public abstract int Available { get; }
        public abstract void Played(GameTime gameTime);
        public virtual int MaxConcurrent { get { return 1; } }
        public virtual int CostToUse { get { return 0; } }
        public virtual void Update(GameTime gameTime)
        {
        }
    }
}
