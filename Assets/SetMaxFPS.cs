using UnityEngine;

/// <summary>
/// Util component to limit FPS with our mostly static UI to not overwork GPU
/// </summary>
public class SetMaxFPS : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
}