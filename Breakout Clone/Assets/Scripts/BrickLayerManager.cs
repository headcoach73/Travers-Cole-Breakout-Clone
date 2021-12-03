using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace BreakoutClone
{
    public class BrickLayerManager : NetworkBehaviour
    {
        public static BrickLayerManager Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton != null)
            {
                Debug.LogWarning("BrickLayerManger Singleton was not null!!!!");
                Destroy(Singleton);
            }
            Singleton = this;
        }


        [Header("Spawning")]
        [SerializeField]
        private GameObject m_brickPrefab;

        [SerializeField]
        [Tooltip("An offset to where all brick layers spawn, useful for final adjustments")]
        private float m_yPositionOffset = -1;

        [SerializeField]
        [Tooltip("Sets the Y scale of individual bricks")]
        private float m_brickHeight = 0.8f;

        [SerializeField]
        [Tooltip("Controls overall width of each layer")]
        private float m_layersWidth = 10;

        [SerializeField]
        [Tooltip("Set total amount of brick in each layer")]
        private int m_bricksPerLayer = 6;

        [SerializeField]
        [Tooltip("How much the total length of each brick can vary by")]
        [Range(0f,0.8f)]
        private float m_brickWidthVariance = 0.2f;

        [SerializeField]
        [Tooltip("Horizontal gap between bricks in a layer")]
        [Range(0,0.5f)]
        private float m_brickGap = 0.1f;

        [Header("Layers")]
        [SerializeField]
        private List<LayerSettings> m_layerSettings = new List<LayerSettings>();


        private List<Brick> m_bricks = new List<Brick>();

        public static Material GetLayerMaterial(int index)
        {
            if (index > Singleton.m_layerSettings.Count)
            {
                Debug.LogWarning("Index of layer out of range");
                return null;
            }
            return Singleton.m_layerSettings[index].LayerMaterial;
        }

        /// <summary>
        /// Clears layers then spawns each layer of bricks
        /// </summary>
        [Server]
        public void SpawnLayers()
        {
            ClearLayers();

            for (int i = 0; i < m_layerSettings.Count; i++)
            {
                SpawnLayer(i);
            }
        }

        /// <summary>
        /// Clear all bricks 
        /// </summary>
        [Server]
        public void ClearLayers()
        {
            foreach (var brick in m_bricks)
            {
                if (brick != null) Destroy(brick);
            }

            m_bricks.Clear();
        }

        private void OnValidate()
        {
            foreach (var layer in m_layerSettings)
            {
                layer.LayerMaterial?.SetColor("_Color", layer.LayerColour);
            }
        }

        public override void OnStartServer()
        {
            SpawnLayers();
        }
        private void SpawnLayer(int layerIndex)
        {
            LayerSettings layerSettings = m_layerSettings[layerIndex];

            var currentPosition = new Vector3(-(m_layersWidth / 2f), layerSettings.LayerYPosition + m_yPositionOffset, 0);

            float totalGapSpace = m_brickGap * (m_bricksPerLayer - 1);
            var averageBrickWidth = (m_layersWidth - totalGapSpace) / m_bricksPerLayer;
            float layerEndPosition = m_layersWidth / 2;

            while (currentPosition.x < layerEndPosition)
            {
                var newBrickWidth = averageBrickWidth + (Random.Range(-m_brickWidthVariance, m_brickWidthVariance) * averageBrickWidth);

                //Need to prevent brick going off edge or last brick being tiny
                //So if its going to be close just set it to the end width
                bool brickCloseToEndPosition = newBrickWidth + currentPosition.x > layerEndPosition - (m_brickWidthVariance*averageBrickWidth);
                if (brickCloseToEndPosition)
                {
                    //Set width to exactly end of layer width
                    newBrickWidth = layerEndPosition - currentPosition.x;
                }

                var newBrick = Instantiate(m_brickPrefab).GetComponent<Brick>();

                //Set Material
                newBrick.SetLayerIndex(layerIndex);

                m_bricks.Add(newBrick);

                //Set position and scale of new brick
                newBrick.transform.position = currentPosition + new Vector3(newBrickWidth / 2, 0, 0);
                var currentScale = newBrick.transform.localScale;
                currentScale.x = newBrickWidth;
                currentScale.y = m_brickHeight;
                newBrick.transform.localScale = currentScale;

                currentPosition.x += newBrickWidth + m_brickGap;

                //Spawn to remote clients
                NetworkServer.Spawn(newBrick.gameObject);
            }
        }
    }

    [System.Serializable]
    public class LayerSettings 
    {
        [Header("Colours")]
        public Material LayerMaterial;

        [Tooltip("This will override the layermaterials color")]
        public Color LayerColour;

        [Header("Position")]
        [Tooltip("Relative to LayerManager Y position offset")]
        public float LayerYPosition = 3;
    }

}