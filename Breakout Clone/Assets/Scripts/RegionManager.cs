using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BreakoutClone 
{
    public class RegionManager : MonoBehaviour
    {
        public static WallPositions WallPositions;

        //Only the server subscribes to this delegate so wall positions is based on the host
        public delegate void ScreenResized();
        public static ScreenResized OnScreenResized;

        private float lastScreenHeight;
        private float lastScreenWidth;

        private void Update()
        {
            //Check if screen size changed
            if (lastScreenHeight != Screen.height || lastScreenWidth != Screen.width)
            {
                UpdateWallPositions();

                lastScreenHeight = Screen.height;
                lastScreenWidth = Screen.width;

                OnScreenResized?.Invoke();
            }
        }

        private static void UpdateWallPositions()
        {
            var camera = Camera.main;
            var bottomLeftCorner = camera.ScreenToWorldPoint(Vector3.zero);
            var topRightCorner = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            WallPositions.Left = bottomLeftCorner.x;
            WallPositions.Right = topRightCorner.x;
            WallPositions.Bottom = bottomLeftCorner.y;
            WallPositions.Top = topRightCorner.y;
        }
    }

    public struct WallPositions
    {
        public float Top;
        public float Bottom;
        public float Left;
        public float Right;
    }

}

