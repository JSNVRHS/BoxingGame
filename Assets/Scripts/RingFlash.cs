using UnityEngine;

public class RingFlash : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite hitSprite;
    SpriteRenderer sr;

    [SerializeField] float flashDuration = 0.1f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Flash()
    {
        sr.sprite = hitSprite;
        Invoke("ResetSprite", flashDuration);
    }

    public void StayHit()
    {
        CancelInvoke();
        sr.sprite = hitSprite;
    }

    void ResetSprite()
    {
        sr.sprite = normalSprite;
    }
}