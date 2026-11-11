using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;                 // leave empty: found in Awake
    [SerializeField] Vector3 offset = new Vector3(0f, 10f, -6f);
    [SerializeField] float smoothTime = 0.25f;         // s
    [SerializeField] float lookAhead = 0.35f;          // s of velocity
    [SerializeField] float dipDepth = 0.3f;            // m
    [SerializeField] float dipDuration = 0.35f;        // s

    PlayerController player;
    Rigidbody targetBody;
    Vector3 focus, focusVelocity, dipOffset;
    Coroutine dip;

    void Awake()
    {
        player = FindAnyObjectByType<PlayerController>();
        target = player.transform;
        targetBody = player.GetComponent<Rigidbody>();
        focus = target.position;
    }

    void OnEnable()  { if (player != null) player.Landed += OnLanded; }
    void OnDisable() { if (player != null) player.Landed -= OnLanded; }

    void OnLanded(float airTime)
    {
        if (dip != null) StopCoroutine(dip);   // stop a running dip first
        dip = StartCoroutine(Dip());
    }

    IEnumerator Dip()
    {
        for (float elapsed = 0f; elapsed < dipDuration; elapsed += Time.deltaTime)
        {
            float p = Ease.OutBack(elapsed / dipDuration);
            // LerpUnclamped, so the overshoot isn't cut flat
            dipOffset = Vector3.LerpUnclamped(Vector3.down * dipDepth, Vector3.zero, p);
            yield return null;
        }
        dipOffset = Vector3.zero;   // exactly at rest
        dip = null;
    }

    void LateUpdate()
    {
        Vector3 flatVelocity = targetBody.linearVelocity;
        flatVelocity.y = 0f;
        Vector3 aim = target.position + flatVelocity * lookAhead;   // look-ahead

        focus = Vector3.SmoothDamp(focus, aim, ref focusVelocity, smoothTime);

        transform.position = focus + offset + dipOffset;
        transform.rotation = Quaternion.LookRotation(-offset);      // looks at the focus point
    }
}