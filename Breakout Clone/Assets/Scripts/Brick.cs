using Mirror;
using UnityEngine;

namespace BreakoutClone
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Brick : NetworkBehaviour, IHitable
    {
        [SyncVar(hook = nameof(LayerIndexHook))]
        private int m_layerIndex = -1; //Set negative one so hook will work

        private void LayerIndexHook(int oldVale, int newValue)
        {
            GetComponent<MeshRenderer>().sharedMaterial = BrickLayerManager.GetLayerMaterial(newValue);
        }

        /// <summary>
        /// Sets layer index so remote clients know which material to use
        /// </summary>
        /// <param name="index"></param>
        [Server]
        public void SetLayerIndex(int index)
        { 
            m_layerIndex = index;
        }


        [Server]
        public void OnBallHit(BallController ballController)
        {
            //If hit by ball self destruct
            ScoreManager.AddBrickBreakScore();
            NetworkServer.Destroy(gameObject);
        }

        public bool CollisionValid(BallController ballController)
        {
            //Collision always valid
            return true;
        }
    }
}