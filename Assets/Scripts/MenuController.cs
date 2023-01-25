using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuController : MonoBehaviour {
  public enum MenuScreen {
    Main,
    Settings,
    Multiplayer,
    Lobby,
    Join,
    Host,
    QuickPlay,
    GameOver,
  }

  public GameObject mainScreen;
  public GameObject settingsScreen;
  public GameObject multiplayerScreen;
  public GameObject lobbyScreen;
  public GameObject joinScreen;
  public GameObject hostScreen;
  public GameObject quickPlayScreen;
  public GameObject gameOverScreen;

  public MenuScreen currentScreenIndex;
  private MenuScreen loadedScreenIndex;

  GameObject GetScreenGameObjectFromEnum(MenuScreen screenIndex) {
    switch (screenIndex) {
      case MenuScreen.Main: return mainScreen;
      case MenuScreen.Settings: return settingsScreen;
      case MenuScreen.Multiplayer: return multiplayerScreen;
      case MenuScreen.Lobby: return lobbyScreen;
      case MenuScreen.Join: return joinScreen;
      case MenuScreen.Host: return hostScreen;
      case MenuScreen.QuickPlay: return quickPlayScreen;
      case MenuScreen.GameOver: return gameOverScreen;
      default: return mainScreen;
    }
  }

  public void LoadScreenClass(MenuScreenChangeComponent c) {
    LoadScreen(c.screen);
  }

  public void Exit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }

  public void LoadScreen(MenuScreen screenIndex) {
    GameObject nextScreen = GetScreenGameObjectFromEnum(screenIndex);
    if (nextScreen == null) {
      Debug.Log("No game object associated with screen: " + screenIndex);
      return;
    }

    HideAll();

    if (nextScreen != null) {
      nextScreen.SetActive(true);
    }

    currentScreenIndex = screenIndex;
    loadedScreenIndex = screenIndex;
  }

  void OnValidate() {
    if (currentScreenIndex != loadedScreenIndex) {
      LoadScreen(currentScreenIndex);
    }
  }

  public void HideAll() {
    if (mainScreen != null) {
      mainScreen.SetActive(false);
    }

    if (settingsScreen != null) {
      settingsScreen.SetActive(false);
    }

    if (multiplayerScreen != null) {
      multiplayerScreen.SetActive(false);
    }

    if (lobbyScreen != null) {
      lobbyScreen.SetActive(false);
    }

    if (joinScreen != null) {
      joinScreen.SetActive(false);
    }

    if (hostScreen != null) {
      hostScreen.SetActive(false);
    }

    if (quickPlayScreen != null) {
      quickPlayScreen.SetActive(false);
    }

    if (gameOverScreen != null) {
      gameOverScreen.SetActive(false);
    }
  }
}
