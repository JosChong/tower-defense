using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyController : MonoBehaviour
{
    private Text _text;
    private int _money;

	internal void Start ()
	{
	    _text = GetComponent<Text>();
	    _money = 100;
	}

    /// <summary>
    /// Changes the player's money
    /// </summary>
    /// <param name="amount"></param>
    public void ChangeMoney(int amount)
    {
        _money += amount;
        UpdateText();
    }

    public int GetMoney()
    {
        return _money;
    }

    private void UpdateText()
    {
        _text.text = "$" + _money;
    }
}
