// -----------------------------------------------------------------------
// <copyright file="ImpHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Grenades;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using Mistaken.CustomItems;
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
            instance = this;
            new ImpItem();
        }

        /// <inheritdoc/>
        public override string Name => "ImpHandler";

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => this.Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => this.Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Map.ChangingIntoGrenade += this.Handle<Exiled.Events.EventArgs.ChangingIntoGrenadeEventArgs>((ev) => this.Map_ChangingIntoGrenade(ev));
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => this.Map_ExplodingGrenade(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => this.Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Map.ChangingIntoGrenade -= this.Handle<Exiled.Events.EventArgs.ChangingIntoGrenadeEventArgs>((ev) => this.Map_ChangingIntoGrenade(ev));
        }

        /// <summary>
        /// Grenade that explodes on impact.
        /// </summary>
        public class ImpItem : CustomItem
        {
            /// <summary>
            /// Gives Impact Grenade to <paramref name="player"/>.
            /// </summary>
            /// <param name="player">Player that Impact Grenade should be given to.</param>
            public static void Give(Player player)
            {
                if (player.Inventory.items.Count < 8)
                {
                    player.AddItem(new Inventory.SyncItemInfo
                    {
                        durability = 1000f,
                        id = ItemType.GrenadeFrag,
                    });
                    player.SetSessionVar(SessionVarType.CI_IMPACT, true);
                }
            }

            /// <inheritdoc cref="CustomItem.Register"/>
            public ImpItem() => this.Register();

            /// <inheritdoc/>
            public override string ItemName => "Impact Grenade";

            /// <inheritdoc/>
            public override ItemType Item => ItemType.GrenadeFrag;

            /// <inheritdoc/>
            public override SessionVarType SessionVarType => SessionVarType.CI_IMPACT;

            /// <inheritdoc/>
            public override int Durability => 001;

            /// <inheritdoc/>
            public override Vector3 Size => ImpHandler.Size;

            /// <inheritdoc/>
            public override Upgrade[] Upgrades => new Upgrade[]
            {
                new Upgrade
                {
                    Chance = 100,
                    Durability = null,
                    Input = ItemType.GrenadeFrag,
                    KnobSetting = Scp914.Scp914Knob.Fine,
                },
            };

            /// <inheritdoc/>
            public override bool OnThrow(Player player, Inventory.SyncItemInfo item, bool slow)
            {
                instance.CallDelayed(
                    1f,
                    () =>
                    {
                        RLogger.Log("IMPACT GRENADE", "THROW", $"{player.PlayerToString()} threw an impact grenade");
                        if (player.GetEffectActive<CustomPlayerEffects.Scp268>())
                            player.DisableEffect<CustomPlayerEffects.Scp268>();
                        Grenade grenade = UnityEngine.Object.Instantiate(player.GrenadeManager.availableGrenades[0].grenadeInstance).GetComponent<Grenade>();
                        grenade.fuseDuration = 999;
                        grenade.InitData(player.GrenadeManager, Vector3.zero, player.CameraTransform.forward, slow ? 0.5f : 1f);
                        grenades.Add(grenade.gameObject);
                        Mirror.NetworkServer.Spawn(grenade.gameObject);
                        grenade.GetComponent<Rigidbody>().AddForce(new Vector3(grenade.NetworkserverVelocities.linear.x * 1.5f, grenade.NetworkserverVelocities.linear.y / 2f, grenade.NetworkserverVelocities.linear.z * 1.5f), ForceMode.VelocityChange);
                        player.RemoveItem(item);
                        grenade.gameObject.AddComponent<ImpComponent>();
                        this.OnStopHolding(player, item);
                    },
                    "OnThrow");
                return false;
            }

            /// <inheritdoc/>
            public override void OnStartHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.SetGUI("impact", PseudoGUIPosition.BOTTOM, "Trzymasz <color=yellow>Granat Uderzeniowy</color>");
            }

            /// <inheritdoc/>
            public override void OnStopHolding(Player player, Inventory.SyncItemInfo item)
            {
                player.SetGUI("impact", PseudoGUIPosition.BOTTOM, null);
            }

            /// <inheritdoc/>
            public override void OnForceclass(Player player)
            {
                player.SetGUI("impact", PseudoGUIPosition.BOTTOM, null);
            }
        }

        internal static readonly Vector3 Size = new Vector3(1f, .40f, 1f);
        internal static readonly float Damage_multiplayer = 0.14f;
        private static HashSet<GameObject> grenades = new HashSet<GameObject>();

        private static ImpHandler instance;
        private GrenadeManager lastImpactThrower;

        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (!grenades.Contains(ev.Grenade))
                return;
            RLogger.Log("IMPACT GRENADE", "EXPLODED", $"Impact grenade exploded");
            var tmp = ev.Grenade.GetComponent<FragGrenade>().thrower;
            this.lastImpactThrower = tmp;
            this.CallDelayed(
                1,
                () =>
                {
                    if (this.lastImpactThrower == tmp)
                        this.lastImpactThrower = null;
                },
                "MapExploadingGrenade");
            foreach (Player player in ev.TargetToDamages.Keys.ToArray())
            {
                ev.TargetToDamages[player] *= Damage_multiplayer;
                RLogger.Log("IMPACT GRENADE", "HURT", $"{player.PlayerToString()} was hurt by an impact grenade");
            }
        }

        private void Server_RoundStarted()
        {
            grenades.Clear();
            var lockers = LockerManager.singleton.lockers.Where(i => i.chambers.Length == 9).ToArray();
            int toSpawn = 8;
            while (toSpawn > 0)
            {
                var locker = lockers[UnityEngine.Random.Range(0, lockers.Length)];
                locker.AssignPickup(ItemType.GrenadeFrag.Spawn(1000f, locker.chambers[UnityEngine.Random.Range(0, locker.chambers.Length)].spawnpoint.position));
                RLogger.Log("IMPACT GRENADE", "SPAWN", $"Impact grenade spawned");
                toSpawn--;
            }
        }

        private void Map_ChangingIntoGrenade(Exiled.Events.EventArgs.ChangingIntoGrenadeEventArgs ev)
        {
            if (ev.Pickup.durability == 1000f)
            {
                ev.IsAllowed = false;
                Grenade grenade = UnityEngine.Object.Instantiate(Server.Host.GrenadeManager.availableGrenades[0].grenadeInstance).GetComponent<Grenade>();
                grenades.Add(grenade.gameObject);
                grenade.fuseDuration = 0.01f;
                grenade.InitData(this.lastImpactThrower ?? Server.Host.GrenadeManager, Vector3.zero, Vector3.zero, 0f);
                grenade.transform.position = ev.Pickup.position;
                Mirror.NetworkServer.Spawn(grenade.gameObject);
                ev.Pickup.Delete();
                RLogger.Log("IMPACT GRENADE", "CHAINED", $"Impact grenade chained");
            }
        }
    }
}
