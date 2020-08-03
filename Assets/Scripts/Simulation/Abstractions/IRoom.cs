using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Pathfinding;
using UnityEngine;

namespace Assets.Scripts.Simulation.Abstractions
{
    public interface IRoom
    {
        /// <summary>
        /// Gets the dimensional extent of the <see cref="IRoom"/> in meter (without the wall).
        /// </summary>
        /// <remarks>
        /// This value is not allowed to change.
        /// </remarks>
        Vector3 RoomSize { get; }

        /// <summary>
        /// Gets the global position of the <see cref="IRoom"/>.
        /// </summary>
        /// <remarks>
        /// This value is not allowed to change.
        /// </remarks>
        Vector3 RoomPosition { get; }

        /// <summary>
        /// Gets the thickness of the walls of the <see cref="IRoom"/>.
        /// </summary>
        /// <remarks>
        /// This value is not allowed to change.
        /// </remarks>
        float WallThickness { get; }

        Graph RoomGraph { get; }
    }
}
