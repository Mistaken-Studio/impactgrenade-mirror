// -----------------------------------------------------------------------
// <copyright file="ImpHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Exiled.API.Features.Items;
using Exiled.API.Interfaces;
using Exiled.CustomItems.API.Features;
using Mistaken.API.Diagnostics;
using Mistaken.RoundLogger;
using UnityEngine;

namespace Mistaken.ImpactGrenade
{
    /// <inheritdoc/>
    public class ImpHandler : Module
    {
        /// <inheritdoc cref="Module.Module(IPlugin{IConfig})"/>
        public ImpHandler(IPlugin<IConfig> plugin)
            : base(plugin)
        {
            Instance = this;
            new ImpItem().TryRegister();
        }

        /// <inheritdoc/>
        public override string Name => "ImpHandler";

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => this.Server_RoundStarted(), "RoundStart");
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => this.Server_RoundStarted(), "RoundStart");
        }

        internal static readonly Vector3 Size = new Vector3(1f, .40f, 1f);

        internal static ImpHandler Instance { get; private set; }

        private void Server_RoundStarted()
        {
            ExplodeDestructiblesPatch.Grenades.Clear();
            ServerThrowPatch.ThrowedItems.Clear();
            var structureLockers = UnityEngine.Object.FindObjectsOfType<MapGeneration.Distributors.SpawnableStructure>().Where(x => x.StructureType == MapGeneration.Distributors.StructureType.LargeGunLocker);
            var lockers = structureLockers.Select(x => x as MapGeneration.Distributors.Locker).Where(x => x.Chambers.Length > 8).ToArray();
            var locker = lockers[UnityEngine.Random.Range(0, lockers.Length)];
            int toSpawn = 6;
            while (toSpawn > 0)
            {
                var chamber = locker.Chambers[UnityEngine.Random.Range(0, locker.Chambers.Length)];
                CustomItem.TrySpawn(2, chamber._spawnpoint.position + (Vector3.up / 10), out Pickup pickup);
                chamber._content.Add(pickup.Base);
                RLogger.Log("IMPACT GRENADE", "SPAWN", $"Impact grenade spawned");
                toSpawn--;
            }
        }
    }
}
