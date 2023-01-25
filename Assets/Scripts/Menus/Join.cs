using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using TMPro;

public class Join : MonoBehaviour {
  private bool isLoading = true;
  private HostInfo[] hosts;

  private Color defaultColor = new Color32(226, 226, 226, 255);
  private Color selectedColor = new Color32(128, 199, 85, 255);

  public GameObject hostInfoPrefab;
  public GameObject hostInfoList;

  private int paginationPerPage = 5;
  private int paginationCurrentPage = 0;
  private int paginationTotalRecords;
  private int paginationPageCount;
  private int paginationSelectedIndex = 0;

  private MenuInputActions menuInputActions;

  void Start() {
    // fetch lobby info
    string jsonResponse = @"[{
        ""id"": ""abc"",
        ""name"": ""Xodus"",
        ""won"": ""4"",
        ""lost"": ""4""
    },{
        ""id"": ""sjh"",
        ""name"": ""Paraquuen"",
        ""won"": ""2"",
        ""lost"": ""91""
    }]";

    List<HostInfo> hostInfo = JsonConvert.DeserializeObject<List<HostInfo>>(jsonResponse);

    paginationTotalRecords = hostInfo.Count;
    paginationCurrentPage = 0;
    paginationSelectedIndex = 0;

    SetHosts(hostInfo);
  }

  void OnEnable() {
    menuInputActions = new MenuInputActions();
    menuInputActions.Enable();
    menuInputActions.Main.Up.performed += context => {
      PreviousSelection();
    };
    menuInputActions.Main.Down.performed += context => {
      NextSelection();
    };
    // menuInputActions.Main.Right.performed += context => {
    //   PreviousPage();
    // };
    // menuInputActions.Main.Left.performed += context => {
    //   NextPage();
    // };
    menuInputActions.Main.Select.performed += context => {
      // TODO: Join the highlighted player or
    };
  }

  void NextPage() {

  }

  void PreviousPage() {

  }

  void NextSelection() {

  }

  void PreviousSelection() {

  }

  void SetHosts(List<HostInfo> hostInfo) {
    Debug.Log("setting hosts... " + hostInfo.Count);
    foreach (Transform t in hostInfoList.transform) {
      if (t.name == "Subtitle") {
        int paginationHostsDisplayed = (paginationPerPage * (paginationCurrentPage + 1));
        if (paginationHostsDisplayed > paginationTotalRecords) {
          paginationHostsDisplayed = paginationTotalRecords;
        }
        t.GetComponent<TMP_Text>().text = String.Format(
            "Hosts ({0}/{1})",
            paginationHostsDisplayed,
            paginationTotalRecords
        );
      } else {
        Destroy(t.gameObject);
      }
    }
    var index = 0;
    foreach (HostInfo hi in hostInfo) {
      Color color = defaultColor;
      var selectionIndex = (paginationCurrentPage * paginationPerPage) + index;
      if (selectionIndex == paginationSelectedIndex) {
        color = selectedColor;
      }

      GameObject hiObj = Instantiate(hostInfoPrefab, hostInfoList.transform);
      var nameText = hiObj.transform.Find("Name").GetComponent<TMP_Text>();
      nameText.text = hi.name;
      nameText.color = defaultColor;
      var statsText = hiObj.transform.Find("Stats").GetComponent<TMP_Text>();
      statsText.text = String.Format("W:{0} L:{1}", hi.won, hi.lost);
      statsText.color = defaultColor;

      index++;
    }
  }
}
