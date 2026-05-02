using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementStateMachine : MonoBehaviour
{
    public enum State
    {
        dash,
        dashDecay,
        slide,
        slideDecay,
        jump,
        none,
        idle,
        charge,
        chargeDecay,
        slideJump,
        slideJumpDecay,
        slideDash,
        slideDashDecay,
        grappleFiring,
        grappled,
        grappleOrbiting,
        grappleWhipping,
        grappleCooldown,
    }

    private List<State> stateList;

    private void Start()
    {
        stateList = new List<State>();
    }

    public bool HasState(State state)
    {
        for (int i = 0; i < stateList.Count; i++)
        {
            if (stateList[i] == state)
                return true;
        }
        return false;
    }

    public void AddState(MovementSO movementSO)
    {
        stateList.Add(movementSO.moveState);
        StartCoroutine(RemoveStateTimed(movementSO.moveState, movementSO.moveTimeLength));

        if (movementSO.hasDecay)
        {
            StartCoroutine(AddStateTimed(movementSO.decayState, movementSO.moveTimeLength));
            StartCoroutine(RemoveStateTimed(movementSO.decayState, movementSO.decayTimeLength + movementSO.moveTimeLength));
        }
    }

    public void AddComboState(MovementSO movementSO, State ingredient1, State ingredient2)
    {
        stateList.Add(movementSO.moveState);
        RemoveState(ingredient1);
        RemoveState(ingredient2);
        StartCoroutine(RemoveStateTimed(movementSO.moveState, movementSO.moveTimeLength));

        if (movementSO.hasDecay)
        {
            StartCoroutine(AddStateTimed(movementSO.decayState, movementSO.moveTimeLength));
            StartCoroutine(RemoveStateTimed(movementSO.decayState, movementSO.decayTimeLength + movementSO.moveTimeLength));
        }
    }

    // Needed to make grapple possible, as we don't want timers on these states (so no movement SO)
    public void AddStateNoTimer(State state)
    {
        if (!HasState(state))
            stateList.Add(state);
    }

    public void RemoveState(State state)
    {
        stateList.Remove(state);
    }

    private IEnumerator AddStateTimed(State state, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        stateList.Add(state);
    }

    private IEnumerator RemoveStateTimed(State state, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        stateList.Remove(state);
    }
}