using UnityEngine;

public class Card : MonoBehaviour{
  public CardData cardData;

  private void Awake() {
    // Debug.Log($"CardData: {cardData}, CardImage: {cardData?.cardImage}");
    if (cardData.cardImage != null && cardData != null){
      this.transform.GetComponent<UnityEngine.UI.Image>().sprite = cardData.cardImage;
    }
  }

  public bool Beats(Card otherCard){
    return cardData.beats == otherCard.cardData;
  }
}
