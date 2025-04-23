using UnityEngine;
using TMPro;
using System;

public class RewardScreenManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI tmp1;
    [SerializeField]
    TextMeshProUGUI tmp2;
    [SerializeField]
    TextMeshProUGUI tmp3;

    public GameObject rewardUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            rewardUI.SetActive(true);

            tmp1.text = "Time Spent: " + Math.Round(GameManager.Instance.timeEnd - GameManager.Instance.timeStart, 2);
            tmp2.text = "Damage Recieved: " + GameManager.Instance.damageReceived;
            tmp3.text = "Damage Dealt: " + GameManager.Instance.damageDealt;
        
        }
        else
        {
            rewardUI.SetActive(false);
            // tmp1.SetActive(false);
        }
    }
}
