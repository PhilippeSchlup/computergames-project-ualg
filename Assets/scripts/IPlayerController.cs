using UnityEngine;

public interface IPlayerController
{
    float CurrentSpeed { get; }

    void Initialize(GameCharacter character, float speed, bool isHunter);
    void UpdateController();
    void FixedUpdateController();
    void OnMove(Vector2 input);
    void Deactivate();
}
