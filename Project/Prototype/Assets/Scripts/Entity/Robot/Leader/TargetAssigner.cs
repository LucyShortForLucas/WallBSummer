using UnityEngine;
using System.Collections.Generic;

public class TargetAssigner : MonoBehaviour
{
    public void AssignEscorts(List<BaseRobotAI> escorts, Transform leaderTransform, Transform currentTarget)
    {
        for (int i = 0; i < escorts.Count; i++)
        {
            BaseRobotAI bodyguard = escorts[i];
            bodyguard.currentTarget = currentTarget;
            bodyguard.holdAttack = true;

            // Position bodyguards around leader
            float side = (i % 2 == 0) ? 1.5f : -1.5f;
            bodyguard.tacticalWaypoint = leaderTransform.position + (leaderTransform.forward * 1.5f) + (leaderTransform.right * side);

            if (bodyguard.CurrentState != bodyguard.Attack) bodyguard.ChangeState(bodyguard.Attack);
            bodyguard.Mover.SetSpeed(bodyguard.stats.moveSpeed);
        }
    }

    public void AssignProportionalTargets(List<BaseRobotAI> attackers, List<Transform> validThreats, Transform fallbackTarget)
    {
        // Fallback if no threats exist
        if (validThreats.Count == 0)
        {
            foreach (var bot in attackers) bot.currentTarget = fallbackTarget;
            return;
        }

        int[] allocations = new int[validThreats.Count];
        int remainingAttackers = attackers.Count;

        float totalDanger = 0f;
        foreach (Transform t in validThreats) totalDanger += t.GetComponent<DangerComponent>().CurrentDanger;

        // Allocate units per threat by danger ratio
        for (int i = 0; i < validThreats.Count; i++)
        {
            if (i == validThreats.Count - 1)
            {
                allocations[i] = remainingAttackers;
            }
            else
            {
                float ratio = validThreats[i].GetComponent<DangerComponent>().CurrentDanger / totalDanger;
                int share = Mathf.FloorToInt(ratio * attackers.Count);

                if (share == 0 && remainingAttackers > 0) share = 1;
                share = Mathf.Min(share, remainingAttackers);

                allocations[i] = share;
                remainingAttackers -= share;
            }
        }

        int currentThreatIndex = 0;
        int assignedToCurrent = 0;

        // Assign specific targets to attackers
        foreach (BaseRobotAI bot in attackers)
        {
            while (currentThreatIndex < validThreats.Count && assignedToCurrent >= allocations[currentThreatIndex])
            {
                currentThreatIndex++;
                assignedToCurrent = 0;
            }

            if (currentThreatIndex < validThreats.Count)
            {
                bot.currentTarget = validThreats[currentThreatIndex];
                assignedToCurrent++;
            }
            else
            {
                bot.currentTarget = validThreats[validThreats.Count - 1];
            }
        }
    }
}