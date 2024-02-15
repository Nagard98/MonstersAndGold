using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    public float m_fadeSpeed;
    public GameObject m_fadeScreen;
    public float m_initialAlpha;

    private CanvasRenderer m_canvasRenderer;

    // Start is called before the first frame update
    void Start()
    {
        m_canvasRenderer = GetComponent<CanvasRenderer>();
        m_canvasRenderer.SetAlpha(m_initialAlpha);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeIn(float delay)
    {
        StartCoroutine(FadeInCoroutine(delay));
    }

    public IEnumerator FadeInCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        while (m_canvasRenderer.GetAlpha() > 0.0f)
        {
            m_canvasRenderer.SetAlpha(m_canvasRenderer.GetAlpha() - (Time.deltaTime * m_fadeSpeed));
            yield return null;
        }
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    public IEnumerator FadeOutCoroutine()
    {
        while (m_canvasRenderer.GetAlpha() < 1.0f)
        {
            m_canvasRenderer.SetAlpha(m_canvasRenderer.GetAlpha() + (Time.deltaTime * m_fadeSpeed));
            yield return null;
        }
    }
}
