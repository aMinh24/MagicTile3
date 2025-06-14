using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSongData", menuName = "Song Data", order = 1)]
public class DataSong : ScriptableObject
{
    public string songName;
    public string singerName;
    public List<TilePointData> tilePointDatas;
}

[System.Serializable]
public class TilePointData // Changed from struct to class
{
    //point[0] is time, point[1] is column index
    public List<Vector2> points; 
    
}