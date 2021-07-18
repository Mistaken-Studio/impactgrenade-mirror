// -----------------------------------------------------------------------
// <copyright file="Scp018Fix.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Grenades;
using Mistaken.API;
using Mistaken.RoundLogger;
using UnityEngine;

namespace Mistaken.ImpactGrenade
{
    /// <summary>
    /// Collision after ball explosion handling.
    /// </summary>
    public class Scp018Fix : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (this == null)
                RLogger.Log("SCP018 FIX", "COLLISION", "Ball is null... but how???");
            else
            {
                if (BallFixModule.ExplodedBalls.Contains(this?.gameObject))
                {
                    RLogger.Log("SCP018 FIX", "PREDESTROY", "Trying to destroy the ball..");
                    Mirror.NetworkServer.Destroy(this?.gameObject);
                    if (this == null)
                        RLogger.Log("SCP018 FIX", "DESTROY", "The ball got destroyed");
                }
            }
        }
    }
}
