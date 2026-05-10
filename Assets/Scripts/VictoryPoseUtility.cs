using UnityEngine;

public static class VictoryPoseUtility
{
    public static void ShowVictoryPose(Transform victorRoot)
    {
        if (victorRoot == null)
        {
            return;
        }

        GameObject victoryPose = FindNamedChild(victorRoot, "victoryPose");
        if (victoryPose == null)
        {
            return;
        }

        DisableSpriteRenderersNamed(victorRoot, "headPrefab");
        DisableSpriteRenderersNamed(victorRoot, "torsoSprite");
        DisableSpriteRenderersNamed(victorRoot, "bruisedTorso");
        DisableSpriteRenderersNamed(victorRoot, "bruisedTorsoSprite");
        DisableSpriteRenderersNamed(victorRoot, "bruisedFace");
        DisableSpriteRenderersNamed(victorRoot, "leftShoulder");
        DisableSpriteRenderersNamed(victorRoot, "rightShoulder");
        DisableSpriteRenderersNamed(victorRoot, "leftHand");
        DisableSpriteRenderersNamed(victorRoot, "rightHand");

        victoryPose.SetActive(true);
    }

    static void DisableSpriteRenderersNamed(Transform root, string objectName)
    {
        Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform descendant in descendants)
        {
            if (descendant.name != objectName)
            {
                continue;
            }

            SpriteRenderer spriteRenderer = descendant.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }
    }

    static GameObject FindNamedChild(Transform root, string childName)
    {
        Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform descendant in descendants)
        {
            if (descendant != root && descendant.name == childName)
            {
                return descendant.gameObject;
            }
        }

        return null;
    }
}
