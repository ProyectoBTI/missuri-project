using UnityEngine;

public class OfficeBuilder : MonoBehaviour
{
    // Variables ajustables desde el Inspector
    [Header("Room Settings")]
    public float roomWidth = 10f;
    public float roomLength = 10f;
    public float roomHeight = 4f;
    public float wallThickness = 0.1f;

    [Header("Furniture Settings")]
    public float deskWidth = 2f;
    public float deskLength = 1f;
    public float deskHeight = 0.1f;

    [Header("Colors")]
    public Color floorColor = new Color(0.3f, 0.3f, 0.3f);
    public Color wallColor = Color.white;
    public Color deskColor = new Color(0.4f, 0.2f, 0.1f);

    void Start()
    {
        CreateOffice();
    }

    void CreateOffice()
    {
        CreateFloor();
        CreateWalls();
        CreateDesk();
        CreateChair();
        CreateLight();
        CreateCamera();
    }

    void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0, 0, 0);
        floor.transform.localScale = new Vector3(roomWidth, wallThickness, roomLength);

        // Aplicar material
        SetObjectColor(floor, floorColor);
    }

    void CreateWalls()
    {
        // Pared frontal
        CreateWall(new Vector3(0, roomHeight / 2, -roomLength / 2), new Vector3(roomWidth, roomHeight, wallThickness), "Wall_Front");

        // Pared trasera
        CreateWall(new Vector3(0, roomHeight / 2, roomLength / 2), new Vector3(roomWidth, roomHeight, wallThickness), "Wall_Back");

        // Pared izquierda
        CreateWall(new Vector3(-roomWidth / 2, roomHeight / 2, 0), new Vector3(wallThickness, roomHeight, roomLength), "Wall_Left");

        // Pared derecha
        CreateWall(new Vector3(roomWidth / 2, roomHeight / 2, 0), new Vector3(wallThickness, roomHeight, roomLength), "Wall_Right");
    }

    void CreateWall(Vector3 position, Vector3 scale, string name)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        SetObjectColor(wall, wallColor);
    }

    void CreateDesk()
    {
        GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "Desk";
        desk.transform.position = new Vector3(0, deskHeight / 2 + wallThickness, 0);
        desk.transform.localScale = new Vector3(deskWidth, deskHeight, deskLength);
        SetObjectColor(desk, deskColor);
    }

    void CreateChair()
    {
        // Asiento
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = "Chair_Seat";
        seat.transform.position = new Vector3(0, 0.5f, 1f);
        seat.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        SetObjectColor(seat, deskColor);

        // Respaldo
        GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        back.name = "Chair_Back";
        back.transform.position = new Vector3(0, 1f, 0.75f);
        back.transform.localScale = new Vector3(0.5f, 0.5f, 0.1f);
        SetObjectColor(back, deskColor);
    }

    void CreateLight()
    {
        GameObject lightObj = new GameObject("Office_Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = 1.5f;
        lightObj.transform.position = new Vector3(0, roomHeight - 0.5f, 0);
    }

    void CreateCamera()
    {
        GameObject cameraObj = new GameObject("Main_Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.transform.position = new Vector3(0, roomHeight / 2, -roomLength);
        cameraObj.transform.rotation = Quaternion.Euler(15, 0, 0);
    }

    void SetObjectColor(GameObject obj, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        obj.GetComponent<Renderer>().material = material;
    }
}