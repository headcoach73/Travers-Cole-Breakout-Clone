using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

namespace BreakoutClone
{
    [RequireComponent(typeof(PaddleController))]
    public class PlayerManager : NetworkBehaviour
    {
        [SyncVar(hook =(nameof(PlayerSettingsHook)))]
        private PlayerSettings m_playerSettings;

        [SerializeField]
        private GameObject m_ballPrefab;

        private PaddleController m_paddleController;
        private BallController m_ballController;

        private void Awake()
        {
            m_paddleController = GetComponent<PaddleController>();
        }

        [Server]
        public void SetPlayerSettings(PlayerSettings playerSettings)
        {
            m_playerSettings = playerSettings;
            
        }

        public override void OnStartServer()
        {
            //Create ball for player
            m_ballController = Instantiate(m_ballPrefab).GetComponent<BallController>();
            NetworkServer.Spawn(m_ballController.gameObject);
            m_paddleController.BallController = m_ballController;
            m_ballController.InitBallController(m_paddleController);
            m_ballController.SetBallColor(m_playerSettings.Color);
        }

        public override void OnStopServer()
        {
            //Remove ball if player disconnects
            NetworkServer.Destroy(m_ballController.gameObject);
        }

        public override void OnStartAuthority()
        {
            //Setup player inputs if this is the local player
            PlayerInputs.SetPlayerPaddle(m_paddleController);
        }

        private void PlayerSettingsHook(PlayerSettings oldValue, PlayerSettings newValue)
        {
            //Setup player settings
            GetComponent<Renderer>().material.SetColor("_Color", newValue.Color);
            m_paddleController.FakeBallController.SetFakeBallColor(newValue.Color);
        }
    }

    [System.Serializable]
    public struct PlayerSettings 
    {
        public Color Color;
        public float HeightPosition;
    }

}