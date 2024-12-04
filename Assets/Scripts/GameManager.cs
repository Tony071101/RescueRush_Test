using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Transform[] catLocations;
    [SerializeField] private GameObject[] catPrefabs;

    private void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
        }

        Instance = this;

        GenerateCats();
    }

    private void GenerateCats() {
        for (int i = 0; i < catLocations.Length; i++) {
            int s = Random.Range(0, catPrefabs.Length);

            Instantiate(catPrefabs[s], catLocations[i].position, Quaternion.identity);
        }
    }
}
