using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
 * Classe gérant les contrôles et autres aspects du joueur
 */ 

public class PlayerControls : MonoBehaviour {

    // Variables d'état du joueur
    private bool isReady;
    private bool isGrounded;
    private bool hasJumped;
    private bool doubleJumpReady;
    private bool isDead = false;
    private bool facingRight = true;

    // Variable indiquant que le personnage slide contre un mur
    public bool wallSliding;

    // Références portant sur le contact avec un mur
    public Transform wallCheckPoint;
    public bool wallCheck;
    public LayerMask wallLayerMask;

    // Valeur de l'index de la scène actuelle
    private int currentScene;

    // Touches de mouvements du personnage
    public KeyCode moveLeft;
    public KeyCode moveRight;
    public KeyCode jump;
    public KeyCode down;

    // Variable permettant le "drag" horizontal du personnage
    private float drag = 0.2f;

    // Vitesse du joueur
    public float speed = 10.0f;
    // Vitesse de descente du joueur
    public float downSpeed = 3.0f;
    // Les différents objets nécessaires
    private Rigidbody2D rb2d;
    private GameManager gameManager;
    public GameObject player;
    
    private Timer timers;

    GameObject HUD;

    // Son de saut du joueur
    private AudioSource jumpSound;

    private Vector2 vect;

    // Fonction indiquant que le joueur est sur le sol
    public void Ground()
    {
        isGrounded = true;
    }

    // Fonction gérant la mort du joueur
    public void Die()
    {
        // Son état passe à "mort"
        isDead = true;
        // On appelle la fonction "PlayerDies()" du HUD
        HUD.SendMessage("PlayerDies", 0.5f, SendMessageOptions.RequireReceiver);
    }

    // Fonction reinitialisant le personnage
    public void ResetCharacter()
    {
        // On réinitialise les états du joueur
        transform.position = Vector2.down;
        isGrounded = false;
        hasJumped = false;
        doubleJumpReady = false;
        isDead = false;
        isReady = false;
        // On appelle la fonction "PlayerReborn()" du HUD
        HUD.SendMessage("PlayerReborn", 0.5f, SendMessageOptions.RequireReceiver);
    }

    // Fonction initialisant le personnage
    public void Ready()
    {
        // Le personnage est prêt à être déplacé
        isReady = true;

        // Et active les deux timers
        timers.timerOn = true;

        // On empêche le personnage de tourner sur lui-même et on autorise les déplacements
        rb2d.constraints = RigidbodyConstraints2D.None;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // Fonction réinitialisant le personnage et sa position
    public void Restart()
    {
        // On appelle les fonctions précédemment décrites
        ResetCharacter();
        Ready();
    }

    // Fonction réinitialisant la partie
    public void ReinitGame()
    {
        // On recommence la partie
        Restart();
        // Et on réinitialise le nombre de tentatives du joueur
        PlayerPrefs.SetInt("NbTotalTries", 0);          
    }

    void Start ()
    {
        // On initialise les objets nécessaires
        rb2d = GetComponent<Rigidbody2D>();
        HUD = GameObject.FindGameObjectWithTag("HUD");
        timers = player.GetComponent<Timer>();
        jumpSound = GetComponent<AudioSource>();

        // On empêche le son de saut d'être joué au démarrage
        jumpSound.playOnAwake = false;

        // On récupère l'index de la scène actuelle
        currentScene = SceneManager.GetActiveScene().buildIndex;

        // On récupère les touches de déplacement du personnage
        moveLeft = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("leftKey", "Q"));
        moveRight = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("rightKey", "D"));
        jump = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("jumpKey", "Z"));
        down = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("downKey", "S"));

        // On récupère l'objet gérant la partie
        gameManager = GameObject.FindObjectOfType<GameManager>();

        // Après une seconde, on autorise le déplacement du personnage
        StartCoroutine(WaitForReady());
    }

    public void OnGUI()
    {
        // Si le personnage est mort, on affiche le texte de mort en rouge
        if (isDead)
        {
            GUI.contentColor = Color.red;
            GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 200f, 200f), "VOUS ETES MORT MAMENE");
        }
    }
    
    void Update () {

        // On récupère le volume des effets du jeu pour celui du saut
        jumpSound.volume = PlayerPrefs.GetFloat("EffectVolume");

        // Si le personnage n'est pas mort et est prêt à être déplacé
        if (!isDead && isReady)
        {
            this.vect = rb2d.velocity;

            // On gère les déplacements selon le bouton pressé
            if (Input.GetKey(down))
            {
                vect.y = -speed;
            }
            else if (Input.GetKey(moveLeft) && Input.GetKey(moveRight))
            {
                vect.x = 0f;
            }
            else if (Input.GetKey(moveLeft))
            {
                vect.x = -speed;
            }
            else if (Input.GetKey(moveRight))
            {
                vect.x = speed;
            }
        }
        // Sinon, on bloque les mouvements du personnage et on réinitialise le timer du niveau
        else
        {
            // On bloque la position du personnage
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;

            //timers.ResetLevelTimer(Time.time);
            timers.timerOn = false;
        }

        if (!isGrounded)
        {

            wallCheck = Physics2D.OverlapCircle(wallCheckPoint.position, 0.25f, wallLayerMask);

            if ((facingRight && Input.GetKey(moveRight)) || (!facingRight && Input.GetKey(moveLeft)))
            {
                if (wallCheck)
                {
                    wallSliding = true;
                }
            }
        }

        if (!wallCheck || isGrounded)
            wallSliding = false;
    }

    // Fonction appliquant un drag horizontal après chaque frame et gérant les sauts
    private void FixedUpdate()
    {
        // On applique les déplacements si on ne slide pas
        if (!wallSliding) { 

            if (vect.x < 0) { 
                facingRight = false;
            }
            else { 
                facingRight = true;
            }
            Flip();
            rb2d.velocity = vect;
        }
        else
            HandleWallSliding();

        // Si le personnage n'est pas mort et est prêt à être déplacé
        if (!isDead && isReady) {
            // Si la touche de saut est enfoncée et le personnage est à terre
            if (Input.GetKey(jump) && isGrounded && !wallSliding)
            {
                // Applique la force effectuant le saut
                rb2d.AddForce(new Vector2(0, speed), ForceMode2D.Impulse);
                // Applique la vitesse souhaitée de saut pour éviter des sauts trop puissants
                rb2d.velocity = new Vector3(rb2d.velocity.x, 10f, rb2d.velocity.y);
                // Le personnage n'est plus à terre
                isGrounded = false;
                // Le personnage a sauté une fois
                hasJumped = true;
                // On lance le son de saut
                jumpSound.Play();

            }

            // Si on appuie sur le bouton de saut alors que le personnage n'est pas à terre et est prêt pour le second saut
            else if (Input.GetKey(jump) && doubleJumpReady && !isGrounded && !wallSliding)
            {
                // Applique la force effectuant le saut
                rb2d.AddForce(new Vector2(0, speed), ForceMode2D.Impulse);
                // Applique la vitesse souhaitée de saut pour éviter des sauts trop puissants
                rb2d.velocity = new Vector3(rb2d.velocity.x, 10f, rb2d.velocity.y);
                // Le personnage ne peut plus sauter
                doubleJumpReady = false;
            }
            // Si le personnage a sauté
            if (hasJumped && !wallSliding)
            {
                // Et que le bouton de saut est relaché
                if (Input.GetKeyUp(jump))
                {
                    // Alors il est prêt à faire le second saut
                    doubleJumpReady = true;
                    hasJumped = false;
                }
            }
        }

        // On applique un drag horizontal
        var vel = rb2d.velocity;
        vel.x *= 1.0f - drag;
        rb2d.velocity = vel;
    }

    void HandleWallSliding ()
    {
        rb2d.velocity = new Vector2(rb2d.velocity.x, -speed / 8);

        if (Input.GetKey(jump))
        {
            if (facingRight)
            {
                rb2d.AddForce(new Vector2(-20, speed));
                rb2d.velocity = new Vector3(rb2d.velocity.x, 10f, rb2d.velocity.y);
            }
            else
            {
                rb2d.AddForce(new Vector2(20, speed));
                rb2d.velocity = new Vector3(rb2d.velocity.x, 10f, rb2d.velocity.y);
            }
        }
    }

    private void Flip ()
    {
        Vector3 theScale = transform.localScale;

        if (facingRight)
        {
            theScale.x = 1;
        }
        else
        {
            theScale.x = -1;
        }

        transform.localScale = theScale;
    }

    // Co-routine permettant de préparer le début d'un niveau
    IEnumerator WaitForReady()
    {
        yield return new WaitForSeconds(1);
        Ready();

        // Et on réinitialise les timers
        if (currentScene == 1)
            timers.ResetTotalTimer(Time.time);
        
        // On reset le timer du niveau
        timers.ResetLevelTimer(Time.time);

        // Et active les deux timers
        timers.timerOn = true;
    }
}
