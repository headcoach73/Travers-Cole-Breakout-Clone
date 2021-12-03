using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BreakoutClone
{
    public class PlayerInputs : MonoBehaviour
    {
        public static PaddleController PlayerPaddleController { get; private set; }

        /// <summary>
        /// Sets player paddle controller for local player
        /// </summary>
        /// <param name="playerPaddle"></param>
        public static void SetPlayerPaddle(PaddleController playerPaddle)
        {
            PlayerPaddleController = playerPaddle;

            //Don't lock cursor until paddle initied
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }


        // Update is called once per frame
        void Update()
        {
            if (PlayerPaddleController == null) 
            {
                //Incase we have left the game
                if (!Cursor.visible)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }

                return;
            } 

            if (Cursor.visible) Cursor.visible = false;


            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayerPaddleController.LaunchBallCommand();
            }

            
            var worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            PlayerPaddleController.UpdatePaddlePosition(worldMousePosition);
        }
    }
}