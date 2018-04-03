using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
	private float _phaseStart;
	private int _phaseState = 0; // 0 is break phase, 1 is wave phase
	private float[] _phaseDuration = {5f, 16f}; // [0] is break duration, [1] is wave duration
	private int _waveNumber = 1;
	private int _wavesToWin = 9;
	private Wave _nextWave;
	private Wave _nextNextWave;
	private Queue<Wave> _waves = new Queue<Wave>();
	private SpawnerController _spawner;

	private class Wave
	{
		public Queue<int> MiniWaves = new Queue<int>();
		public Queue<string> MiniWaveTypes = new Queue<string>();
		
		public Wave(int one, string onetype, int two = 0, string twotype = null, int three = 0, string threetype = null)
		{
			MiniWaves.Enqueue(one);
			MiniWaveTypes.Enqueue(onetype);
			if (two != 0) MiniWaves.Enqueue(two);
			if (twotype != null) MiniWaveTypes.Enqueue(twotype);
			if (three != 0) MiniWaves.Enqueue(three);
			if (threetype != null) MiniWaveTypes.Enqueue(threetype);
		}
	}

	private void InitializeWaves()
	{
		_waves.Enqueue(new Wave(2, "normal", 2, "normal", 1, "normal"));
		_waves.Enqueue(new Wave(4, "normal", 3, "normal", 3, "normal"));
		_waves.Enqueue(new Wave(5, "normal", 5, "fast", 5, "normal"));
		_waves.Enqueue(new Wave(5, "normal", 2, "strong", 5, "normal"));
		_waves.Enqueue(new Wave(10, "normal", 4, "strong", 10, "normal"));
		_waves.Enqueue(new Wave(10, "normal", 4, "strong", 10, "fast"));
		_waves.Enqueue(new Wave(10, "fast", 4, "strong", 10, "fast"));
		_waves.Enqueue(new Wave(10, "fast", 8, "strong", 10, "fast"));
		_waves.Enqueue(new Wave(8, "strong", 10, "fast", 8, "strong"));
	}
	
	internal void Start ()
	{
		_spawner = GetComponent<SpawnerController>();
		InitializeWaves();
		_nextWave = _waves.Dequeue();
		_nextNextWave = _waves.Peek();
		FindObjectOfType<WaveInformationController>().Initialize();
		_phaseStart = Time.time;
	}
	
	internal void Update ()
	{
		if (_waveNumber > _wavesToWin && FindObjectsOfType<EnemyController>().Length == 0)
		{
			FindObjectOfType<GridController>().EndGame("victory");
		}
		if (Time.time < _phaseStart + _phaseDuration[_phaseState]) return;
		
		_phaseState = -_phaseState + 1;
		if (_phaseState == 1 && _waveNumber <= _wavesToWin) StartCoroutine(SendWave());
		_phaseStart = Time.time;
		
		/*
		if (_waveNumber > _wavesToWin && FindObjectsOfType<EnemyController>().Length == 0)
		{
			StopAllCoroutines();
			if (FindObjectOfType<BaseController>().HP <= 0) return;
			FindObjectOfType<GridController>().EndGame("victory");
			return;
		}
		*/
	}

	private IEnumerator SendWave()
	{
		while (_nextWave.MiniWaves.Count != 0)
		{
			var miniWave = _nextWave.MiniWaves.Dequeue();
			var miniWaveType = _nextWave.MiniWaveTypes.Dequeue();
			yield return StartCoroutine(SendMiniWave(miniWave, miniWaveType));
		}

		if (_waves.Count > 0)
		{
			_nextWave = _waves.Dequeue();
			_nextNextWave = _waves.Count > 0 ? _waves.Peek() : null;
		}
		_waveNumber++;
		
	}

	private IEnumerator SendMiniWave(int miniWave, string type)
	{
		for (var s = 0; s < 5; s++)
		{
			var delay = 0f;
			for (var e = 0; e < miniWave / 5; e++)
			{
				_spawner.SpawnEnemy(type, _waveNumber);
				yield return new WaitForSeconds(0.33f);
				delay += 0.33f;
			}
			if (miniWave % 5 > 0)
			{
				_spawner.SpawnEnemy(type, _waveNumber);
				miniWave--;
			}
			yield return new WaitForSeconds(1f - delay);
		}
	}

	public int GetPhase()
	{
		return _phaseState;
	}

	public float GetPhaseStart()
	{
		return _phaseStart;
	}

	public float[] GetPhaseDurations()
	{
		return new float[] {_phaseDuration[0] + _phaseDuration[1], 2 * _phaseDuration[0] + _phaseDuration[1], _phaseDuration[0]};
	}
	
	public int GetWaveNumber()
	{
		return _waveNumber;
	}

	public Queue<int> GetMiniWaves()
	{
		return _nextWave.MiniWaves;
	}
	
	public Queue<string> GetMiniWaveTypes()
	{
		return _nextWave.MiniWaveTypes;
	}
	
	public int GetWaveNumberNext()
	{
		return _waveNumber + 1;
	}
	
	public Queue<int> GetMiniWavesNext()
	{
		return _nextNextWave.MiniWaves;
	}
	
	public Queue<string> GetMiniWaveTypesNext()
	{
		return _nextNextWave.MiniWaveTypes;
	}

	public int GetWavesToWin()
	{
		return _wavesToWin;
	}
}
