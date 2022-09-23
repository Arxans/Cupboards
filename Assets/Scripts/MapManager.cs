using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public void OnMapSelected(int index)
    {
        DataHolder.mapIndex = index;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
