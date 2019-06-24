﻿// simple script to always set y position to order in layer for visiblity
using UnityEngine;

namespace E.Utility
{
    public class SortByDepth : MonoBehaviour
    {
        new public SpriteRenderer renderer;

        // precision is useful for cases where two players stand at
        //   y=0 and y=0.1, which would both be sortingOrder=0 otherwise
        public int precision = 100;

        // offset in case it's needed (e.g. for mounts that should be behind the
        // player, even if the player is above it in .y)
        public int offset = 0;

        void Update()
        {
            // we negate it because that's how Unity's sorting order works
            renderer.sortingOrder = -Mathf.RoundToInt((transform.position.y + offset) * precision);
        }
    }
}