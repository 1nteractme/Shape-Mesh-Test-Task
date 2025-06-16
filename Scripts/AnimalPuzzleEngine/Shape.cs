using UnityEngine;

public class Shape : MonoBehaviour
{
    [Header("Renderer References")]
    [SerializeField] private SpriteRenderer shapeRenderer;
    [SerializeField] private SpriteRenderer borderRenderer;
    [SerializeField] private SpriteRenderer animalRenderer;

    [Header("Physics Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D shapeCollider;

    public ShapeData ShapeData { get; private set; }

    public void Initialize(ShapeData data)
    {
        ShapeData = data;
        ApplyVisuals(data);
        ApplySpecialBehavior(data);
    }

    private void ApplyVisuals(ShapeData data)
    {
        if (data.ShapeType != null && data.AnimalType != null)
        {
            shapeRenderer.sprite = data.ShapeType.shapeSprite;
            borderRenderer.sprite = data.ShapeType.borderSprite;
            animalRenderer.sprite = data.AnimalType.animalSprite;
        }

        shapeRenderer.color = data.ShapeColor;
    }

    private void ApplySpecialBehavior(ShapeData data)
    {
        if (data.ShapeType == null) return;

        if (data.ShapeType.Type == ShapeTypeAsset.ShapeType.Heavy)
            rb.mass = 20f;
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnShapeClicked(this);
    }

    public void EnablePhysics()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.bodyType = RigidbodyType2D.Dynamic;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
            col.enabled = true;
    }
}