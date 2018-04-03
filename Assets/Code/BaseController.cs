using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseController : MonoBehaviour
{
    public GridController.Index Position;
    public int HP;
    private HealthController _hp;
    private Slider _healthBar;
    private int _baseHealth = 100;

    internal void Start()
    {
        _hp = GetComponent<HealthController>();
        _healthBar = GameObject.Find("Health").GetComponent<Slider>();
        
        _hp.SetHP(_baseHealth);
    }

    void Update()
    {
        HP = _hp.GetHP();
    }

    /// <summary>
    /// Deals damage to the base
    /// </summary>
    /// <param name="damage"></param>
    public void DealDamage(int damage)
    {
        _hp.ChangeHP(-damage);
        if (_hp.GetHP() <= 0)
        {
            FindObjectOfType<GridController>().EndGame("defeat");
        }
        _healthBar.value = (float) _hp.GetHP() / _baseHealth * 100;
    }
}
