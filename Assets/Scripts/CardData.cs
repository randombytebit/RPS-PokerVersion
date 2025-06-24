using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "ScriptableObjects/CardData")]
public class CardData : ScriptableObject{
  public Sprite cardImage = null;
  public string cardName;
  public CardData beats;
}
