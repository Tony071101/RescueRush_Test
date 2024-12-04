using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2CheckPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")){
            GameManager.Instance.ChangeState(GameManager.GameState.Phase_Two);
        }
    }
}
