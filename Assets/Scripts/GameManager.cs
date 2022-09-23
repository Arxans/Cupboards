using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int m_Width, m_Height;
    [SerializeField] private Tile m_TilePrefab = null;
    [SerializeField] private RectTransform m_ConnectingLinePrefab = null;
    [SerializeField] private Indicator m_IndicatorPrefab = null;
    [SerializeField] private Color[] m_TileColors = null; 

    [SerializeField] private GameObject m_GamePanel = null;
    [SerializeField] private GameObject m_ExamplePanel = null;
    [SerializeField] private GameObject m_WinDialog = null;

    [SerializeField] private TextAsset[] m_MapsTextFiles = null;

    private int m_CountOfTiles;
    private int m_CountOfMapPoints;
    private int m_CountOfConnections;

    int[] m_WinningTilesPos;
    int[] m_InitTilesPos;

    private Tile m_ActiveTile = null;

    private Dictionary<int, Vector2> m_MapPoints = null;
    private Tile[] m_Tiles;

    private Dictionary<int, Indicator> m_Indicators = null;
    private List<int>[] m_ConnectBetweenPoints;

    private void Start()
    {
        string str;

        using (StringReader reader = new StringReader(m_MapsTextFiles[DataHolder.mapIndex].text))
        {
            if (!int.TryParse(reader.ReadLine(), out m_CountOfTiles))
                Debug.LogError("TryParse Error");
            if (!int.TryParse(reader.ReadLine(), out m_CountOfMapPoints))
                Debug.LogError("TryParse Error");

            m_MapPoints = new Dictionary<int, Vector2>();
            int commaPos;

            for (int i = 0; i < m_CountOfMapPoints; ++i)
            {
                str = reader.ReadLine();
                commaPos = str.IndexOf(',');
                if (commaPos == -1)
                    continue;

                string strLeft = str.Substring(0, commaPos);
                string strRight = str.Substring(commaPos + 1, str.Length - commaPos - 1);

                Vector2 point;
                if (!float.TryParse(strLeft, out point.x))
                    Debug.LogError("TryParse Error");
                if (!float.TryParse(strRight, out point.y))
                    Debug.LogError("TryParse Error");

                m_MapPoints.Add(i, point);
            }

            m_InitTilesPos = new int[m_CountOfTiles];
            m_WinningTilesPos = new int[m_CountOfTiles];

            int value;

            str = reader.ReadLine() + ',';
            commaPos = str.IndexOf(',');
            int idx = 0;
            string substr;
            while (commaPos != -1 && idx < m_InitTilesPos.Length)
            {
                substr = str.Substring(0, commaPos);
                str = str.Substring(commaPos + 1, str.Length - commaPos - 1);

                if (!int.TryParse(substr, out value))
                    Debug.LogError("TryParse Error");

                m_InitTilesPos[idx++] = value - 1;

                commaPos = str.IndexOf(',');
            }

            str = reader.ReadLine() + ',';
            commaPos = str.IndexOf(',');
            idx = 0;
            while (commaPos != -1 && idx < m_WinningTilesPos.Length)
            {
                substr = str.Substring(0, commaPos);
                str = str.Substring(commaPos + 1, str.Length - commaPos - 1);

                if (!int.TryParse(substr, out value))
                    Debug.LogError("TryParse Error");

                m_WinningTilesPos[idx++] = value - 1;

                commaPos = str.IndexOf(',');
            }


            if (!int.TryParse(reader.ReadLine(), out m_CountOfConnections))
                Debug.LogError("TryParse Error");

            // static array with list elements
            m_ConnectBetweenPoints = new List<int>[m_CountOfMapPoints];
            for (int i = 0; i < m_ConnectBetweenPoints.Length; ++i)
                m_ConnectBetweenPoints[i] = new List<int>();

            for (int i = 0; i < m_CountOfConnections; ++i)
            {
                str = reader.ReadLine();
                commaPos = str.IndexOf(',');
                if (commaPos == -1)
                    continue;

                string strLeft = str.Substring(0, commaPos);
                string strRight = str.Substring(commaPos + 1, str.Length - commaPos - 1);

                int nLeft, nRight;
                if (!int.TryParse(strLeft, out nLeft))
                    Debug.LogError("TryParse Error");
                if (!int.TryParse(strRight, out nRight))
                    Debug.LogError("TryParse Error");

                nLeft--; nRight--;

                m_ConnectBetweenPoints[nLeft].Add(nRight);
                m_ConnectBetweenPoints[nRight].Add(nLeft);
            }
         
            m_Tiles = new Tile[m_CountOfTiles];
        }

        CreateExampleMap();
        CreateMainMap();
    }

    private void CreateExampleMap()
    {
        SpawnConnectingLines(true);
        SpawnTiles(true);

        m_ExamplePanel.transform.localScale /= 2;
    }

    private void CreateMainMap()
    {
        SpawnConnectingLines(false);
        SpawnIndicators();
        SpawnTiles(false);
    }

    private void SpawnConnectingLines(bool bExample)
    {
        Vector2 startPos, endPos;
        for (int startIndex = 0; startIndex < m_ConnectBetweenPoints.Length; ++startIndex)
        {
            if (!m_MapPoints.TryGetValue(startIndex, out startPos))
                Debug.LogError("No such key found");

            foreach (int endIndex in m_ConnectBetweenPoints[startIndex])
            {
                // avoiding the creation of existing connections
                if (startIndex > endIndex)
                    continue;

                if (!m_MapPoints.TryGetValue(endIndex, out endPos))
                    Debug.LogError("No such key found");

                RectTransform line = Instantiate(m_ConnectingLinePrefab, startPos, Quaternion.identity);
                if (line)
                {
                    line.SetParent(
                        bExample ? m_ExamplePanel.transform : m_GamePanel.transform, 
                        false
                    );

                    float width = Mathf.Max(50f, Mathf.Abs(endPos.x - startPos.x));
                    float height = Mathf.Max(50f, Mathf.Abs(endPos.y - startPos.y));

                    line.sizeDelta = new Vector2(width, height);

                    float offsetX = 0;
                    if (endPos.x - startPos.x > 0)
                        offsetX = 50;
                    else if (endPos.x - startPos.x < 0)
                        offsetX = -50;

                    float offsetY = 0;
                    if (endPos.y - startPos.y > 0)
                        offsetY = 50;
                    else if (endPos.y - startPos.y < 0)
                        offsetY = -50;

                    line.localPosition = new Vector3(
                        line.localPosition.x + offsetX,
                        line.localPosition.y + offsetY,
                        line.localPosition.z
                    );


                    line.name = $"Line {startIndex} - {endIndex}";
                }
            }
        }
    }

    private void SpawnIndicators()
    {
        m_Indicators = new Dictionary<int, Indicator>();

        Vector2 pos;

        for (int i = 0; i < m_MapPoints.Count; ++i)
        {
            pos = m_MapPoints[i];

            var indicator = Instantiate(m_IndicatorPrefab, pos, Quaternion.identity);
            if (indicator)
            {
                indicator.transform.SetParent(m_GamePanel.transform, false);
                indicator.name = $"PosIndicator {pos.x} {pos.y}";
                indicator.m_GameManager = this;
                indicator.m_MapIndex = i;

                indicator.gameObject.SetActive(false);

                m_Indicators.Add(i, indicator);
            }
        }
    }

    private void SpawnTiles(bool bExample)
    {
        int colorIndex = 0;
        Vector2 pos;

        int idx = 0;
        var arrTilesPos = bExample ? m_WinningTilesPos : m_InitTilesPos;
        foreach (int tilePos in arrTilesPos)
        {
            pos = m_MapPoints[tilePos];

            var spawnedTile = Instantiate(m_TilePrefab, pos, Quaternion.identity);
            if (spawnedTile)
            {
                spawnedTile.transform.SetParent(
                    bExample ? m_ExamplePanel.transform : m_GamePanel.transform,
                    false
                );
                spawnedTile.name = $"Tile {pos.x} {pos.y}";
                spawnedTile.m_GameManager = this;
                spawnedTile.m_MapIndex = tilePos;
                if (bExample)
                    spawnedTile.GetComponent<BoxCollider2D>().enabled = false;

                var image = spawnedTile.GetComponent<Image>();

                if (image && colorIndex < m_TileColors.Length)
                    image.color = m_TileColors[colorIndex++];

                m_Tiles[idx++] = spawnedTile;
            }
        }
    }

    public void OnTileClicked(in Tile tile)
    {
        if (m_ActiveTile == tile)
            DeselectTile();
        else
            SelectTile(tile);
    }

    public void OnIndicatorClicked(in Indicator indicator)
    {
        m_ActiveTile.m_MapIndex = indicator.m_MapIndex;
        m_ActiveTile.m_newPosition = indicator.transform.position;

        DeselectTile();

        if (IsWin())
            m_WinDialog.SetActive(true);
    }

    private List<int> FindFreePositions(int startPos)
    {
        Queue<int> queue = new Queue<int>();
        bool[] visited = new bool[9]; /*need to replace to countOfPoints*/

        queue.Enqueue(startPos);

        // close the passage through the vertices where there are tiles
        foreach (Tile tile in m_Tiles)
            visited[tile.m_MapIndex] = true;

        while (queue.Count > 0)
        {
            int vertex = queue.Dequeue();
            visited[vertex] = true;

            foreach (int neighbor in m_ConnectBetweenPoints[vertex])
            {
                if (!visited[neighbor])
                    queue.Enqueue(neighbor);
            }
        }

        // I set the flag to false for places where the tile is installed  
        // so that these positions do not fall into the array of free positions
        foreach (Tile tile in m_Tiles)
            visited[tile.m_MapIndex] = false;

        List<int> freePositions = new List<int>();
        for (int i = 0; i < visited.Length; ++i)
        {
            if (visited[i] && i != startPos)
                freePositions.Add(i);
        }

        return freePositions;
    }

    private void SelectTile(in Tile tile)
    {
        m_ActiveTile = tile;

        var freePositions = FindFreePositions(tile.m_MapIndex);
        foreach (int pos in freePositions)
            m_Indicators[pos].gameObject.SetActive(true);
    }

    private void DeselectTile()
    {
        m_ActiveTile = null;

        // hide all indicators
        foreach (var indicator in m_Indicators)
            indicator.Value.gameObject.SetActive(false);
    }

    private bool IsWin()
    {
        for (int i = 0; i < m_Tiles.Length; ++i)
        {
            if (m_Tiles[i].m_MapIndex != m_WinningTilesPos[i])
                return false;
        }

        return true;
    }

    public void BackToMapSelection()
    {
        SceneManager.LoadScene("MapSelection");
    }
}
