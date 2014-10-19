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

namespace zombieDefense
{
    /// <summary>
    /// Game parameters
    /// </summary>
    public static class GameParams
    {
        /// <summary>
        /// The number of zomies that are in play when the game begins
        /// </summary>
        public static int INITIAL_ZOMBIE_COUNT = 10;

        /// <summary>
        /// Number of zombies that spawn during every spawn wave
        /// </summary>
        public static int ZOMBIE_SPAWN_NUMBER = 2;

        /// <summary>
        /// Number of seconds between spawn waves
        /// </summary>
        public static double ZOMBIE_SPAWN_INTERVAL = 2.5;

        /// <summary>
        /// Maximum number of zombies to allow to be in play at once
        /// </summary>
        public static int MAX_ZOMBIE_COUNT = 2000;

        /// <summary>
        /// Height of the gutter bar at the bottom of the screen
        /// </summary>
        public static int GUTTER_HEIGHT = 100;

        /// <summary>
        /// Length of time to draw the lazer line for when shooting
        /// </summary>
        public static int LAZER_FIRE_DURATION = 250;

        /// <summary>
        /// Max and initial hip points for the brain
        /// </summary>
        public static int GENERATOR_HP = 500;

        /// <summary>
        /// Number of bombs to drop during an air strike
        /// </summary>
        public static int AIRSTRIKE_BOMBS = 20;
    }
}
