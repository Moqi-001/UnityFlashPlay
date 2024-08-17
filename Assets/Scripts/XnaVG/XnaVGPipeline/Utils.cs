using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


    public static class Utils
    {
        //public static Vector2 ToVec2(this System.Windows.Point point)
        //{
        //    return new Vector2((float)point.X, (float)-point.Y);
        //}

        public static Vector2 ToVec2(this Point point)
        {
            return new Vector2((float)point.X, (float)-point.Y);
        }

        public static UnityEngine.Vector2 ScaleAndOffset(this UnityEngine.Vector2 vector2, Vector2 scale, Vector2 offset)
        {
            vector2.x *= scale.X;
            vector2.y *= scale.Y;
            vector2.x += offset.X;
            vector2.y += offset.Y;
            return vector2;
        }

    }

