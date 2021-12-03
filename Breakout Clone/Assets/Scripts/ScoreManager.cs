using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace BreakoutClone
{
    public class ScoreManager : NetworkBehaviour
    {
        #region Singleton

        public static ScoreManager Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton != null)
            {
                Debug.LogWarning("ScoreManager Singleton was not null!!!!");
                Destroy(Singleton);
            }
            Singleton = this;
        }

        #endregion
        public static int CurrentScore => Singleton.m_currentScore;

        [SyncVar]
        private int m_currentScore;

        [SerializeField]
        private int m_pointsPerBrick = 100;

        public override void OnStartServer()
        {
            m_currentScore = 0;
        }

        public static void AddBrickBreakScore()
        {
            Singleton.m_currentScore += Singleton.m_pointsPerBrick;
        }
    }
}