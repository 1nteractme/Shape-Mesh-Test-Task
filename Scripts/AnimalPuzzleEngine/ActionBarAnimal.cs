using UnityEngine;
using UnityEngine.UI;

public class ActionBarAnimal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image shape;
    [SerializeField] private Image animal;
    [SerializeField] private Image border;

    public ShapeData ShapeData { get; private set; }

    public void Initialize(ShapeData shapeData)
    {
        ShapeData = shapeData;
        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        if (shape != null)
        {
            shape.sprite = ShapeData.ShapeType.shapeSprite;
            shape.color = ShapeData.ShapeColor;
        }

        if (animal != null && ShapeData.AnimalType != null)
            animal.sprite = ShapeData.AnimalType.animalSprite;

        if (border != null)
            border.sprite = ShapeData.ShapeType.borderSprite;

    }
}