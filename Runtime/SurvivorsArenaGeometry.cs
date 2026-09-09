using System;
using System.Collections.Generic;
using UnityEngine;
using Deucarian.Common;

namespace Deucarian.TemplateGameSurvivors
{
    internal static class SurvivorsArenaGeometry
    {
        public const float InfiniteArenaTileSize = 12f;
        public const int InfiniteArenaGridRadius = 2;
        public const int InfiniteArenaLandmarkCount = 8;
        public const float InfiniteArenaLandmarkJitterRadius = 2.35f;

        public static float ResolveArenaPresentationAnchor(float value)
        {
            return Mathf.Floor((value + InfiniteArenaTileSize * 0.5f) / InfiniteArenaTileSize) * InfiniteArenaTileSize;
        }

        public static void ResolveArenaLandmarkOffset(int index, out int offsetX, out int offsetZ)
        {
            switch (index % InfiniteArenaLandmarkCount)
            {
                case 0:
                    offsetX = -1;
                    offsetZ = -1;
                    return;
                case 1:
                    offsetX = 0;
                    offsetZ = -1;
                    return;
                case 2:
                    offsetX = 1;
                    offsetZ = -1;
                    return;
                case 3:
                    offsetX = -1;
                    offsetZ = 0;
                    return;
                case 4:
                    offsetX = 1;
                    offsetZ = 0;
                    return;
                case 5:
                    offsetX = -1;
                    offsetZ = 1;
                    return;
                case 6:
                    offsetX = 0;
                    offsetZ = 1;
                    return;
                default:
                    offsetX = 1;
                    offsetZ = 1;
                    return;
            }
        }

        public static float ResolveArenaLandmarkJitter(int cellX, int cellZ, int salt)
        {
            unchecked
            {
                int hash = cellX * 73856093 ^ cellZ * 19349663 ^ salt * 83492791;
                int positive = hash & 0x7fffffff;
                return (positive % 1001) / 1000f - 0.5f;
            }
        }

        public static long ResolveArenaLandmarkKey(int cellX, int cellZ)
        {
            return ((long)cellX << 32) ^ (uint)cellZ;
        }
    }
}
