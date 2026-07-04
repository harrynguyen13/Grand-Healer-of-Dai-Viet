using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingHarvestText : MonoBehaviour
{
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private float moveSpeed = 0.6f;
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private float fadeOutTime = 0.4f;

    private Color startColor;

    public void Setup(string message)
    {
        if (textLabel != null)
        {
            textLabel.text = message;
            startColor = textLabel.color;
        }

        StartCoroutine(FloatAndFade());
    }

    private IEnumerator FloatAndFade()
    {
        float timer = 0f;

        while (timer < lifeTime)
        {
            timer += Time.deltaTime;
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            yield return null;
        }

        float fadeTimer = 0f;

        while (fadeTimer < fadeOutTime)
        {
            fadeTimer += Time.deltaTime;

            if (textLabel != null)
            {
                Color color = startColor;
                color.a = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutTime);
                textLabel.color = color;
            }

            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            yield return null;
        }

        Destroy(gameObject);
    }
}