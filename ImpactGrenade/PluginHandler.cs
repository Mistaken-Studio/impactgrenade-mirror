// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace Mistaken.ImpactGrenade
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Mistaken Devs";

        /// <inheritdoc/>
        public override string Name => "ImpactGrenade";

        /// <inheritdoc/>
        public override string Prefix => "MIMP";

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Medium;

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(3, 0, 4);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;

            this.harmony = new HarmonyLib.Harmony("com.mistaken.impactgrenade");
            this.harmony.PatchAll();

            new ImpHandler(this);

            // new BallFixModule(this);
            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            this.harmony.UnpatchAll();

            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        private HarmonyLib.Harmony harmony;
    }
}
