// using UnityEngine;

// public class enemyController : MonoBehaviour
// {
//     // Movement
//     public float TrueSpeed = 8f;
//     public float desiredSpeed;

//     private NavMeshAgent navMeshAgent;
//     private Transform thisPlayer;
//     private Rigidbody playerRigidbody;
    
//     void Start()
//     {
//         navMeshAgent = GetComponent<NavMeshAgent>();
//         GameObject playerObj = GameObject.FindWithTag("Player");
        

//         if (playerObj == null || navMeshAgent == null)
//         {
//             enabled = false;
//             return;
//         }

//         thisPlayer = playerObj.transform;
//         playerRigidbody = playerObj.GetComponent<Rigidbody>();

//         desiredSpeed = TrueSpeed;
//         navMeshAgent.speed = desiredSpeed;

        
//     }

//     void Update()
//     {
//         if (thisPlayer == null || navMeshAgent == null)
//         {
//             Vector2 target = thisPlayer;
//             navMeshAgent.SetDestination(target);
//             navMeshAgent.speed = desiredSpeed;
//         } else
//         {
//             return;
//         }
//     }
// }
