using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int m_Width, m_Height;
    [SerializeField] private Tile m_TilePrefab = null;
    [SerializeField] private RectTransform m_ConnectingLinePrefab = null;
    [SerializeField] private Indicator m_IndicatorPrefab = null;
    [SerializeField] private Color[] m_TileColors = null; 

    [SerializeField] private Transform m_Camera = null;
    [SerializeField] private GameObject m_Canvas = null;
    [SerializeField] private GameObject m_ExamplePanel = null;

    int[] m_WinningTilesPos;
    int[] m_InitTilesPos;

    private Tile m_ActiveTile = null;

    private Dictionary<int, Vector2> m_MapPoints = null;
    private Tile[] m_Tiles;

    private Dictionary<int, Indicator> m_Indicators = null;
    private List<int>[] m_ConnectBetweenPoints;

    private void Start()
    {
        /*input values*/
        int countOfTiles = 6;
        int countOfPoints = 9;
        // index of the chip and position
        m_MapPoints = new Dictionary<int, Vector2>();
        m_MapPoints.Add(0, new Vector2(100, 100));
        m_MapPoints.Add(1, new Vector2(200, 100));
        m_MapPoints.Add(2, new Vector2(300, 100));
        m_MapPoints.Add(3, new Vector2(100, 200));
        m_MapPoints.Add(4, new Vector2(200, 200));
        m_MapPoints.Add(5, new Vector2(300, 200));
        m_MapPoints.Add(6, new Vector2(100, 300));
        m_MapPoints.Add(7, new Vector2(200, 300));
        m_MapPoints.Add(8, new Vector2(300, 300));

        m_InitTilesPos = new int[]{ 1, 2, 3, 7, 8, 9};
        m_WinningTilesPos = new int[]{ 7, 8, 9, 1, 2, 3};
        
        int numOfConnections = 8;
        // static array with list elements
        m_ConnectBetweenPoints = new List<int>[countOfPoints];
        for (int i = 0; i < m_ConnectBetweenPoints.Length; ++i)
            m_ConnectBetweenPoints[i] = new List<int>();

        m_ConnectBetweenPoints[0].Add(3);
        m_ConnectBetweenPoints[1].Add(4);
        m_ConnectBetweenPoints[2].Add(5);
        m_ConnectBetweenPoints[3].Add(4);
        m_ConnectBetweenPoints[4].Add(5);
        m_ConnectBetweenPoints[3].Add(6);
        m_ConnectBetweenPoints[4].Add(7);
        m_ConnectBetweenPoints[5].Add(8);

        // connecting points in two directions
        m_ConnectBetweenPoints[3].Add(0);
        m_ConnectBetweenPoints[4].Add(1);
        m_ConnectBetweenPoints[5].Add(2);
        m_ConnectBetweenPoints[4].Add(3);
        m_ConnectBetweenPoints[5].Add(4);
        m_ConnectBetweenPoints[6].Add(3);
        m_ConnectBetweenPoints[7].Add(4);
        m_ConnectBetweenPoints[8].Add(5);

        m_Tiles = new Tile[6]; /*need to replace to countOfTiles*/

        SpawnConnectingLines();
        SpawnIndicators();
        SpawnTiles();

        CreateExample();
    }

    private void SpawnConnectingLines()
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
                    line.SetParent(m_Canvas.transform, false);

                    float width = Mathf.Max(50f, Mathf.Abs(endPos.x - startPos.x));
                    float height = Mathf.Max(50f, Mathf.Abs(endPos.y - startPos.y));

                    line.sizeDelta = new Vector2(width, height);

                    float offsetX = 0;
                    if (endPos.x - startPos.x > 0)
                        offsetX = 25;
                    else if (endPos.x - startPos.x < 0)
                        offsetX = -25;

                    float offsetY = 0;
                    if (endPos.y - startPos.y > 0)
                        offsetY = 25;
                    else if (endPos.y - startPos.y < 0)
                        offsetY = -25;

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
                indicator.transform.SetParent(m_Canvas.transform, false);
                indicator.name = $"PosIndicator {pos.x} {pos.y}";
                indicator.m_GridManager = this;
                indicator.m_MapIndex = i;

                indicator.gameObject.SetActive(false);

                m_Indicators.Add(i, indicator);
            }
        }
    }

    private void SpawnTiles()
    {
        int colorIndex = 0;
        Vector2 pos;

        int idx = 0;
        foreach (int tilePos in m_InitTilesPos)
        {
            pos = m_MapPoints[tilePos - 1];

            var spawnedTile = Instantiate(m_TilePrefab, pos, Quaternion.identity);
            if (spawnedTile)
            {
                spawnedTile.transform.SetParent(m_Canvas.transform, false);
                spawnedTile.name = $"Tile {pos.x} {pos.y}";
                spawnedTile.m_GridManager = this;
                spawnedTile.m_MapIndex = tilePos - 1;

                var image = spawnedTile.GetComponent<Image>();

                if (image && colorIndex < m_TileColors.Length)
                    image.color = m_TileColors[colorIndex++];

                m_Tiles[idx++] = spawnedTile;
            }
        }

        /*float minX = float.MaxValue;
        float maxX = int.MinValue;
        float minY = int.MaxValue;
        float maxY = int.MinValue;

        // find screen width and height   
        foreach (KeyValuePair<int, Vector2> pair in m_Positions)
        {
            if (pair.Value.x < minX)
                minX = pair.Value.x;
            if (pair.Value.x > maxX)
                maxX = pair.Value.x;
            if (pair.Value.y < minY)
                minY = pair.Value.y;
            if (pair.Value.y > maxY)
                maxY = pair.Value.y;
        }

        float width = maxX - minX;
        float height = maxY - minY;
        m_Camera.transform.position = new Vector3(  (float)width / 2 - 0.5f, (float)height / 2 - 0.5f, m_Camera.transform.position.z);*/
    }

    private void CreateExample()
    {
        // Spawn Connecting Lines
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
                    line.SetParent(m_ExamplePanel.transform, false);

                    float width = Mathf.Max(50f, Mathf.Abs(endPos.x - startPos.x));
                    float height = Mathf.Max(50f, Mathf.Abs(endPos.y - startPos.y));

                    line.sizeDelta = new Vector2(width, height);

                    float offsetX = 0;
                    if (endPos.x - startPos.x > 0)
                        offsetX = 25;
                    else if (endPos.x - startPos.x < 0)
                        offsetX = -25;

                    float offsetY = 0;
                    if (endPos.y - startPos.y > 0)
                        offsetY = 25;
                    else if (endPos.y - startPos.y < 0)
                        offsetY = -25;

                    line.localPosition = new Vector3(
                        line.localPosition.x + offsetX,
                        line.localPosition.y + offsetY,
                        line.localPosition.z
                    );


                    line.name = $"Line {startIndex} - {endIndex}";
                }
            }
        }

        // spawn Tiles
        int colorIndex = 0;
        Vector2 pos;


        int idx = 0;
        foreach (int tilePos in m_WinningTilesPos)
        {
            pos = m_MapPoints[tilePos - 1];

            var spawnedTile = Instantiate(m_TilePrefab, pos, Quaternion.identity);
            if (spawnedTile)
            {
                spawnedTile.transform.SetParent(m_ExamplePanel.transform, false);
                spawnedTile.name = $"Tile {pos.x} {pos.y}";
                spawnedTile.m_GridManager = this;
                spawnedTile.m_MapIndex = tilePos - 1;
                spawnedTile.GetComponent<BoxCollider2D>().enabled = false;

                var image = spawnedTile.GetComponent<Image>();

                if (image && colorIndex < m_TileColors.Length)
                    image.color = m_TileColors[colorIndex++];

                m_Tiles[idx++] = spawnedTile;

            }
        }

        m_ExamplePanel.transform.localScale /= 2;
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
        m_ActiveTile.transform.position = indicator.transform.position;

        DeselectTile();
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

    private bool IsVictory()
    {
        for (int i = 0; i < m_Tiles.Length; ++i)
        {
            if (m_Tiles[i].m_MapIndex != m_WinningTilesPos[i])
                return false;
        }

        return true;
    }
}
