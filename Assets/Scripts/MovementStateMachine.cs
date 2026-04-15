using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementStateMachine : MonoBehaviour
{
    public enum State
    {
        dash,
        dashDecay,
        jump,
        none,
        idle,
    }

    public List<State> stateList;
    
    //
    //loops through everything to find if there's something in the statelist
    public bool HasState(State state)
    {
        for(int i = 0; i < stateList.Count; i++)
        {
            if(stateList[i] == state)
            {
                return true;
            }
        }
        return false;
    }

    //addState
    public void AddState(MovementSO movementSO)
    {
        //First add the state to the list
        stateList.Add(movementSO.moveState);
        
        //Remove it after moveTimeLength
        RemoveState(movementSO.moveState, movementSO.moveTimeLength);

        //If it has a decay state
        if (movementSO.hasDecay)
        {
            // add it to the state list
            stateList.Add(movementSO.decayState);

            //remove it after decayTimeLength
            RemoveState(movementSO.decayState, movementSO.decayTimeLength);
        } 
    }

    //removeState
    IEnumerator RemoveState(State state, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        stateList.Remove(state);
    }
}
