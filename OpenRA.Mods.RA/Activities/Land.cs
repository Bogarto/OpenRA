#region Copyright & License Information
/*
 * Copyright 2007-2010 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see LICENSE.
 */
#endregion

using System;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.RA.Activities
{
	public class Land : IActivity
	{
		readonly float2 Pos;
		bool isCanceled;
		Actor Structure;
		
		public Land(float2 pos) { Pos = pos; }
		public Land(Actor structure) { Structure = structure; Pos = Structure.CenterLocation; }
		
		public IActivity NextActivity { get; set; }

		public IActivity Tick(Actor self)
		{
			if (Structure != null && Structure.IsDead())
			{
				Structure = null;
				isCanceled = true;
			}
			
			if (isCanceled) return NextActivity;

			var d = Pos - self.CenterLocation;
			if (d.LengthSquared < 50)		/* close enough */
				return NextActivity;

			var aircraft = self.traits.Get<Aircraft>();

			if (aircraft.Altitude > 0)
				--aircraft.Altitude;

			var desiredFacing = Util.GetFacing(d, aircraft.Facing);
			aircraft.Facing = Util.TickFacing(aircraft.Facing, desiredFacing, aircraft.ROT);
			var speed = .2f * aircraft.MovementSpeedForCell(self, self.Location);
			var angle = aircraft.Facing / 128f * Math.PI;

			self.CenterLocation += speed * -float2.FromAngle((float)angle);
			aircraft.Location = Util.CellContaining(self.CenterLocation);

			return this;
		}

		public void Cancel(Actor self) { isCanceled = true; NextActivity = null; }
	}
}
