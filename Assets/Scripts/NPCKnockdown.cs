using UnityEngine;

public class NPCKnockdown : MonoBehaviour
{
    public GameObject torso;
    public GameObject legs;
    public GameObject headPrefab;
    public GameObject knockdownSprite;
    [SerializeField] float rotationOffset = 0f;

    public void Knockdown()
    {
        if (knockdownSprite != null)
        {
            knockdownSprite.transform.position = headPrefab != null
                ? headPrefab.transform.position
                : transform.position;
            Quaternion sourceRotation = headPrefab != null
                ? headPrefab.transform.rotation
                : transform.rotation;
            knockdownSprite.transform.rotation =
                sourceRotation * Quaternion.Euler(0f, 0f, rotationOffset);
        }

        torso.SetActive(false);
        legs.SetActive(false);
        headPrefab.SetActive(false);
        knockdownSprite.SetActive(true);
    }
}
