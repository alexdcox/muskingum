using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using System;

public class GameStats {
  public int id;
  public int turns;
  public Int64 started;
  public Int64 ended;
  public int winner;
  public List<PlayerStats> player = new();
}

public class PlayerStats {
  public int unitsSummoned;
  public int unitsLost;
  public int damageDealt;
  public int damageReceived;
  public int energyGained;
  public int energySpent;
  public Dictionary<int, int> drawCostDistribution = new();
}

public class GameOver : MonoBehaviour {
  public TMP_Text subtitle;
  public Color p1Color;
  public Color p2Color;

  public TMP_Text player1UnitsSummoned;
  public TMP_Text player1UnitsLost;
  public TMP_Text player1DamageDealt;
  public TMP_Text player1DamageReceived;
  public TMP_Text player1EnergyGained;
  public TMP_Text player1EnergySpent;
  public Slider player1Cost1Slider;
  public Slider player1Cost2Slider;
  public Slider player1Cost3Slider;
  public Slider player1Cost4Slider;
  public Slider player1Cost5Slider;
  public Slider player1Cost6Slider;

  public TMP_Text player2UnitsSummoned;
  public TMP_Text player2UnitsLost;
  public TMP_Text player2DamageDealt;
  public TMP_Text player2DamageReceived;
  public TMP_Text player2EnergyGained;
  public TMP_Text player2EnergySpent;
  public Slider player2Cost1Slider;
  public Slider player2Cost2Slider;
  public Slider player2Cost3Slider;
  public Slider player2Cost4Slider;
  public Slider player2Cost5Slider;
  public Slider player2Cost6Slider;

  void Start() {
    // string stats = @"{""game"":{""id"":0,""length"":230,""turns"":0},""player"":[{""unitsSummoned"":6,""unitsLost"":2,""damageDealt"":16,""damageReceived"":8,""energyGained"":14,""energySpent"":14,""drawCostDistribution"":{""1"":2,""2"":8,""3"":4,""4"":3,""5"":0,""6"":0}},{""unitsSummoned"":6,""unitsLost"":2,""damageDealt"":16,""damageReceived"":8,""energyGained"":14,""energySpent"":14,""drawCostDistribution"":{""1"":2,""2"":8,""3"":4,""4"":3,""5"":0,""6"":0}}]}";
    // GameStats gameStats = JsonConvert.DeserializeObject<GameStats>(stats);
    // RenderGameStats(gameStats);
  }

  public void RenderGameStats(GameStats gameStats) {
    if (gameStats.winner == 0) {
      subtitle.text = "P1 Wins!";
      subtitle.color = p1Color;
    } else {
      subtitle.text = "P2 Wins!";
      subtitle.color = p2Color;
    }
    
    player1UnitsSummoned.text = gameStats.player[0].unitsSummoned.ToString();
    player1UnitsLost.text = gameStats.player[0].unitsLost.ToString();
    player1DamageDealt.text = gameStats.player[0].damageDealt.ToString();
    player1DamageReceived.text = gameStats.player[0].damageReceived.ToString();
    player1EnergyGained.text = gameStats.player[0].energyGained.ToString();
    player1EnergySpent.text = gameStats.player[0].energySpent.ToString();

    float player1CostTotal = 0;
    foreach (KeyValuePair<int, int> entry in gameStats.player[0].drawCostDistribution) {
      player1CostTotal = Math.Max(player1CostTotal, entry.Value);
    }

    player1Cost1Slider.value = Math.Max((float)gameStats.player[0].drawCostDistribution[1] / player1CostTotal, 0.02f);
    player1Cost2Slider.value = Math.Max((float)gameStats.player[0].drawCostDistribution[2] / player1CostTotal, 0.02f);
    player1Cost3Slider.value = Math.Max((float)gameStats.player[0].drawCostDistribution[3] / player1CostTotal, 0.02f);
    player1Cost4Slider.value = Math.Max((float)gameStats.player[0].drawCostDistribution[4] / player1CostTotal, 0.02f);
    player1Cost5Slider.value = Math.Max((float)gameStats.player[0].drawCostDistribution[5] / player1CostTotal, 0.02f);
    player1Cost6Slider.value = Math.Max((float)gameStats.player[0].drawCostDistribution[6] / player1CostTotal, 0.02f);

    player2UnitsSummoned.text = gameStats.player[1].unitsSummoned.ToString();
    player2UnitsLost.text = gameStats.player[1].unitsLost.ToString();
    player2DamageDealt.text = gameStats.player[1].damageDealt.ToString();
    player2DamageReceived.text = gameStats.player[1].damageReceived.ToString();
    player2EnergyGained.text = gameStats.player[1].energyGained.ToString();
    player2EnergySpent.text = gameStats.player[1].energySpent.ToString();

    float player2CostTotal = 0;
    foreach (KeyValuePair<int, int> entry in gameStats.player[1].drawCostDistribution) {
      player2CostTotal = Math.Max(player2CostTotal, entry.Value);
    }

    player2Cost1Slider.value = Math.Max((float)gameStats.player[1].drawCostDistribution[1] / player2CostTotal, 0.02f);
    player2Cost2Slider.value = Math.Max((float)gameStats.player[1].drawCostDistribution[2] / player2CostTotal, 0.02f);
    player2Cost3Slider.value = Math.Max((float)gameStats.player[1].drawCostDistribution[3] / player2CostTotal, 0.02f);
    player2Cost4Slider.value = Math.Max((float)gameStats.player[1].drawCostDistribution[4] / player2CostTotal, 0.02f);
    player2Cost5Slider.value = Math.Max((float)gameStats.player[1].drawCostDistribution[5] / player2CostTotal, 0.02f);
    player2Cost6Slider.value = Math.Max((float)gameStats.player[1].drawCostDistribution[6] / player2CostTotal, 0.02f);
  }
}
