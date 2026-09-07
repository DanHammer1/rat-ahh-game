using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEditor.U2D;

public class CrowbarPossess : Possessable {
    public override string GetInteractionPromptText() {
        return "Hold E to possess crowbar.";
    }
}
