using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;

public class MovingCube : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask catLayerMask;
    [SerializeField] private LayerMask playerLayerMask;
    private void Update() {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log($"Collided with: {other.gameObject.name}");
        if (IsInLayerMask(other.gameObject, catLayerMask)) {
            Destroy(other.gameObject);
        }

        if (IsInLayerMask(other.gameObject, playerLayerMask)) {
            GameManager.Instance.ChangeState(GameManager.GameState.GameEnd);
        }
    }

    //Check layer
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask) {
        return (layerMask.value & (1 << obj.layer)) > 0;
    }
}
