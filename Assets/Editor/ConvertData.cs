using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ConvertData
{
    [System.Serializable]
    private class AccentDataJson
    {
        public string audio_file;
        public List<double> accent_timestamps;
    }

    [MenuItem("Tools/Convert Accent Data to Song")]
    public static void ConvertAccentsToSongData()
    {
        string jsonFilePath = Path.Combine(Application.dataPath, "Resources", "Data", "accents_data.json");

        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError($"JSON file not found at: {jsonFilePath}");
            return;
        }

        string jsonString = File.ReadAllText(jsonFilePath);
        AccentDataJson jsonData = JsonUtility.FromJson<AccentDataJson>(jsonString);

        if (jsonData == null)
        {
            Debug.LogError("Failed to parse JSON data.");
            return;
        }

        DataSong dataSong = ScriptableObject.CreateInstance<DataSong>();

        dataSong.songName = Path.GetFileNameWithoutExtension(jsonData.audio_file);
        dataSong.singerName = "Default Singer"; // You can change this or leave it empty
        dataSong.tilePointDatas = new List<TilePointData>();

        foreach (double timestamp in jsonData.accent_timestamps)
        {
            TilePointData tilePointData = new TilePointData();
            tilePointData.points = new List<Vector2>();

            float time = (float)timestamp;
            int columnIndex = Random.Range(0, 4); // Generates a random integer between 0 (inclusive) and 4 (exclusive), so 0, 1, 2, or 3.

            tilePointData.points.Add(new Vector2(time, columnIndex));
            dataSong.tilePointDatas.Add(tilePointData);
        }

        string outputDir = Path.Combine("Assets", "Resources", "Data", "Songs");
        if (!Directory.Exists(Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), outputDir)))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), outputDir));
        }

        string assetPath = Path.Combine(outputDir, dataSong.songName + ".asset");
        
        AssetDatabase.CreateAsset(dataSong, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Successfully converted accent data to SongData asset at: {assetPath}");
    }
}
