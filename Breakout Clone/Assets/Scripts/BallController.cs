using UnityEngine;
using Mirror;

namespace BreakoutClone
{
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : NetworkBehaviour
    {
        public Vector3 Direction { get; private set; }
        public bool IsBallFlying => m_movementEnabled;

        [SerializeField]
        [Tooltip("Scales the balls speed")]
        private float m_speed;

        [SerializeField]
        [Tooltip("Max angle from vertical ball can be launched from")]
        private float m_maxLaunchAngle = 45;
        [SerializeField]
        [Tooltip("Used to avoid self collisions with raycasts")]
        private LayerMask m_collisionLayerMask;

        private PaddleController m_ballOwner;
        private MeshRenderer m_meshRenderer;
        private bool m_movementEnabled;

        [SyncVar(hook=nameof(VisibilityHook))]
        private bool m_isBallVisible = true;

        [SyncVar(hook = nameof(BallColorHook))]
        private Color m_ballColor;

        private void Awake()
        {
            m_meshRenderer = GetComponent<MeshRenderer>();
        }

        private void BallColorHook(Color oldValue, Color newValue)
        {
            m_meshRenderer.material.SetColor("_Color", newValue);
        }

        private void VisibilityHook(bool oldValue, bool newValue)
        {
            m_meshRenderer.enabled = newValue;
        }

        /// <summary>
        /// Manually set the balls direction
        /// </summary>
        /// <param name="newDirection"></param>
        [Server]
        public void SetDirection(Vector3 newDirection)
        {
            Direction = newDirection;
        }

        /// <summary>
        /// Launch ball from start position on owner paddle.
        /// </summary>
        [Server]
        public void LaunchBall()
        {
            //Set position to launch position
            transform.position = m_ballOwner.BallStartTransform.position;
            //Send teleport Rpc so doesn't try to interpolate there on remote client
            GetComponent<NetworkTransform>().RpcTeleport(transform.position);

            var launchAngle = Random.Range(-m_maxLaunchAngle, m_maxLaunchAngle);
            if (launchAngle == 0) launchAngle = 0.1f; //Avoids quaternion of size zero
            var launchDirection = Quaternion.Euler(0, 0, launchAngle) * Vector3.up;
            Direction = launchDirection.normalized;
            m_movementEnabled = true;

            //Make ball visible
            m_isBallVisible = true;
            m_ballOwner.SetFakeBallVisibility(false);
        }

        /// <summary>
        /// Reflects the ball direction along plane described by wallNormal param
        /// </summary>
        /// <param name="wallNormal"></param>
        [Server]
        public void Bounce(Vector3 wallNormal)
        {
            Direction = Vector3.Reflect(Direction, wallNormal);
        }


        /// <summary>
        /// Sets the ballOwner paddle and Resets the ball
        /// </summary>
        /// <param name="ballOwner"></param>
        public void InitBallController(PaddleController ballOwner)
        {
            m_ballOwner = ballOwner;
            if (isServer) ResetBall();
        }

        /// <summary>
        /// Sets the real ball invisible and sets the fake ball visible
        /// </summary>
        [Server]
        public void ResetBall()
        {
            m_movementEnabled = false;
            //Hide ball
            m_isBallVisible = false;
            //Show Fake Ball
            m_ballOwner.SetFakeBallVisibility(true);
        }

        /// <summary>
        /// Sets the ball color. This is synced to all clients via hook
        /// </summary>
        /// <param name="newColor"></param>
        [Server]
        public void SetBallColor(Color newColor)
        {
            m_ballColor = newColor;
        }


        private void OnValidate()
        {
            //Ensure ball is a kinematic trigger
            GetComponent<SphereCollider>().isTrigger = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            //Let server handle collision
            if (!isServer) return;

            //Check if collider is hitable
            var hitable = other.GetComponent<IHitable>();
            if (hitable == null || !hitable.CollisionValid(this)) return;

            var closestPoint = other.ClosestPoint(transform.position);
            //Find last position before the hit
            var lastPosition = transform.position - Direction * m_speed * Time.deltaTime;

            RaycastHit hit;
            //Don't really need max distance so just use velocity instead should always be reasonable
            //Layer mask needed so doesnt self collide
            if (Physics.Raycast(lastPosition, closestPoint - lastPosition, out hit, m_speed, m_collisionLayerMask)) 
            {
                //Bounce ball off hitable surface normal
                Bounce(hit.normal);

                hitable.OnBallHit(this);
            }
            else
            {
                Debug.LogWarning("Raycast failed to hit collider called by OnTriggerEnter");
            }
        }

        void Update()
        {
            if (m_movementEnabled && isServer)
            {
                CheckBoundaryCollisions();
                ApplyVelocity();
            }
        }

        private void ApplyVelocity()
        {
            transform.position = transform.position + Direction * m_speed * Time.deltaTime;
        }

        private void CheckBoundaryCollisions()
        {
            var position = transform.position;
            var wallPositions = RegionManager.WallPositions;
            if (position.x < wallPositions.Left)
            {
                Bounce(Vector3.right);
                position.x = wallPositions.Left;
            }

            if (position.x > wallPositions.Right)
            {
                Bounce(Vector3.left);
                position.x = wallPositions.Right;
            }

            if (position.y < wallPositions.Bottom)
            {
                //Then you lose
                ResetBall();
                return; //Must return so position doesn't get reset
            }

            if (position.y > wallPositions.Top)
            {
                Bounce(Vector3.down);
                position.y = wallPositions.Top;
            }

            transform.position = position;
        }
    }
}