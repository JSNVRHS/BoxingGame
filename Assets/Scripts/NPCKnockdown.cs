using UnityEngine;

public class NPCKnockdown : MonoBehaviour
{
    public GameObject torso;
    public GameObject legs;
    public GameObject headPrefab;
    public GameObject knockdownSprite;

    public void Knockdown()
    {
        torso.SetActive(false);
        legs.SetActive(false);
        headPrefab.SetActive(false);
        knockdownSprite.SetActive(true);
    }
}