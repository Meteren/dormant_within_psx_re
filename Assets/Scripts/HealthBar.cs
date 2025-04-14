using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image fill;
    [SerializeField] private Image border;
    [SerializeField] private Image avatar;

    Color avatarOriginalColor;
    Color fillOriginalColor;
    Color borderOriginalColor;

    public float MaXHealth {  get; set; }
    public float MinHealth { get; set; }

    private float defaultMaxHealth = 100;

    private float defaultMinHealth = 0;

    public bool shouldFlicker;

    public delegate bool ShouldFlicker();

    private void Start()
    {
        avatarOriginalColor = avatar.color;
        fillOriginalColor = fill.color;
        borderOriginalColor = border.color;

        MaXHealth = defaultMaxHealth;
        MinHealth = defaultMinHealth;
    }

    public void SetHealth(float healthAmount,ShouldFlicker shouldFlicker)
    {
        healthBar.value = healthAmount;
        this.shouldFlicker = shouldFlicker.Invoke();
        if (this.shouldFlicker && gameObject.activeInHierarchy)
        {
            Debug.Log("Flicker coroutine initted");
            StartCoroutine(Flicker());
        }          
    }  

    private IEnumerator Flicker()
    {
        
        while (shouldFlicker)
        {
            yield return new WaitForSeconds(0.2f);
            Color newColor = new Color(1f, 0f, 0f, 85f / 255f);
            avatar.color = newColor;
            fill.color = Color.red;
            border.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            avatar.color = avatarOriginalColor;
            fill.color = fillOriginalColor;
            border.color = borderOriginalColor; 

        }

        border.color = borderOriginalColor;
        fill.color = fillOriginalColor;
        avatar.color = avatarOriginalColor;
    }
    private void OnEnable()
    {
        if(shouldFlicker)
            StartCoroutine(Flicker());
    }

    private void OnDisable()
    {
        border.color = borderOriginalColor;
        fill.color = fillOriginalColor;
        avatar.color = avatarOriginalColor;
    }

}
