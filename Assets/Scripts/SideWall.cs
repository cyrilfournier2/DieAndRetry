using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Classe gérant les interations entre le joueur et le sol
 */

public class SideWall : MonoBehaviour
{
    /*
    public GameObject player;
    private PlayerControls playerScript;

    // Variable indiquant si il s'agit d'un mur droit ou gauche
    public bool isRight;

    private void Start()
    {
        playerScript = player.GetComponent<PlayerControls>();
    }

    // Si une collision se produit entre le joueur et le sol, on appelle la fonction "Ground()" du joueur
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name == "Player")
        {
            Debug.Log("Collision enter - " + isRight + " - " + name, this);
            if (isRight)
                playerScript.againstWallR = true;
            else
                playerScript.againstWallL = true;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.name == "Player")
        {
            Debug.Log("Collision exit - " + isRight + " - " +name, this);
            if (isRight)
                playerScript.againstWallR = false;
            else
                playerScript.againstWallL = false;
        }
    }*/
}
