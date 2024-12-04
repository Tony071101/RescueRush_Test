using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndCheckPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")){
            GameManager.Instance.ChangeState(GameManager.GameState.GameEnd);
        }
    }
}
