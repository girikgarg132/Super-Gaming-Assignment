using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefs : MonoBehaviour
{
    public static int HighScore
    {
        get => PlayerPrefs.GetInt(nameof(HighScore), 0);
        set => PlayerPrefs.SetInt(nameof(HighScore), value);
    }
}
