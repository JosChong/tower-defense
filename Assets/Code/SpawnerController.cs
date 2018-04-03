using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    public GridController.Index Position;
	Dictionary<string, Object> _prefabs = new Dictionary<string, Object>();
    private GridController _grid;

	internal void Start()
	{
		_grid = FindObjectOfType<GridController>();

		_prefabs.Add("normal", Resources.Load("Enemy"));
		_prefabs.Add("fast", Resources.Load("FastEnemy"));
		_prefabs.Add("strong", Resources.Load("StrongEnemy"));
	}

	public void SpawnEnemy(string type, int waveNumber)
	{
	    if (_grid.GameOver) return;
		var enemy = (GameObject) Instantiate(_prefabs[type], gameObject.transform);
		enemy.GetComponent<EnemyController>().Start();
		enemy.GetComponent<EnemyController>().SetType(type, waveNumber);
		enemy.GetComponent<EnemyController>().initialize(_grid.findEnemyPath(Position));
	}
}
