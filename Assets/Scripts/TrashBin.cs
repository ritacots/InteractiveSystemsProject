using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [Header("Lid Reference")]
    [Tooltip("Drag the 'TrashbinLid' child object here.")]
    public Transform lid;

    [Header("Lid Animation")]
    public float openAngle = -80f;
    public float animSpeed = 8f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;

    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private bool _isOpen = false;

    public void SetLidOpen(bool open)
    {
        if (open == _isOpen) return; 
        _isOpen = open;
        targetAngle = open ? openAngle : 0f;

        if (audioSource != null)
        {
            AudioClip clip = open ? openClip : closeClip;
            if (clip != null) { audioSource.clip = clip; audioSource.Play(); }
        }
    }

    void Update()
    {
        if (lid == null) return;
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * animSpeed);
        lid.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
    }
}