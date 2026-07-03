using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    void Update()
    {
        IInteractable.TryDisplayInteractionText();
    }
}
