using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // for PlayerInput (new input system)

public class EntryCutscene : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;              // your player in this scene
    public Animator playerAnimator;       // animator on the player
    public PlayerInput playerInput;       // PlayerInput component on the player
    public Transform startPoint;          // where the player appears (e.g., doorway)
    public Transform endPoint;            // where they walk to (e.g., inside room)
    public float walkSpeed = 3f;
    public float arriveDistance = 0.05f;

    [Header("Fade (optional)")]
    public FadeScreen fader;              // drag your FadeScreen if you want fade-in/out
    public float prePause = 0.25f;        // pause before moving
    public float postPause = 0.25f;       // pause after arriving
    public float fadeInDuration = 0.8f;   // if you want fade-in when scene starts

    [Header("Animator Params (pick what you use)")]
    public string speedParam = "Speed";   // or leave empty if you use isWalking bool
    public string isWalkingParam = "isWalking";
    public string moveXParam = "MoveX";   // for 4-dir anims
    public string moveYParam = "MoveY";

    void Start()
    {
        // Position player at start and disable control
        if (player && startPoint) player.position = startPoint.position;
        if (playerInput) playerInput.enabled = false;

        StartCoroutine(DoCutscene());
    }

    IEnumerator DoCutscene()
    {
        // Optional fade-in from black
        if (fader) yield return fader.FadeIn(fadeInDuration);

        yield return new WaitForSecondsRealtime(prePause);

        // Walk toward endPoint
        if (player && endPoint)
        {
            Vector2 dir = ((Vector2)endPoint.position - (Vector2)player.position).normalized;
            DriveAnimator(dir, walk: true);

            while (Vector2.Distance(player.position, endPoint.position) > arriveDistance)
            {
                player.position = Vector2.MoveTowards(player.position, endPoint.position, walkSpeed * Time.deltaTime);
                yield return null;
            }

            DriveAnimator(Vector2.zero, walk: false);
        }

        yield return new WaitForSecondsRealtime(postPause);

        // Re-enable controls
        if (playerInput) playerInput.enabled = true;
    }

    void DriveAnimator(Vector2 dir, bool walk)
    {
        if (playerAnimator == null) return;

        if (!string.IsNullOrEmpty(speedParam))
            playerAnimator.SetFloat(speedParam, walk ? 1f : 0f);

        if (!string.IsNullOrEmpty(isWalkingParam))
            playerAnimator.SetBool(isWalkingParam, walk);

        if (!string.IsNullOrEmpty(moveXParam))
            playerAnimator.SetFloat(moveXParam, dir.x);

        if (!string.IsNullOrEmpty(moveYParam))
            playerAnimator.SetFloat(moveYParam, dir.y);
    }
}
