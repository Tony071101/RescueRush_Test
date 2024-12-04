using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;

public class MovingCube : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private void Update() {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}
