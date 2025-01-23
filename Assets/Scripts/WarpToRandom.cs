using UnityEngine;

public class WarpToRandom : MonoBehaviour
{
    public float targetTime = 20.0f;
    public float areaX = 10;
    public float areaY = 10;
    public Vector3 newPosition;
    private void Start()
    {
        getNewPosition();
    }
    void Update()
    {
        targetTime -= Time.deltaTime;
        if (targetTime <= 0.0f)
        {
            getNewPosition();
            gameObject.transform.position = newPosition;
            targetTime = 15.0f;
        }
    }
    private Vector3 getNewPosition()
    {
        newPosition = new Vector3(Random.Range(-areaX, areaX), 0f, Random.Range(-areaY, areaY));
        return newPosition;
    }
}
