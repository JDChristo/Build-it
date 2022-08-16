using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Station : MonoBehaviour
{
    public CollectableType type;

    public int originalAmount;
    private int requiredAmount;
    public TextMeshProUGUI amountText;
    public Image fillIndicator;
    public GameObject finalModel;
    public bool isFull;

    private void Awake()
    {
        isFull = false;
        amountText.text = originalAmount.ToString();
        finalModel.SetActive(isFull);
    }

    public void RecieveAmount(int amount)
    {
        if (requiredAmount >= originalAmount)
        {
            return;
        }
        requiredAmount += amount;
        isFull = requiredAmount >= originalAmount;

        amountText.text = (originalAmount - requiredAmount).ToString();
        fillIndicator.fillAmount = requiredAmount / (float)originalAmount;
        if (isFull)
        {
            DisableStation();
        }
    }

    public void DisableStation()
    {
        GetComponent<Collider>().enabled = false;
        fillIndicator.gameObject.SetActive(false);
        amountText.gameObject.SetActive(false);

        var finalScale = finalModel.transform.localScale;
        finalModel.transform.localScale = Vector3.zero;
        finalModel.SetActive(true);
        finalModel.LeanScale(finalScale, 1f).setEaseOutBounce().setDelay(0.2f);
    }

}
