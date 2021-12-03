using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BreakoutClone
{
    public interface IHitable
    {
        public bool CollisionValid(BallController ballController);
        public void OnBallHit(BallController ballController);
    }
}