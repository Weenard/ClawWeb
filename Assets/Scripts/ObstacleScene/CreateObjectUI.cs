using UnityEngine;
using TMPro;
using System.Globalization;

public class CreateObstacleUI : MonoBehaviour
{
    public TMP_InputField inputX;
    public TMP_InputField inputY;
    public TMP_InputField inputZ;

    public ObstaclesScenarioController controller;

    public void OnCreateClicked()
    {
        if (TryGetVector3(out Vector3 position))
        {
            if (!controller.TryAddObstacle(position, out string error))
            {
                Debug.Log(error);
            }
        }
        else
        {
            Debug.Log("Invalid input");
        }
    }

    public void OnClearClicked()
    {
        controller.ClearObstacles();
    }

    private bool TryGetVector3(out Vector3 result)
    {
        result = Vector3.zero;

        bool xOk = float.TryParse(inputX.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
        bool yOk = float.TryParse(inputY.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float y);
        bool zOk = float.TryParse(inputZ.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float z);

        if (xOk && yOk && zOk)
        {
            result = new Vector3(x, y, z);
            return true;
        }

        return false;
    }
}