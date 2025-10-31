using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EntryCutscene : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;               // player transform
    public Animator playerAnimator;        // same Animator used in gameplay
    public PlayerInput playerInput;        // the PlayerInput you normally use
    public Transform startPoint;
    public Transform endPoint;
    public float walkSpeed = 3f;
    public float arriveDistance = 0.05f;

    [Header("Fade (optional)")]
    public FadeScreen fader;
    public float prePause = 0.25f;
    public float postPause = 0.25f;
    public float fadeInDuration = 0.8f;

    void Start()
    {
        // put player at entry spot and turn off controls
        if (player && startPoint)
            player.position = startPoint.position;

        if (playerInput)
            playerInput.enabled = false;

        StartCoroutine(DoCutscene());
    }

    IEnumerator DoCutscene()
    {
        // fade from black
        if (fader)
            yield return fader.FadeIn(fadeInDuration);

        yield return new WaitForSecondsRealtime(prePause);

        if (player && endPoint)
        {
            // walk until close enough
            while (Vector2.Distance(player.position, endPoint.position) > arriveDistance)
            {
                // direction we are traveling THIS frame
                Vector2 dir = ((Vector2)endPoint.position - (Vector2)player.position).normalized;

                // drive animator exactly like PlayerMovement does
                DriveAnimatorDuringWalk(dir);

                // move player
                player.position = Vector2.MoveTowards(
                    player.position,
                    endPoint.position,
                    walkSpeed * Time.deltaTime
                );

                yield return null;
            }

            // stop walking anim and set final facing
            DriveAnimatorStop();
        }

        yield return new WaitForSecondsRealtime(postPause);

        // give control back to player
        if (playerInput)
            playerInput.enabled = true;
    }

    void DriveAnimatorDuringWalk(Vector2 dir)
    {
        if (!playerAnimator) return;

        // this matches Update() in PlayerMovement
        playerAnimator.SetBool("isWalking", true);
        playerAnimator.SetFloat("InputX", dir.x);
        playerAnimator.SetFloat("InputY", dir.y);

        // we do NOT touch LastInputX/Y yet while moving
    }

    void DriveAnimatorStop()
    {
        if (!playerAnimator) return;

        // stop walk
        playerAnimator.SetBool("isWalking", false);

        // lock the idle facing like your Move() callback does
        float ix = playerAnimator.GetFloat("InputX");
        float iy = playerAnimator.GetFloat("InputY");

        playerAnimator.SetFloat("LastInputX", ix);
        playerAnimator.SetFloat("LastInputY", iy);
    }
}