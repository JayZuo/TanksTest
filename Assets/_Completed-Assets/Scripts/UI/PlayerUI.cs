using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Tooltip("UI Text to display Player's Name")]
    public GameObject PlayerUiPrefab;

    [Tooltip("Pixel offset from the player target")]
    public Vector3 ScreenOffset = new Vector3(0f, 30f, 0f);

    readonly float tankHeight = 1.7f;
    GameObject thisTank;
    GameObject UI;

    void Awake()
    {
        thisTank = gameObject;
    }

    // Use this for initialization
    void Start()
    {
        UI = Instantiate(this.PlayerUiPrefab);
        UI.GetComponent<Text>().text = thisTank.GetComponent<PhotonView>().owner.NickName;
        UI.transform.SetParent(GameObject.Find("MessageCanvas").GetComponent<Transform>());
    }

    void OnDisable()
    {
        print("UI DISABLE");
        UI.SetActive(false);
    }

    void OnEnable()
    {
        if (UI != null)
        {
            UI.SetActive(true);

        }
    }

    void LateUpdate()
    {
        Vector3 _targetPosition = thisTank.transform.position;
        _targetPosition.y += tankHeight;
        UI.transform.position = Camera.main.WorldToScreenPoint(_targetPosition) + ScreenOffset;
    }
}
