using UnityEngine;
using System.Collections;

public interface IMovableComponent {
    void UpdateMove(float deltaX, float deltaY);
}
