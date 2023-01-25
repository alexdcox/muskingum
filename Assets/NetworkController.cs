using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NativeWebSocket;
using HexGame;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class NetworkController : MonoBehaviour {
  public GameController gameController;
  public MenuController menuController;
  
  private static string ServerUrl = "ws://localhost:3030";

  private const string MessageTypeOutLogin = "login";
  private const string MessageTypeOutSummon = "summon";
  private const string MessageTypeOutSkipSummon = "skipsummon";
  private const string MessageTypeOutMove = "move";
  private const string MessageTypeOutSkipMove = "skipmove";
  private const string MessageTypeOutAttack = "attack";
  private const string MessageTypeOutSkipAttack = "skipattack";
  private const string MessageTypeOutGameState = "gamestate";

  private const string MessageTypeInGameState = "gamestate";
  private const string MessageTypeInPlayerId = "playerid";
  private const string MessageTypeInPlayerIndex = "playerindex";
  private const string MessageTypeInGameStats = "gamestats";

  // public event CombatantFoundEvent CombatantFound;
  // public delegate void CombatantFoundEvent();
  private bool _isAwaitingCombatant = false;

  WebSocket websocket;

  async void Start() {
    websocket = new WebSocket(ServerUrl);

    websocket.OnOpen += () => {
      Debug.Log("Connection open!");
    };

    websocket.OnError += (e) => {
      Debug.Log("Error! " + e);
    };

    websocket.OnClose += (e) => {
      Debug.Log("Connection closed!");
    };

    websocket.OnMessage += (bytes) => {
      var jsonString = System.Text.Encoding.UTF8.GetString(bytes);
      Debug.Log("OnMessage: " + jsonString);
      JObject o = JObject.Parse(jsonString);
      string type = (string)o["type"];
      switch (type) {
        case MessageTypeInGameState:
          GameState gameState = JsonConvert.DeserializeObject<GameState>(((JObject)o["state"]).ToString());
          if (_isAwaitingCombatant) {
            _isAwaitingCombatant = false;
            gameController.NewGame(gameState);
          } else {
            gameController.Refresh(gameState);
          }
          break;
        case MessageTypeInPlayerId:
          gameController.SetPlayerId((string)o["playerid"]);
          break;
        case MessageTypeInPlayerIndex:
          gameController.SetPlayerIndex((int)o["playerindex"]);
          break;
        case MessageTypeInGameStats:
          GameStats gameStats = JsonConvert.DeserializeObject<GameStats>(((JObject)o["stats"]).ToString());
          gameController.EndGame(gameStats);
          break;
      }
    };

    await websocket.Connect();
  }

  void Update() {
#if !UNITY_WEBGL || UNITY_EDITOR
    if (websocket != null) {
      websocket.DispatchMessageQueue();
    }
#endif
  }

  private async void OnApplicationQuit() {
    if (websocket == null) {
      return;
    }
    await websocket.Close();
  }

  public void SendMessageJoinCombatantQueue(string name) {
    _isAwaitingCombatant = true;
    websocket.SendText(@"{
      ""type"": """ + MessageTypeOutLogin + @""",
      ""name"": """ + name + @"""
    }");
  }

  public void SendMessageSummon(UnitId unitId, DoubledCoord coord) {
    websocket.SendText(@"{
      ""type"": """ + MessageTypeOutSummon + @""",
      ""unitid"": " + (int)unitId + @",
      ""coord"": {
        ""col"": " + coord.col + @",
        ""row"": " + coord.row + @"
      }
    }");
  }

  public void SendMessageSkipTurn() {
    SendNoContentMessage(MessageTypeOutSkipMove);  
  }

  public void SendMessageMove(DoubledCoord from, DoubledCoord to) {
    SendFromToMessage(MessageTypeOutMove, from, to);
  }
  
  public void SendMessageAttack(DoubledCoord from, DoubledCoord to) {
    SendFromToMessage(MessageTypeOutAttack, from, to);
  }

  public void SendMessageGetGameState() {
    SendNoContentMessage(MessageTypeOutGameState);
  }

  private void SendNoContentMessage(string type) {
    websocket.SendText(@"{""type"": """ + type + @"""}");
  }
  
  private void SendFromToMessage(string type, DoubledCoord from, DoubledCoord to) {
    websocket.SendText(@"{
      ""type"": """ + type + @""",
      ""from"": {
        ""col"": " + from.col + @",
        ""row"": " + from.row + @"
      },
      ""to"": {
        ""col"": " + to.col + @",
        ""row"": " + to.row + @"
      }
    }");
  }
}
