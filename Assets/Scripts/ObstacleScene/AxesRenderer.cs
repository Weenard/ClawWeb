using UnityEngine;

public class AxesRenderer : MonoBehaviour
{
    public float axisLength = 2f;
    public float arrowSize = 0.2f;

    private Transform axesRoot;

    void Start()
    {
        axesRoot = new GameObject("AxesRoot").transform;
        axesRoot.parent = transform;

        CreateAxis(Vector3.right, Color.red, "X");
        CreateAxis(Vector3.up, Color.green, "Y");
        CreateAxis(Vector3.forward, Color.blue, "Z");
    }

    void LateUpdate()
    {
        // Держим оси без вращения
        axesRoot.position = transform.position;
        axesRoot.rotation = Quaternion.identity;
    }

    void CreateAxis(Vector3 direction, Color color, string label)
    {
        GameObject axis = new GameObject(label + "_Axis");
        axis.transform.parent = axesRoot;

        LineRenderer lr = axis.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = false;
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, direction * axisLength);

        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        lr.material = mat;

        // Стрелка
        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        arrow.transform.parent = axis.transform;
        arrow.transform.localScale = new Vector3(arrowSize, arrowSize, arrowSize);
        arrow.transform.localPosition = direction * axisLength;
        arrow.GetComponent<Renderer>().material.color = color;

        // Текст
        GameObject textObj = new GameObject(label + "_Label");
        textObj.transform.parent = axis.transform;
        textObj.transform.localPosition = direction * (axisLength + 0.3f);

        TextMesh text = textObj.AddComponent<TextMesh>();
        text.text = label;
        text.characterSize = 0.2f;
        text.color = color;
        text.fontSize = 40;
    }
}