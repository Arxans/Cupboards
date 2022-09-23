using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
    public GameManager m_GameManager = null;
    public int m_MapIndex;

    private Image m_Image;
    private Color m_OriginalColor;
    private Color m_HighlightColor;

    private void Start()
    {
        m_Image = GetComponent<Image>();
        if (m_Image)
        {
            m_OriginalColor = m_Image.color;
            m_HighlightColor = new Color(m_Image.color.r + 0.2f, m_Image.color.g + 0.2f, m_Image.color.b + 0.2f);
        }

    }

    private void OnMouseDown()
    {
        m_GameManager.OnIndicatorClicked(this);
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
