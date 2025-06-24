using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameState{
  MainMenu,
  Initializing,
  InGame,
  GameOver
}

public class GameManager : MonoBehaviour{ 
  private GameState currentState = GameState.Initializing;
  private int _potMoney = 0;
  private int _playerMoney = 1000;
  private int _aiMoney = 100;
  private int _playerBet = 0;
  private int _playerRaiseMoney = 0;
  private int _aiBet = 0;
  private int _aiRaiseMoney = 0;
  private int _blindAmount = 5;
  private string _dealer = "none";
  private string _winner = "none";
  private string _optionSelector = "none";
  private Image[] _dealerImage;
  private List<string> _optionsHistory = new List<string>();
  private int _roundsPlayed = 0;

  public Card playerCard;
  public Card aiCard;
  public Card[] cardOption;
  public TextMeshProUGUI potMoneyText;
  public TextMeshProUGUI playerMoneyText;
  public TextMeshProUGUI aiMoneyText;
  public TextMeshProUGUI winnerText;
  public GameObject cardPanel;
  public GameObject cardSelectionPanel;
  public GameObject optionsPanel;
  public GameObject gameOverScreen;
  public GameObject raisePanel;
  public GameObject moneyDisplayPanel;
  public GameObject dealerPanel;
  public GameObject WinnerPanel;

  private void Awake() {
    _dealerImage = dealerPanel.transform.GetComponentsInChildren<Image>();
  }

  private void Start() {
    InitializeGame();
  }

  private void Update() {
    switch (currentState){
      case GameState.MainMenu:
        break;
      case GameState.Initializing:
        InitialStateUpdate();
        break;
      case GameState.InGame:
        InGameStateUpdate();
        break;
      case GameState.GameOver:
        GameOverStateUpdate();
        break;
    }
    GameOver();
  }

  private void InitializeGame(){
    currentState = GameState.Initializing;
    InitalizeObject();
    InitalizeScreen();
    _roundsPlayed = 0;
  }
  private void InitialStateUpdate(){
    if (_optionsHistory.Count == 0){
        RoundStart();
    }
  }

  private void InGameStateUpdate() {
  }

  private void GameOverStateUpdate(){
    if (Input.GetKeyDown(KeyCode.R)){
        InitializeGame();
    }
  }
  private void RoundStart(){
    currentState = GameState.InGame;
    InitalizeScreen();
    InitalizeObject();
    SelectDealer();
    SmallBlind();
    UpdateMoney();
    PlayerCardSelection();
    _roundsPlayed++;
  }

  private void SelectDealer(){
    bool randDealer = UnityEngine.Random.value > 0.5f;
    if (randDealer){
      _dealer = "player";
      _dealerImage[0].gameObject.SetActive(true);
    } else {
      _dealer = "ai";
      _dealerImage[1].gameObject.SetActive(true);
    }
    _optionSelector = (_dealer == "player")? "player": "ai";
  }

  private void SmallBlind(){
    if (_dealer == "player"){
      _aiRaiseMoney = _blindAmount;
      _aiMoney -= _blindAmount;
      _aiBet += _blindAmount;
    } else if (_dealer == "ai") {
      _playerRaiseMoney = _blindAmount;
      _playerMoney -= _blindAmount;
      _playerBet += _blindAmount;
    }
  }

  private void UpdateMoney(){
    UpdatePotMoney();
    UpdateMoneyDisplay();
  }

  private void UpdatePotMoney(){
    _potMoney = _playerBet + _aiBet;
  }

  private void UpdateMoneyDisplay(){
    playerMoneyText.text = _playerMoney.ToString();
    aiMoneyText.text = _aiMoney.ToString();
    potMoneyText.text = _potMoney.ToString();
  }

  private void PlayerCardSelection(){
    cardPanel.SetActive(true);
    cardSelectionPanel.SetActive(true);
  }

  public void SelectCard(Card selectedCard){
    playerCard.cardData = selectedCard.cardData;
    AICardSelection();
    cardSelectionPanel.SetActive(false);
    OptionFlowControl();
  }

  private void AICardSelection(){
    int randomIndex = UnityEngine.Random.Range(0, 3);
    aiCard.cardData = cardOption[randomIndex].cardData;
  }

  private void ShowCards(){
    if (playerCard != null && aiCard != null) {
        playerCard.transform.GetComponent<Image>().sprite = null;
        aiCard.transform.GetComponent<Image>().sprite = null;

        if (playerCard.cardData != null && playerCard.cardData.cardImage != null && aiCard.cardData != null && aiCard.cardData.cardImage != null) {
            playerCard.transform.GetComponent<Image>().sprite = playerCard.cardData.cardImage;
            aiCard.transform.GetComponent<Image>().sprite = aiCard.cardData.cardImage;
        } else {
            Debug.LogError("Card data or image is not set.");
        }
    } else {
        Debug.LogError("Card is not assigned.");
    }
  }

  private void OptionFlowControl(){
    if (_optionSelector == "player"){
      Debug.Log("Player do decision");
      optionsPanel.SetActive(true);
    } else if (_optionSelector == "ai"){
      Debug.Log("Ai do decision");
      AIRandomOption();
    }
  } 

  private bool ShouldEndRound(){
    if (_optionsHistory.Count < 2){
      return false;
    }

    string lastAction = _optionsHistory[_optionsHistory.Count - 1];
    string secondLastAction = _optionsHistory[_optionsHistory.Count - 2];

    if ((secondLastAction == "Raise" && lastAction == "Call") || (secondLastAction == "Call" && lastAction == "Call")){
      return true;
    }

    return false;
  }

  private void AIRandomOption(){
    float randomNumber = UnityEngine.Random.value; 

    if (randomNumber < 0.1f){
      AIHandleAction("Throw");
      Debug.Log("AI Throw");
    } else if (randomNumber < 0.7f) {
      AIHandleAction("Call");
      Debug.Log("AI Call");
    } else {
      AIHandleAction("Raise");
      Debug.Log("AI Raise");
    }
  }

  private void AIHandleAction(string action){
    switch (action){
      case "Throw":
        _winner = "player";
        UpdateMoney();
        DetermineRoundOutcome();
        break;
                
      case "Call":
        Call("ai");
        _optionSelector = "player";
        if (ShouldEndRound()) {
          ShowCards();
          CheckWinner();
          UpdateMoney();
          DetermineRoundOutcome();
        } else {
          OptionFlowControl();
        }
        break;

      case "Raise":
        AIHandleRaise(10);
        break;
    }
  }

  public void HandleAction(string action){
    switch (action){
      case "Throw":
        optionsPanel.SetActive(false);
        _winner = "ai";
        UpdateMoney();
        DetermineRoundOutcome();
        break;
          
      case "Call":
        optionsPanel.SetActive(false);
        Call("player");
        _optionSelector = "ai";
        if (ShouldEndRound()) {
          ShowCards();
          CheckWinner();
          UpdateMoney();
          DetermineRoundOutcome();
        } else {
          OptionFlowControl();
        }
        break;

      case "Raise":
        DisplayRaisePanel(true);
        break;
    }
  }

  public void AIHandleRaise(int raiseNumber){
    if (_aiMoney - _aiBet >= raiseNumber){
      _aiRaiseMoney = raiseNumber;
      _aiMoney -= _aiRaiseMoney;
      _aiBet += _aiRaiseMoney;
      _optionsHistory.Add("Raise");
      _optionSelector = "player";
      if (ShouldEndRound()) {
        ShowCards();
        CheckWinner();
        UpdateMoney();
        DetermineRoundOutcome();
      } else {
        OptionFlowControl();
      }
    } else {
      Debug.Log($"AiMoney not enough");
    }
  }

  public void HandleRaise(int raiseNumber){
    if (_playerMoney - _playerBet >= raiseNumber){
      _playerRaiseMoney = raiseNumber;
      _playerMoney -= _playerRaiseMoney;
      _playerBet += _playerRaiseMoney;
      _optionsHistory.Add("Raise");
      _optionSelector = "ai";
      if (ShouldEndRound()) {
        ShowCards();
        CheckWinner();
        UpdateMoney();
        DetermineRoundOutcome();
      } else {
        DisplayRaisePanel(false);
        OptionFlowControl();
      }
    } else {
      Debug.Log($"PlayerMoney not enough");
      DisplayRaisePanel(false);
    }
  }


  private void Call(string selector){
    if (selector == "player") {
      _playerBet += _aiRaiseMoney;
      _playerMoney -= _aiRaiseMoney;
    } else if (selector == "ai") {
      _aiBet += _playerRaiseMoney;
      _aiMoney -= _playerRaiseMoney;
    }
    _optionsHistory.Add("Call");
    UpdateMoney();
  }

  private void CheckWinner(){
    if (playerCard.Beats(aiCard)){
      _winner = "player";
    } else if (aiCard.Beats(playerCard)){
      _winner = "ai";
    } else {
      _winner = "none";
    }
    Debug.Log($"Winner is {_winner}");
  }

  private void DisplayRaisePanel(bool open){
    if (open){
      cardPanel.SetActive(false);
      optionsPanel.SetActive(false);
      moneyDisplayPanel.SetActive(false);
      raisePanel.SetActive(true);
    } else {
      cardPanel.SetActive(true);
      optionsPanel.SetActive(true);
      moneyDisplayPanel.SetActive(true);
      raisePanel.SetActive(false);
    }
  }

  private void DetermineRoundOutcome(){
    switch(_winner){
      case "player":
        _playerMoney += _potMoney;
        break;
      
      case "ai":
        _aiMoney += _potMoney;
        break;
      
      case "none":
        _playerMoney += _playerBet;
        _aiMoney += _aiBet;
        break;
    }
    optionsPanel.SetActive(false);
    winnerText.text = "Winner: " + _winner;
    WinnerPanel.SetActive(true);
    Invoke("RoundStart", 2);
  }

  private void InitalizeObject(){
    _dealerImage[0].gameObject.SetActive(false);
    _dealerImage[1].gameObject.SetActive(false);
    _potMoney = 0;
    _playerBet = 0;
    _aiBet = 0;
    _playerRaiseMoney = 0;
    _aiRaiseMoney = 0;
    _dealer = "none";
    _winner = "none";
    _optionSelector = "none";
    _optionsHistory.Clear();
    playerCard.cardData = cardOption[3].cardData;
    aiCard.cardData = cardOption[3].cardData;
    ShowCards();
    UpdateMoney();
  }

  private void InitalizeScreen(){
    cardPanel.SetActive(true);
    cardSelectionPanel.SetActive(true);
    moneyDisplayPanel.SetActive(true);
    raisePanel.SetActive(false);
    optionsPanel.SetActive(false);
    gameOverScreen.SetActive(false);
    WinnerPanel.SetActive(false);
  }

  private async void GameOver(){
    if ((_playerMoney <= 0 || _aiMoney <= 0) && _optionsHistory.Count == 0){
      gameOverScreen.SetActive(true);
      cardSelectionPanel.SetActive(false);
      cardPanel.SetActive(false);
      moneyDisplayPanel.SetActive(false);
      raisePanel.SetActive(false);
      optionsPanel.SetActive(false);
      dealerPanel.SetActive(false);

      if (_aiMoney <= 0)
      {
        try
          {
              var gameData = new GameAPIClient.GameData
              {
                  playerFinalMoney = _playerMoney,
                  roundsToWin = _roundsPlayed
              };

              // Use the singleton instance
              string response = await GameAPIClient.Instance.PostDataAsync("savegame", gameData);
              Debug.Log($"Game result saved to server: {response}");
          }
          catch (System.Exception e)
          {
              Debug.LogError($"Failed to save game result: {e.Message}");
          }
      }
    }
  }
}