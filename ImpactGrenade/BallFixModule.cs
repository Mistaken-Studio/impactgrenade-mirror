// -----------------------------------------------------------------------
// <copyright file="BallFixModule.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Interfaces;
using Exiled.Events.EventArgs;
using Grenades;
using Mistaken.API.Diagnostics;
using UnityEngine;

namespace Mistaken.ImpactGrenade
{
    /// <summary>
    /// Fix for ball that's not getting deleted/distabled.
    /// </summary>
    public class BallFixModule : Module
    {
        /// <inheritdoc cref="Module.Module(IPlugin{IConfig})"/>
        public BallFixModule(IPlugin<IConfig> plugin)
            : base(plugin)
        {
        }

        /// <inheritdoc/>
        public override string Name => "BallFix";

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => this.Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => this.Server_RoundStarted(), "RoundStart");
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => this.Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => this.Server_RoundStarted(), "RoundStart");
        }

        internal static HashSet<GameObject> ExplodedBalls { get; set; } = new HashSet<GameObject>();

        private void Map_ExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (ev.Grenade.TryGetComponent<Scp018Grenade>(out Scp018Grenade _))
            {
                ev.Grenade.AddComponent<Scp018Fix>();
                ExplodedBalls.Add(ev.Grenade);
            }
        }

        private void Server_RoundStarted()
        {
            ExplodedBalls.Clear();
        }
    }
}
