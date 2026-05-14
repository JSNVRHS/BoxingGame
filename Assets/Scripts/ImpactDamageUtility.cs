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
        public bool debugDamageOverride;
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
        Torso attackerTorso = null;
        if (attackerRoot != null)
        {
            attackerTorso = attackerRoot.GetComponentInChildren<Torso>(true);
        }

        ImpactBreakdown breakdown = new ImpactBreakdown
        {
            defenderAttacking = defenderTorso != null && defenderTorso.IsPunchingActive(),
            defenderWalkingToward = IsWalkingToward(defenderRoot, attackerRoot),
            attackerWalkingToward = IsWalkingToward(attackerRoot, defenderRoot)
        };

        if (attackerTorso != null)
        {
            breakdown.reachedPeakRotation = attackerTorso.HasReachedRotationCapThisPunch();
        }
        else
        {
            breakdown.reachedPeakRotation = reachedPeakRotation;
        }

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

        if (breakdown.score >= 4)
        {
            breakdown.damage = 5;
        }
        else
        {
            breakdown.damage = breakdown.score;
        }

        SimpleOSNpcAI npcAi = null;
        if (attackerRoot != null)
        {
            npcAi = attackerRoot.GetComponentInChildren<SimpleOSNpcAI>(true);
        }
        if (npcAi != null && npcAi.TryGetOverridePunchDamage(out int overrideDamage))
        {
            breakdown.debugDamageOverride = true;
            breakdown.score = overrideDamage;
            breakdown.damage = overrideDamage;
        }

        return breakdown;
    }

    public static string FormatReasons(ImpactBreakdown breakdown)
    {
        int peakRotation = 0;
        if (breakdown.reachedPeakRotation)
        {
            peakRotation = 1;
        }

        int defenderAttacking = 0;
        if (breakdown.defenderAttacking)
        {
            defenderAttacking = 1;
        }

        int defenderWalkingToward = 0;
        if (breakdown.defenderWalkingToward)
        {
            defenderWalkingToward = 1;
        }

        int attackerWalkingToward = 0;
        if (breakdown.attackerWalkingToward)
        {
            attackerWalkingToward = 1;
        }

        int debugDamageOverride = 0;
        if (breakdown.debugDamageOverride)
        {
            debugDamageOverride = 1;
        }

        return
            $"peakRotation={peakRotation}, " +
            $"defenderAttacking={defenderAttacking}, " +
            $"defenderWalkingToward={defenderWalkingToward}, " +
            $"attackerWalkingToward={attackerWalkingToward}, " +
            $"debugDamageOverride={debugDamageOverride}";
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
