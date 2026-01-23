using UnityEngine;
using UnityEngine.UI;
using GeoidHeightsDotNet;

public class WaterLevelSlider : MonoBehaviour
{
    [Header("References")]
    public Slider mySlider;          
    public Transform objectToMove;

    [Header("Settings")]
    public int minYear = 2020;
    public int maxYear = 2150;

    void Start()
    {
        // Optional: Initialize the object's position based on the current slider value
        UpdateObjectPosition(mySlider.value);

        // Add a listener to the slider to detect changes
        mySlider.onValueChanged.AddListener(UpdateObjectPosition);
    }

    // This function is called whenever the slider value changes
    void UpdateObjectPosition(float value)
    {
        if (objectToMove == null) return;

        // 1. SAFETY CHECK: Ensure Singleton exists and has data
        if (CityDataManager.Instance == null || CityDataManager.Instance.SeaLevelsByYear == null)
        {
            // If no city is loaded yet, do nothing (or set to default 0)
            return;
        }

        // 2. RETRIEVE DATA FROM SINGLETON
        double latitude = CityDataManager.Instance.Latitude;
        double longitude = CityDataManager.Instance.Longitude;
        var seaLevels = CityDataManager.Instance.SeaLevelsByYear;

        // 3. CALCULATE GEOID UNDULATION
        // This sets the "base" height of the water at this specific GPS coordinate
        double undulation = GeoidHeights.undulation(latitude, longitude);

        // 4. CALCULATE INTERPOLATED SEA LEVEL RISE
        // Convert slider value (0.0 to 1.0) to a specific year (e.g. 2045.5)
        float currentYear = Mathf.Lerp(minYear, maxYear, value);

        // Find the decade brackets (Floor = 2040, Ceil = 2050)
        // Note: This math assumes data is in 10-year steps starting from minYear
        int floorYear = minYear + Mathf.FloorToInt((currentYear - minYear) / 10.0f) * 10;
        int ceilYear = floorYear + 10;

        // Get Sea Levels for those years
        double seaFloor = 0.0;
        double seaCeil = 0.0;

        if (seaLevels.ContainsKey(floorYear)) seaFloor = seaLevels[floorYear];

        // If ceiling doesn't exist (e.g. past max year), clamp to floor
        if (seaLevels.ContainsKey(ceilYear)) seaCeil = seaLevels[ceilYear];
        else seaCeil = seaFloor;

        // Interpolate between the two decades
        float t = (currentYear - floorYear) / 10.0f; // 0.0 to 1.0 within the decade
        float riseInMeters = Mathf.Lerp((float)seaFloor, (float)seaCeil, t);

        // 5. APPLY FINAL HEIGHT
        // Final Y = Base Geoid Height + Sea Level Rise
        float finalY = (float)(undulation + riseInMeters);

        Vector3 currentPos = objectToMove.position;
        objectToMove.position = new Vector3(currentPos.x, finalY, currentPos.z);

        // Optional Debug (Can be noisy in Update)
        // Debug.Log($"Year: {currentYear:F1} | Undulation: {undulation:F2} | Rise: {riseInMeters:F3} | Total Y: {finalY}");
    }
    void OnDestroy()
    {
        // Good practice: Remove the listener when the object is destroyed
        if (mySlider != null)
            mySlider.onValueChanged.RemoveListener(UpdateObjectPosition);
    }
}