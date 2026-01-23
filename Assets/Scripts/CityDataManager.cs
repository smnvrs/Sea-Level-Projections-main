using System;
using System.Collections.Generic; // Required for Dictionary
using UnityEngine;

public class CityDataManager : MonoBehaviour
{
    public static CityDataManager Instance { get; private set; }

    public string CityName { get; private set; }
    public string Country { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    // CHANGED: Now stores the full history (Year -> Water Level)
    public Dictionary<int, double> SeaLevelsByYear { get; private set; }

    // We keep the specific selected year just for reference
    public int SelectedYear { get; private set; }

    public event Action OnCityDataChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // CHANGED: The signature now asks for the dictionary of sea levels
    public void SetCurrentCity(string name, string country, double lat, double lon, Dictionary<int, double> seaLevels, int selectedYear)
    {
        CityName = name;
        Country = country;
        Latitude = lat;
        Longitude = lon;
        SeaLevelsByYear = seaLevels; // Save the whole list
        SelectedYear = selectedYear;

        Debug.Log($"[Singleton] Data Updated for {CityName}. Loaded {SeaLevelsByYear.Count} historical data points.");

        OnCityDataChanged?.Invoke();
    }

    // Helper: If you just want the water level for the currently selected year
    public double GetCurrentSeaLevel()
    {
        if (SeaLevelsByYear != null && SeaLevelsByYear.ContainsKey(SelectedYear))
        {
            return SeaLevelsByYear[SelectedYear];
        }
        return 0.0;
    }
}