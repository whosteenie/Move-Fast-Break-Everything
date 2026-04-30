using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TreeLore : MonoBehaviour
{
    bool triggered = false;

    public Canvas TreeCanvas;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && triggered == false)
        {
            //Play the camera sound, 
            // pop up the lore screen
            TreeCanvas.enabled = true;
            StartCoroutine(RemoveCanvasTimer());
            Time.timeScale = .5f;
            // send the picture to game manager for the game over screen.
        }
        if(collision.tag == "Player")
        {
            triggered = true;
        }
    }

    private IEnumerator RemoveCanvasTimer()
    {
       yield return new WaitForSeconds(3);
       RemoveCanvas();
    }

    private void RemoveCanvas()
    {
        TreeCanvas.enabled = false; 
        Time.timeScale = 1f;
        print("Removed Canvas");
    }


}
