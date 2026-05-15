using UnityEngine;
public class Spawner : MonoBehaviour
{
    // Step 5 – Add variables for configuring the spawner properties
    [SerializeField] GameObject spawnObject;
    [SerializeField] float startTime = 0.0f;
    [SerializeField] float spawnEvery = 3.0f;
    // Step 6 – Add variables to define the boundaries
    [SerializeField] Vector2 spawnAreaMin;
    [SerializeField] Vector2 spawnAreaMax;
    Vector3 GetRandomPosition()
    {
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        return new Vector3(x, y, 0);
    }
    public void SpawnNew()
    {
        Vector3 spawnPos = GetRandomPosition();
        var newSpawn = Instantiate(spawnObject);
        newSpawn.transform.position = spawnPos;
        newSpawn.SetActive(true);
    }

    void Start()
    {
        InvokeRepeating("SpawnNew", startTime, spawnEvery);
    }
}

