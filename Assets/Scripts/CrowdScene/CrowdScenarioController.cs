using UnityEngine;
using UnityEngine.UI;

public class CrowdScenarioController : MonoBehaviour
{
    [Header("References")]
    public CrowdSystem crowdSystem;
    public Slider DensitySlider;

    private int initialDensity;

    public float CrowdDensity01
    {
        get
        {
            if (crowdSystem == null) return 0;
            return (float)crowdSystem.targetDensity / crowdSystem.maxDensity;
        }
    }

    private void Start()
    {
        if (crowdSystem != null)
            initialDensity = crowdSystem.targetDensity;
    }

    // вызывается из WebSocket
    public void SetCrowdDensity(float normalized)
    {
        if (crowdSystem == null) return;

        int value = Mathf.RoundToInt(normalized * crowdSystem.maxDensity);
        DensitySlider.value = value;
    }

    public void ResetScenario()
    {
        if (crowdSystem == null) return;

        DensitySlider.value = initialDensity;
    }
}