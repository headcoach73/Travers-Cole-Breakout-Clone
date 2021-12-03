using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace BreakoutClone
{
    public class PaddleController : NetworkBehaviour, IHitable
    {
        public BallController BallController { get; set; }
        public FakeBallController FakeBallController => m_fakeBallController;
        public Transform BallStartTransform => m_ballStartTransform;

        [Header("Paddle Settings")]
        [SerializeField]
        [Tooltip("The amount of added x velocity added to the ball when it hits the paddle. " +
            "This is scaled by how far away the collision is from the centre of the paddle")]
        private float m_ballAcceleration;
        [SerializeField]
        [Tooltip("Size of the paddle")]
        private float m_paddleSize = 3f;
        [SerializeField]
        [Tooltip("Creates a buffer between the wall and the paddle that the paddle cannot cross.")]
        private float m_wallBuffer = 0.2f;

        [Header("Ball")]
        [SerializeField]
        private Transform m_ballStartTransform;

        [SerializeField]
        private FakeBallController m_fakeBallController;

        //Visibility synced here so fake ball controller doesn't need network identity
        [SyncVar(hook =nameof(FakeBallVisibilityHook))]
        private bool m_fakeBallVisible;

        //Paddle boundary
        [SyncVar]
        private float m_minPaddlePosition;
        [SyncVar]
        private float m_maxPaddlePosition;

        public bool CollisionValid(BallController ballController)
        {
            return ballController == BallController;
        }

        public void OnBallHit(BallController ballController)
        {
            //Add x velocity to the ball based on the offset of the hit
            var xHitOffset = ballController.transform.position.x - transform.position.x;

            //scale by paddle width
            xHitOffset /= m_paddleSize / 2;
            Vector3 additonalVelocity = new Vector3(m_ballAcceleration * xHitOffset, 0, 0);
            //Set Direction will handle normalizing the direction
            ballController.SetDirection(ballController.Direction + additonalVelocity);
        }

        /// <summary>
        /// Sets fake ball visibility on all clients
        /// </summary>
        /// <param name="visible"></param>
        [Server]
        public void SetFakeBallVisibility(bool visible)
        {
            m_fakeBallVisible = visible;
        }

        /// <summary>
        /// Allows remote clients to ask to launch their ball
        /// </summary>
        [Command]
        public void LaunchBallCommand()
        {
            LaunchBall();
        }

        /// <summary>
        /// Launches Ball if it is not already flying
        /// </summary>
        [Server]
        private void LaunchBall()
        {
            if (BallController.IsBallFlying) return; //Don't launch if its already flying!!!!!!!!!!!!!!

            BallController.LaunchBall();
        }

        /// <summary>
        /// Sets paddle world position within constraints
        /// </summary>
        /// <param name="targetWorldPosition"></param>
        public void UpdatePaddlePosition(Vector3 targetWorldPosition)
        {
            var currentPosition = transform.position;
            currentPosition.x = targetWorldPosition.x;

            currentPosition.x = Mathf.Clamp(currentPosition.x, m_minPaddlePosition, m_maxPaddlePosition);
            transform.position = currentPosition;
        }

        
        private void OnValidate()
        {
            UpdatePaddleSize();
        }

        public override void OnStartServer()
        {
            RegionManager.OnScreenResized += UpdatePaddleBoundary;
            UpdatePaddleBoundary(); //Ensure boundary has been calculated
        }

        public override void OnStopServer()
        {
            RegionManager.OnScreenResized -= UpdatePaddleBoundary;
        }


        private void UpdatePaddleSize()
        {
            var currentScale = transform.localScale;
            currentScale.x = m_paddleSize;
            transform.localScale = currentScale;
        }


        private void UpdatePaddleBoundary()
        {
            var wallPositions = RegionManager.WallPositions;
            m_minPaddlePosition = wallPositions.Left + (m_paddleSize / 2) + m_wallBuffer;
            m_maxPaddlePosition = wallPositions.Right - (m_paddleSize / 2) - m_wallBuffer;
        }

        private void FakeBallVisibilityHook(bool oldValue, bool newValue)
        {
            m_fakeBallController.SetFakeBallVisibility(newValue);
        }
    }
}