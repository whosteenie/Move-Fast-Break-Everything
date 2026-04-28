using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu()]
public class MovementSO : ScriptableObject
{
    public string moveName;
    public MovementStateMachine.State moveState;
    public float baseMoveTimeLength;
    public float moveTimeLength;
    public float movePower;
    public float healthScale;
    public float defenseScale;
    public float strengthScale;
    public float agilityScale;
    public float dexterityScale;
    public float luckScale;
    public bool hasDecay;
    public MovementStateMachine.State decayState;
    public float baseDecayTimeLength;
    public float decayTimeLength;
}