using UnityEngine;

public class ShapeData
{
    public ShapeTypeAsset ShapeType { get; }
    public Color ShapeColor { get; }
    public AnimalTypeAsset AnimalType { get; }

    public ShapeData(ShapeTypeAsset shapeType, Color shapeColor, AnimalTypeAsset animalType)
    {
        ShapeType = shapeType;
        ShapeColor = shapeColor;
        AnimalType = animalType;
    }

    public bool Equals(ShapeData other) =>
        ShapeType.Equals(other.ShapeType) && ShapeColor.Equals(other.ShapeColor) && AnimalType.Equals(other.AnimalType);
}