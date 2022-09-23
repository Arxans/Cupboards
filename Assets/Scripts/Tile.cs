using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public GridManager m_GridManager = null;
    public int m_MapIndex;
    public Vector3 m_newPosition;

    private Image m_Image;
    private Color m_OriginalColor;
    private Color m_HighlightColor;
    private float m_Speed = 2.5f;

    private void Start()
    {
        m_newPosition = transform.position;
        m_Image = GetComponent<Image>();
        if (m_Image)
        {
            m_OriginalColor = m_Image.color;
            m_HighlightColor = new Color(m_Image.color.r + 0.2f, m_Image.color.g + 0.2f, m_Image.color.b + 0.2f);
        }

        m_newPosition = transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, m_newPosition, Time.deltaTime * m_Speed);
    }

    private void OnMouseDown()
    {
        m_GridManager.OnTileClicked(this);
    }
    
    private void OnMouseEnter()
    {
        m_Image.color = m_HighlightColor;
    }
    
    private void OnMouseExit()
    {
        m_Image.color = m_OriginalColor;
    }
}
