using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
	private int _currentHP;
	
    /// <summary>
    /// Sets current HP to a given value
    /// </summary>
    /// <param name="HP"></param>
    public void SetHP(int HP)
    {
	    _currentHP = HP;
    }
	
	/// <summary>
	/// Changes current HP by a given value
	/// </summary>
	/// <param name="change"></param>
	public void ChangeHP(int change)
	{
		_currentHP += change;
	}

	/// <summary>
	///  Returns current HP
	/// </summary>
	/// <returns></returns>
	public int GetHP()
	{
		return _currentHP;
	}
}
