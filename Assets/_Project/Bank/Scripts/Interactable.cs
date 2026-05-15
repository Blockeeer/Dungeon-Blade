using UnityEngine;

namespace DungeonBlade.Bank
{
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] protected string promptText = "Press [F] to interact";

        public virtual string PromptText => promptText;

        public abstract void OnInteract(GameObject player);
    }
}
