using System;
using NUnit.Framework;
using UnityEngine;

public class SnakeMovementTests
{
    private Type movementType;

    [SetUp]
    public void SetUp()
    {
        movementType = Type.GetType("SnakeMovement, Assembly-CSharp");
        Assert.That(movementType, Is.Not.Null);
    }

    [Test]
    public void SetDirection_IgnoresImmediateReverse()
    {
        object movement = Activator.CreateInstance(movementType);
        movementType.GetMethod("Initialize").Invoke(movement, null);
        movementType.GetProperty("IsMoving").SetValue(movement, true);

        movementType.GetMethod("SetDirection").Invoke(movement, new object[] { Vector2Int.left });
        movementType.GetMethod("Tick").Invoke(movement, new object[] { 1f });

        Assert.That((Vector2Int)movementType.GetProperty("CurrentDirection").GetValue(movement), Is.EqualTo(Vector2Int.right));
    }

    [Test]
    public void OnFoodEaten_SpeedsUpAtConfiguredInterval()
    {
        object movement = Activator.CreateInstance(movementType);
        movementType.GetProperty("BaseMoveInterval").SetValue(movement, 0.2f);
        movementType.GetProperty("MinMoveInterval").SetValue(movement, 0.1f);
        movementType.GetProperty("SpeedUpInterval").SetValue(movement, 2);
        movementType.GetProperty("SpeedUpAmount").SetValue(movement, 0.05f);
        movementType.GetMethod("Initialize").Invoke(movement, null);

        movementType.GetMethod("OnFoodEaten").Invoke(movement, null);
        Assert.That((float)movementType.GetProperty("CurrentMoveInterval").GetValue(movement), Is.EqualTo(0.2f));

        movementType.GetMethod("OnFoodEaten").Invoke(movement, null);
        Assert.That((float)movementType.GetProperty("CurrentMoveInterval").GetValue(movement), Is.EqualTo(0.15f).Within(0.0001f));
    }
}
