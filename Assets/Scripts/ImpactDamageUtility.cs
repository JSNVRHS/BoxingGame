using UnityEngine;

public static class ImpactDamageUtility
{
    public struct HandImpactInfo
    {
        public bool isPunching;
        public bool isHeadPunch;
        public bool isBodyPunch;
        public bool reachedPeakRotation;
    }

    public struct ImpactBreakdown
    {
        public int score;
        public int damage;
        public bool reachedPeakRotation;
        public bool defenderAttacking;
        public bool defenderWalkingToward;
        public bool attackerWalkingToward;
    }

    public static bool TryGetHandImpactInfo(Collider2D hit, out HandImpactInfo info)
    {
        info = default;

        if (hit == null)
        {
            return false;
        }

        RightHand rightHand = hit.GetComponent<RightHand>();
        if (rightHand != null)
        {
            info.isPunching = rightHand.IsPunching;
            info.isHeadPunch = rightHand.IsHeadPunch;
            info.isBodyPunch = rightHand.IsBodyPunch;
            info.reachedPeakRotation = rightHand.HasReachedPeakRotationThisPunch;
            return true;
        }

        LeftHand leftHand = hit.GetComponent<LeftHand>();
        if (leftHand != null)
        {
            info.isPunching = leftHand.IsPunching;
            info.isHeadPunch = leftHand.IsHeadPunch;
            info.isBodyPunch = leftHand.IsBodyPunch;
            info.reachedPeakRotation = leftHand.HasReachedPeakRotationThisPunch;
            return true;
        }

        BrawlerRightHand brawlerRightHand = hit.GetComponent<BrawlerRightHand>();
        if (brawlerRightHand != null)
        {
            info.isPunching = brawlerRightHand.IsPunching;
            info.isHeadPunch = brawlerRightHand.IsHeadPunch;
            info.isBodyPunch = brawlerRightHand.IsBodyPunch;
            info.reachedPeakRotation = brawlerRightHand.HasReachedPeakRotationThisPunch;
            return true;
        }

        BrawlerLeftHand brawlerLeftHand = hit.GetComponent<BrawlerLeftHand>();
        if (brawlerLeftHand != null)
        {
            info.isPunching = brawlerLeftHand.IsPunching;
            info.isHeadPunch = brawlerLeftHand.IsHeadPunch;
            info.isBodyPunch = brawlerLeftHand.IsBodyPunch;
            info.reachedPeakRotation = brawlerLeftHand.HasReachedPeakRotationThisPunch;
            return true;
        }

        return false;
    }

    public static ImpactBreakdown CalculateImpactBreakdown(
        bool reachedPeakRotation,
        Torso defenderTorso,
        Transform defenderRoot,
        Transform attackerRoot
    )
    {
        Torso attackerTorso = attackerRoot != null ? attackerRoot.GetComponentInChildren<Torso>(true) : null;

        ImpactBreakdown breakdown = new ImpactBreakdown
        {
            reachedPeakRotation = attackerTorso != null
                ? attackerTorso.HasReachedRotationCapThisPunch()
                : reachedPeakRotation,
            defenderAttacking = defenderTorso != null && defenderTorso.IsPunchingActive(),
            defenderWalkingToward = IsWalkingToward(defenderRoot, attackerRoot),
            attackerWalkingToward = IsWalkingToward(attackerRoot, defenderRoot)
        };

        if (breakdown.reachedPeakRotation)
        {
            breakdown.score += 1;
        }

        if (breakdown.defenderAttacking)
        {
            breakdown.score += 1;
        }

        if (breakdown.defenderWalkingToward)
        {
            breakdown.score += 1;
        }

        if (breakdown.attackerWalkingToward)
        {
            breakdown.score += 1;
        }

        breakdown.damage = breakdown.score >= 4 ? 5 : breakdown.score;
        return breakdown;
    }

    public static string FormatReasons(ImpactBreakdown breakdown)
    {
        return
            $"peakRotation={(breakdown.reachedPeakRotation ? 1 : 0)}, " +
            $"defenderAttacking={(breakdown.defenderAttacking ? 1 : 0)}, " +
            $"defenderWalkingToward={(breakdown.defenderWalkingToward ? 1 : 0)}, " +
            $"attackerWalkingToward={(breakdown.attackerWalkingToward ? 1 : 0)}";
    }

    static bool IsWalkingToward(Transform defenderRoot, Transform attackerRoot)
    {
        if (defenderRoot == null || attackerRoot == null)
        {
            return false;
        }

        Movement movement = defenderRoot.GetComponentInChildren<Movement>(true);
        if (movement == null || movement.body == null)
        {
            return false;
        }

        Vector2 velocity = movement.body.linearVelocity;
        if (velocity.sqrMagnitude <= 0.01f)
        {
            return false;
        }

        Vector2 towardAttacker = ((Vector2)attackerRoot.position - (Vector2)defenderRoot.position).normalized;
        return Vector2.Dot(velocity.normalized, towardAttacker) > 0.25f;
    }
}
