using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; 

public class EntryCutscene : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;            
    public Animator playerAnimator;       
    public PlayerInput playerInput;       
    public Transform startPoint;
    public Transform endPoint;            
    public float walkSpeed = 3f;
    public float arriveDistance = 0.05f;

    [Header("Fade (optional)")]
    public FadeScreen fader;              
    public float prePause = 0.25f;        
    public float postPause = 0.25f;       
    public float fadeInDuration = 0.8f;   

    [Header("Animator Params (pick what you use)")]
    public string speedParam = "Speed";  
    public string isWalkingParam = "isWalking";
    public string moveXParam = "MoveX";   
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
        // fade-in from black
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

        // Re enable controls
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
