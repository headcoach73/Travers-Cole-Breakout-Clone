using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BreakoutClone
{
    [RequireComponent(typeof(MeshRenderer))]
    public class FakeBallController : MonoBehaviour
    {
        private MeshRenderer m_fakeBallMeshRenderer;

        private void Awake()
        {
            m_fakeBallMeshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetFakeBallColor(Color newColor)
        {
            m_fakeBallMeshRenderer.material.SetColor("_Color", newColor);
        }

        public void SetFakeBallVisibility(bool visible)
        {
            m_fakeBallMeshRenderer.enabled = visible;
        }
    }
}