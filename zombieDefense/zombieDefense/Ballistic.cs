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
using Microsoft.Xna.Framework.Graphics;

namespace zombieDefense
{
    /// <summary>
    /// Ballistic class used to link source and target entities
    /// </summary>
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
