using UnityEngine;
using System.Collections.Generic;

public class FormationDirector : MonoBehaviour
{
    public void ApplyFormation(List<BaseRobotAI> loyalBots, Vector3 dangerCenter, FormationData activeFormation, Transform leaderTransform)
    {
        // Strongest in the front
        loyalBots.Sort((a, b) =>
        {
            int healthTierA = a.Health.CurrentHealth / 25;
            int healthTierB = b.Health.CurrentHealth / 25;
            int tierCompare = healthTierB.CompareTo(healthTierA);

            if (tierCompare == 0) return a.gameObject.GetEntityId().CompareTo(b.gameObject.GetEntityId());
            return tierCompare;
        });

        Vector3 dirToDanger = (dangerCenter - leaderTransform.position);
        dirToDanger.y = 0;
        dirToDanger.Normalize();

        Vector3 rightLine = Vector3.Cross(Vector3.up, dirToDanger);
        Vector3 centerAnchor = leaderTransform.position + (dirToDanger * (activeFormation.ForwardOffset + (activeFormation.Spacing * 2.5f)));

        for (int i = 0; i < loyalBots.Count; i++)
        {
            BaseRobotAI bot = loyalBots[i];

            // Calculate waypoints based on shape
            if (activeFormation.Shape == FormationData.FormationShape.Wall)
            {
                float sideMultiplier = (i % 2 == 0) ? -1f : 1f;
                float step = Mathf.Ceil(i / 2f);
                float offsetAmount = step * sideMultiplier * activeFormation.Spacing;
                if (i == 0) offsetAmount = 0f;
                bot.TacticalWaypoint = centerAnchor + (rightLine * offsetAmount);
            }
            else if (activeFormation.Shape == FormationData.FormationShape.Spear)
            {
                int rank = Mathf.CeilToInt(i / 2f);
                float sideMultiplier = (i % 2 != 0) ? 1f : -1f;
                if (i == 0) sideMultiplier = 0f;

                Vector3 backwardOffset = -dirToDanger * (rank * activeFormation.Spacing);
                Vector3 lateralOffset = rightLine * (rank * sideMultiplier * activeFormation.Spacing);
                bot.TacticalWaypoint = centerAnchor + backwardOffset + lateralOffset;
            }
            else if (activeFormation.Shape == FormationData.FormationShape.Box)
            {
                float angle = i * (Mathf.PI * 2f / loyalBots.Count);
                Vector3 targetForward = dirToDanger * Mathf.Cos(angle);
                Vector3 targetRight = rightLine * Mathf.Sin(angle);
                Vector3 circleOffset = (targetForward + targetRight) * activeFormation.Spacing;
                bot.TacticalWaypoint = leaderTransform.position + circleOffset;
            }

            // Push waypoint away from danger if holding attack
            if (bot.HoldAttack)
            {
                Vector3 pushAway = (bot.TacticalWaypoint - dangerCenter).normalized;
                pushAway.y = 0;
                bot.TacticalWaypoint += pushAway * (activeFormation.Spacing * 1.5f);
            }
        }
    }

    public void SynchronizeSpeeds(List<BaseRobotAI> loyalBots, float baseSpeed, IRobotMover leaderMover, Vector3 leaderWaypoint, Transform leaderTransform, float breakDistance)
    {
        float maxLagDistance = 0f;
        foreach (BaseRobotAI bot in loyalBots)
        {
            float dist = Vector3.Distance(bot.transform.position, bot.TacticalWaypoint);
            if (dist > maxLagDistance) maxLagDistance = dist;
        }

        float leaderLag = Vector3.Distance(leaderTransform.position, leaderWaypoint);
        float leaderSyncSpeed = baseSpeed;

        // Adjust leader speed
        if (maxLagDistance > breakDistance * 1.5f) leaderSyncSpeed = 0f;
        else if (maxLagDistance > 2f)
        {
            float speedMultiplier = Mathf.Clamp01((leaderLag + 1f) / (maxLagDistance + 0.1f));
            leaderSyncSpeed = Mathf.Max(baseSpeed * speedMultiplier, baseSpeed * 0.2f);
        }

        leaderMover.SetSpeed(leaderSyncSpeed);

        // Adjust bot speed
        foreach (BaseRobotAI bot in loyalBots)
        {
            float botLag = Vector3.Distance(bot.transform.position, bot.TacticalWaypoint);
            float botSyncSpeed = bot.Stats.MoveSpeed;

            if (maxLagDistance > 2f)
            {
                float speedMultiplier = Mathf.Clamp01((botLag + 1f) / (maxLagDistance + 0.1f));
                botSyncSpeed = bot.Stats.MoveSpeed * speedMultiplier;
            }

            bot.Mover.SetSpeed(Mathf.Max(botSyncSpeed, bot.Stats.MoveSpeed * 0.2f));
        }
    }
}