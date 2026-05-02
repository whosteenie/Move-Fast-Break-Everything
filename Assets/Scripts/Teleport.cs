// using System.Collections;
// using UnityEngine;

// public class Teleport : MonoBehaviour
// {
//     [Header("Config")]
//     public MovementSO teleportSO;
//     public KeyCode teleportKey = KeyCode.T;

//     [Header("Cooldown")]
//     public float cooldown = 3f;

//     [Header("References")]
//     public Rigidbody2D rb;
//     public MovementStateMachine movementStateMachine;
//     public TestMovement testMovement;

//     private bool IsWindingUp => movementStateMachine.HasState(MovementStateMachine.State.teleportWindup);
//     private bool IsWindingDown => movementStateMachine.HasState(MovementStateMachine.State.teleportWinddown);
//     private bool OnCooldown => movementStateMachine.HasState(MovementStateMachine.State.teleportCooldown);

//     private bool CanTeleport => !IsWindingUp && !IsWindingDown && !OnCooldown
//                                 && !movementStateMachine.HasState();

//     private void Update()
//     {
//         if (Input.GetKeyDown(teleportKey) && CanTeleport)
//             StartCoroutine(TeleportSequence());
//     }

//     private IEnumerator TeleportSequence()
//     {
//         // windup — player is frozen
//         movementStateMachine.AddStateRaw(MovementStateMachine.State.teleportWindup);
//         yield return new WaitForSeconds(teleportSO.moveTimeLength);
//         movementStateMachine.RemoveState(MovementStateMachine.State.teleportWindup);

//         // blink
//         Vector2 facing = testMovement.GetFacing();
//         float distance = teleportSO.movePower;
//         rb.MovePosition(rb.position + facing * distance);

//         // winddown — player is frozen
//         movementStateMachine.AddStateRaw(MovementStateMachine.State.teleportWinddown);
//         yield return new WaitForSeconds(teleportSO.decayTimeLength);
//         movementStateMachine.RemoveState(MovementStateMachine.State.teleportWinddown);

//         // cooldown
//         movementStateMachine.AddStateRaw(MovementStateMachine.State.teleportCooldown);
//         yield return new WaitForSeconds(cooldown);
//         movementStateMachine.RemoveState(MovementStateMachine.State.teleportCooldown);
//     }
// }