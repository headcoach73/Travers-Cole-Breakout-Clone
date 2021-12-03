using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BreakoutClone
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_scoreText;

        [SerializeField]
        private string m_scoreTitleString = "Score:";

        private void Update()
        {
            m_scoreText.text = m_scoreTitleString + " " + ScoreManager.CurrentScore.ToString();
        }
    }
}