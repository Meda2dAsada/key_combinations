using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class CubeActions : MonoBehaviour
{
    [Header("Action sounds")]
    [SerializeField] private AudioClip villagerSound;
    //Si vamos
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip eatingSound;

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float jumpSpeed = 5f;
    [SerializeField, Min(0.1f)] private float secondJumpDelay = 0.25f;
    [SerializeField, Min(0.1f)] private float flipDuration = 0.8f;

    private Rigidbody body;
    private AudioSource audioSource;
    private Coroutine currentAction;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void DoubleJump()
    {
        BeginAction(DoubleJumpRoutine());
    }

    public void Punch()
    {
        BeginAction(PunchRoutine());
    }

    public void Backflip()
    {
        BeginAction(BackflipRoutine());
    }

    private void BeginAction(IEnumerator action)
    {
        if (!isActiveAndEnabled || currentAction != null) return;

        originalScale = transform.localScale;
        originalRotation = body.rotation;
        currentAction = StartCoroutine(action);
    }

    private void Jump()
    {
        Vector3 velocity = body.linearVelocity;
        velocity.y = jumpSpeed;
        body.linearVelocity = velocity;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    private IEnumerator DoubleJumpRoutine()
    {
        Jump();
        PlaySound(villagerSound);
        yield return new WaitForSeconds(secondJumpDelay);
        // A second upward impulse while the cube is still in the air.
        Jump();
        PlaySound(villagerSound);
        yield return new WaitForSeconds(secondJumpDelay);
        currentAction = null;
    }

    private IEnumerator PunchRoutine()
    {
        PlaySound(explosionSound);
        const float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Stretch forward quickly, then retract to suggest a punch.
            float progress = elapsed / duration;
            float extension = progress < 0.25f
                ? progress / 0.25f
                : (1f - progress) / 0.75f;
            transform.localScale = Vector3.Scale(originalScale,
                new Vector3(1f - 0.2f * extension, 1f - 0.2f * extension, 1f + extension));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        currentAction = null;
    }

    private IEnumerator BackflipRoutine()
    {
        Jump();
        PlaySound(eatingSound);
        float elapsed = 0f;
        var physicsStep = new WaitForFixedUpdate();
        while (elapsed < flipDuration)
        {
            yield return physicsStep;
            elapsed += Time.fixedDeltaTime;
            body.angularVelocity = Vector3.zero;
            float angle = -360f * Mathf.Clamp01(elapsed / flipDuration);
            body.MoveRotation(originalRotation * Quaternion.Euler(angle, 0f, 0f));
        }
        body.rotation = originalRotation;
        body.angularVelocity = Vector3.zero;
        currentAction = null;
    }

    private void OnDisable()
    {
        if (currentAction == null) return;

        StopCoroutine(currentAction);
        transform.localScale = originalScale;
        body.rotation = originalRotation;
        body.angularVelocity = Vector3.zero;
        currentAction = null;
    }
}
