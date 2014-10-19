/*
# Zombie Defence - a zombie survival game
#
# Copyright (C) 2014 Robert Nijkamp
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful, 
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace zombieDefense
{
    /// <summary>
    /// Base class to manage gutter entity state.
    /// </summary>
    public abstract class GutterDetail
    {
        public abstract int Available { get; }
        public abstract void Played(GameTime gameTime);
        public virtual int MaxConcurrent { get { return 1; } }
        public virtual int CostToUse { get { return 0; } }
        public virtual void Update(GameTime gameTime) { }
    }
}
