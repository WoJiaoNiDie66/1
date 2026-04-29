using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SelectionUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public float duration = 8f;

    [SerializeField]
    protected Image highlightImage;

    protected const float MAXALPHA = 0.95f;

    protected const float MINALPHA = 0.20f;

    protected bool flashStopped = true;

    private float lastRealTime;

    private float currentRealTime;

    private Coroutine currentCoroutine;

    public SelectorManager ParentSelector { get; private set; }

    protected virtual void Start()
    {
        ParentSelector = transform.parent.GetComponent<SelectorManager>();
        lastRealTime = Time.realtimeSinceStartup;
    }

    public virtual void Highlight()
    {
        if (highlightImage != null && currentCoroutine == null)
        {
            flashStopped = false;
            highlightImage.color = new Color(1, 1, 1, MINALPHA);
            StartFlashing();
        }
    }

    public virtual void UnHighlight()
    {
        if (highlightImage != null)
        {
            flashStopped = true;
            StopFlashing();
            highlightImage.color = new Color(1, 1, 1, MINALPHA);
        }
    }

    public void StartFlashing()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(FlashRoutine());
    }

    public void StopFlashing()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }

    private IEnumerator FlashRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(Flash(MAXALPHA));
            //yield return new WaitForSeconds(delay);
            yield return StartCoroutine(Flash(MINALPHA));
        }
    }

    private IEnumerator Flash(float targetAlpha)
    {
        Color startColor = highlightImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        float elapsedTime = 0f;
        float lastTime = Time.realtimeSinceStartup;

        while (elapsedTime < duration && !flashStopped)
        {
            float currentRealTime = Time.realtimeSinceStartup;
            float deltaTime = currentRealTime - lastTime;
            lastTime = currentRealTime;

            // Cap delta time to avoid huge jumps
            deltaTime = Mathf.Min(deltaTime, 0.1f);

            Debug.Log("deltaTime: " + deltaTime);
            Debug.Log("currentRealTime- lastTime: " + (currentRealTime - lastTime));

            elapsedTime += deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            highlightImage.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }
    }

    private void OnUIHover()
    {
        ParentSelector?.UIHover(this);
    }

    private void OnUIClicked()
    {
        ParentSelector?.UIClicked(this);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        OnUIHover();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        OnUIClicked();
    }
}
