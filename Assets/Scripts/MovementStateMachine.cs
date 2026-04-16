using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    }

    private List<State> stateList;

    private void Start()
    {
        stateList = new List<State>();
    }

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
        StartCoroutine(RemoveState(movementSO.moveState, movementSO.moveTimeLength));

        //If it has a decay state
        if (movementSO.hasDecay)
        {
            // add it to the state list
            StartCoroutine(AddStateDumb(movementSO.decayState, movementSO.moveTimeLength));

            //remove it after decayTimeLength
            StartCoroutine(RemoveState(movementSO.decayState, movementSO.decayTimeLength+movementSO.moveTimeLength));
        } 
    }

    private IEnumerator AddStateDumb(State state, float waitTime)
    {
       yield return new WaitForSeconds(waitTime);
       stateList.Add(state); 
    }

    //removeState
    private IEnumerator RemoveState(State state, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        stateList.Remove(state);
    }
}
