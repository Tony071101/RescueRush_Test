
using UnityEngine;

public class ProceduralGenerator : MonoBehaviour
{
    public GameObject prefab;
    public int numberOfPrefabInstances = 200;
    public Vector3 generationAreaSize = new Vector3(100f, 1f, 100f);

    public Transform parentContainer;


    public float absoluteGroundLevel = 30f; // We set it to 30, because we initially set our terrain to have a height of 30

    void Start()
    {
        // If no parentContainer provided, instances will be generated as children of the generator.
        if (parentContainer == null)
        {
            parentContainer = transform.root;
        }

        gameObject.transform.position = new Vector3(gameObject.transform.position.x, absoluteGroundLevel, gameObject.transform.position.z);

        Generate();
    }

    void Generate()
    {
        for (int i = 0; i < numberOfPrefabInstances; i++)
        {
            Vector3 randomPosition = GetRandomPositionInGenerationArea();
            Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            //Instantiate(prefab, randomPosition, randomRotation);
            Instantiate(prefab, randomPosition, randomRotation, parentContainer.transform);
        }
    }

    Vector3 GetRandomPositionInGenerationArea()
    {
        Vector3 randomPosition = new Vector3(
           Random.Range(-generationAreaSize.x / 2, generationAreaSize.x / 2),
           0f,
           Random.Range(-generationAreaSize.z / 2, generationAreaSize.z / 2)
       );

        // Optionally, you can adjust the Y coordinate based on terrain height or other criteria
        // For example:
        // randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);

        return transform.position + randomPosition;
    }

    // Draw Gizmo to visualize the generation area
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, generationAreaSize);
    }
}