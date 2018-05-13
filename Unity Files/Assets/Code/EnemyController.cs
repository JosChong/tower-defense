using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private GridController _grid;
    private Rigidbody _rb;
    private HealthController _hp;
    private int _damage;
    private int _money;
	private float _speed;
    private int _freezeCount;
    private float _freezeSpeed;
    private bool _frozen;
    private float _freezeDamageTime;
    private float _freezeTimeout;

	private List<GridController.Index> path;
	private int nextNode = 0;

    internal void Start()
    {
        _grid = FindObjectOfType<GridController>();
        _rb = GetComponent<Rigidbody>();
        _hp = GetComponent<HealthController>();
        _freezeDamageTime = 0f;
        _frozen = false;
    }

    internal void Update()
	{
		//if (_hp.GetHP() <= 0) { Die(true); }
	    _rb.velocity = GetVelocity();
		updateProgress();

	    if (_frozen)
	    {
	        HandleFreeze(); 
	    }

	}

	public void SetType(string type, int waveNumber)
	{
		var multiplier = Mathf.Pow(1.1f, waveNumber - 1);
		             
		switch(type)
		{
			case "normal":
				_damage = Mathf.RoundToInt(5 * multiplier);
				_money =  Mathf.RoundToInt(4 * multiplier);
				_hp.SetHP(Mathf.RoundToInt(20 * multiplier));
				_speed = 0.5f;
				break;
			case "fast":
				_damage = Mathf.RoundToInt(5 * multiplier);
				_money =  Mathf.RoundToInt(4 * multiplier);
				_hp.SetHP(Mathf.RoundToInt(10 * multiplier));
				_speed = 1f;
				break;
			case "strong":
				_damage = Mathf.RoundToInt(10 * multiplier);
				_money =  Mathf.RoundToInt(8 * multiplier);
				_hp.SetHP(Mathf.RoundToInt(50 * multiplier));
				_speed = 0.4f;
				break;
		}
	}

	public void initialize(List<GridController.Index> path)
	{
	    this.path = path;
    }

    /// <summary>
    /// Vector in direction of path to base
    /// </summary>
    /// <returns></returns>
    private Vector3 GetVelocity()
    {
        var speed = _frozen ? _freezeSpeed : _speed;
		var direction = (getNextPosition() - _rb.position).normalized * speed;
		direction.y = 0;
        return direction;
    }

	private Vector3 getNextPosition(){
		if(nextNode >= path.Count) return _rb.position;

		Vector3 nextPos = _grid.grid[path[nextNode].x, path[nextNode].y].transform.position;
		nextPos.y = transform.position.y;
		return nextPos;
	}

	private void updateProgress(){
		if((getNextPosition()-_rb.position).sqrMagnitude < 0.005f){
			nextNode++;
		}
	}

    public void NewPath(GridController.Index pos)
    {
        if (path[nextNode].x == pos.x && path[nextNode].y == pos.y)
            nextNode--;
        path = _grid.findEnemyPath(path[nextNode]);
        if (path == null) Die(false);
        nextNode = 0;
    }

    public void ApplyFreeze(int freezeCount)
    {
        _freezeCount = freezeCount;
        _freezeSpeed = _speed * Mathf.Pow(0.5f, (float) _freezeCount);
        _freezeTimeout = Time.time;
        _frozen = true;
    }

    private void HandleFreeze()
    {
        if (Time.time > _freezeTimeout + 0.1f)
        {
            _frozen = false;
        }

        if (Time.time > _freezeDamageTime + 0.5f)
        {
            DealDamage(1 * _freezeCount);
            _freezeDamageTime = Time.time;
        }
    }

    /// <summary>
    /// Something hit me
    /// </summary>
    /// <param name="colli"></param>
    internal void OnCollisionEnter(Collision colli)
    {
        var hitGameObject = colli.gameObject;
        if (hitGameObject.GetComponent<BaseController>() != null)
        {
            hitGameObject.GetComponent<BaseController>().DealDamage(_damage);
            Die(false);
        }
        if (hitGameObject.GetComponent<BulletController>() != null)
        {
            DealDamage(hitGameObject.GetComponent<BulletController>().Damage);
        }
	    if (hitGameObject.GetComponent<BasicTowerController>() != null ||
	        hitGameObject.GetComponent<ShockTowerController>() != null ||
	        hitGameObject.GetComponent<FreezeTowerController>() != null)
	    {
		    _rb.position = FindObjectOfType<SpawnerController>().transform.position;
	    }
    }
	
	public void DealDamage(int damage)
	{
		_hp.ChangeHP(-damage);
		if (_hp.GetHP() <= 0) { Die(true); }
	}

    /// <summary>
    /// We all die eventually
    /// </summary>
    private void Die(bool addMoney)
    {
        if (addMoney)
        {
            FindObjectOfType<MoneyController>().ChangeMoney(_money);
        }
        Destroy(gameObject);
    }
}
