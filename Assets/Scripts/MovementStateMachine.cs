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
        charge,
        chargeDecay,
        slideJump,
        slideJumpDecay,
        slideDash,
        slideDashDecay,
    }

    private List<State> stateList;

    private void Start()
    {
        stateList = new List<State>();
    }

    private void Update()
    {
        // Debug.Log(stateList[0]);
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
        // print("In add State");
        //First add the state to the list
        stateList.Add(movementSO.moveState);
        
        //Remove it after moveTimeLength
        
        StartCoroutine(RemoveStateTimed(movementSO.moveState, movementSO.moveTimeLength));
        // Debug.Log(stateList[0]);
        // Debug.Log(stateList[1]);
        //If it has a decay state
        if (movementSO.hasDecay)
        {
            // add it to the state list
            StartCoroutine(AddStateTimed(movementSO.decayState, movementSO.moveTimeLength));
            // Debug.Log(stateList[0]);
            //remove it after decayTimeLength
            StartCoroutine(RemoveStateTimed(movementSO.decayState, movementSO.decayTimeLength+movementSO.moveTimeLength));
            // Debug.Log(stateList[0]);
        } 
        // Debug.Log(stateList[0]);
        // Debug.Log(stateList[1]);
    }

    public void AddComboState(MovementSO movementSO, State ingredient1, State ingredient2)
    {
        //Add the comboState to the stateList
        //Remember to have this interfere with the component pieces, also only one combo per two moves
        stateList.Add(movementSO.moveState);

        //Remove all the components
        RemoveState(ingredient1);
        RemoveState(ingredient2);

        //Set up the timer to remove the combostate.
        //Remember it should be longer than both of them combined.
        //But should be powerful enough to make it pop and desirable.
        StartCoroutine(RemoveStateTimed(movementSO.moveState, movementSO.moveTimeLength));

        //If it has a decay state set it up 
        //to do things like slow you down, teleport you backwards, or a simpler cooldown
        if (movementSO.hasDecay)
        {
            // add the decayState to the state list
            StartCoroutine(AddStateTimed(movementSO.decayState, movementSO.moveTimeLength));
            // Debug.Log(stateList[0]);

            //remove it after decayTimeLength
            //although due to the annoying way unity does timers we actually do moveTimeLength+decayTimeLength
            StartCoroutine(RemoveStateTimed(movementSO.decayState, movementSO.decayTimeLength+movementSO.moveTimeLength));
            // Debug.Log(stateList[0]);
        } 
    }

    private IEnumerator AddStateTimed(State state, float waitTime)
    {
       yield return new WaitForSeconds(waitTime);
       stateList.Add(state); 
    }

    //removeState
    private IEnumerator RemoveStateTimed(State state, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        stateList.Remove(state); 
    }

    private void RemoveState(State state)
    {
        stateList.Remove(state); 
    }
}
