namespace CoinQuest
{
    public interface IWorldInteractable
    {
        string InteractionPrompt { get; }
        void Interact(GameManager manager);
    }
}
