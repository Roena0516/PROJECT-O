using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using FMODUnity;

public class CompleteLoading : MonoBehaviour
{
    private SettingsManager settingsManager;

    private void Start()
    {
        settingsManager = SettingsManager.Instance;
        settingsManager.LoadSettings();

        DontDestroyOnLoad(settingsManager.gameObject);

        SceneManager.LoadSceneAsync("Menu");
    }
}
