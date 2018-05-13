using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockTowerController : MonoBehaviour
{
	public GridController.Index Position;
	private Transform _cap;
	private float _lastTime, _waitTime, _shootTime;
	private int _damage;
	private LineRenderer[] _lineRenderers = new LineRenderer[3];

	internal void Start()
	{
		_cap = gameObject.transform.GetChild(1);
		_lastTime = 0f;
		_waitTime = 3f;
		_shootTime = 0.1f;
		_damage = 25;

		_lineRenderers[0] = GetComponent<LineRenderer>();
		_lineRenderers[1] = gameObject.transform.GetChild(0).GetComponent<LineRenderer>();
		_lineRenderers[2] = _cap.GetComponent<LineRenderer>();
	}

	internal void Update()
	{
		var closestEnemies = FindClosestEnemies();
		if (closestEnemies.Count == 0)
		{
			ClearLines(0);
			return;
		}
		
		Vector3 direction = closestEnemies[0].transform.position - _cap.position;
		Quaternion rotation = Quaternion.LookRotation(direction);
		_cap.rotation = rotation;

		if (Time.time < _lastTime + _waitTime) return;
		StartCoroutine(Shock(closestEnemies));
	}

	private IEnumerator Shock(List<EnemyController> closestEnemies)
	{
		var enemies = closestEnemies;
		_lastTime = Time.time;

		while (Time.time < _lastTime + _shootTime)
		{
			var i = 0;
			var lastPosition = _cap.position;
			foreach (var enemy in enemies)
			{
				if (enemy != null)
				{
					_lineRenderers[i].SetPosition(0, lastPosition);
					_lineRenderers[i++].SetPosition(1, enemy.gameObject.transform.position);
					lastPosition = enemy.gameObject.transform.position;
				}
			}
			if (i < 3)
			{
				ClearLines(i);
			}
			yield return null;
		}
		
		foreach (var enemy in enemies)
		{
			if (enemy != null)
			{
				enemy.gameObject.GetComponent<EnemyController>().DealDamage(_damage);
			}
		}
		ClearLines(0);
	}

	private void ClearLines(int i)
	{
		for (var x = i; x < 3; x++)
		{
			_lineRenderers[x].SetPosition(0, Vector3.zero);
			_lineRenderers[x].SetPosition(1, Vector3.zero);
		}
	}
	
	private List<EnemyController> FindClosestEnemies()
	{
		var enemies = new List<EnemyController>();
		
		for (var i = 0; i < 3; i++)
		{
			if (i == 0 || enemies.Count == 0)
			{
				if (FindClosestEnemy(_cap, enemies) != null)
				{
					enemies.Add(FindClosestEnemy(_cap, enemies));
				}
			}
			else
			{
				if (FindClosestEnemy(enemies[enemies.Count-1].gameObject.transform, enemies) != null)
				{
					enemies.Add(FindClosestEnemy(enemies[enemies.Count-1].gameObject.transform, enemies));
				}
			}
		}

		return enemies;
	}

	private EnemyController FindClosestEnemy(Transform go, List<EnemyController> targetedEnemies)
	{
		var enemies = FindObjectsOfType<EnemyController>();
		EnemyController closestEnemy = null;
		Vector3 pos = go.position;
		foreach (var enemy in enemies)
		{
			if (go.gameObject.GetComponent<EnemyController>() != enemy && !targetedEnemies.Contains(enemy))
			{
				if (closestEnemy == null)
				{
					if (Vector3.Distance(pos, enemy.gameObject.transform.position) < 2)
					{
						closestEnemy = enemy;
					}
				}
				else if (Vector3.Distance(pos, enemy.transform.position) <=
				    Vector3.Distance(pos, closestEnemy.transform.position))
				{
					closestEnemy = enemy;
				}
			}
		}
		return closestEnemy;
	}
	
	void OnMouseDown()
	{
		transform.parent.gameObject.GetComponent<CubeController>().ClickResponse();
	}
}
