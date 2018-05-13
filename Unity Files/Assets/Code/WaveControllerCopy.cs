using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveControllerCopy : MonoBehaviour
{
	private float _phaseStart;
	private int _phaseState = 0; // 0 is break phase, 1 is wave phase
	private float[] _phaseDuration = {10f, 40f}; // [0] is break duration, [1] is wave duration
	private int _waveNumber = 1;
	private Wave _nextWave;
	private SpawnerController _spawner;

	private int _wavesToWin = 5;
	private Wave _nextNextWave;

	private class Wave
	{
		public int[] MiniWaves = new int[6];
		
		public Wave(int waveNumber)
		{
			MiniWaves[0] = MiniWaves[2] = MiniWaves[4] = waveNumber + 4;
			MiniWaves[1] = MiniWaves[5] = (waveNumber / 2) * 2;
			MiniWaves[3] = waveNumber;
		}
	}
	
	internal void Start ()
	{
		_phaseStart = Time.time;
		_nextWave = new Wave(_waveNumber);
		_spawner = GetComponent<SpawnerController>();
		FindObjectOfType<WaveInformationController>().Initialize();
		
		_nextNextWave = new Wave(_waveNumber + 1);
	}
	
	internal void Update ()
	{
		if (Time.time < _phaseStart + _phaseDuration[_phaseState]) return;
		
		_phaseState = -_phaseState + 1;
		if (_phaseState == 1) StartCoroutine(SendWave());
		else if (_waveNumber > _wavesToWin && FindObjectsOfType<EnemyController>().Length == 0) FindObjectOfType<GridController>().EndGame("victory");
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
		yield return StartCoroutine(SendMiniWave(0));
		yield return StartCoroutine(SendMiniWave(1, "fast"));
		yield return StartCoroutine(SendMiniWave(2));
		yield return StartCoroutine(SendMiniWave(3, "strong"));
		yield return StartCoroutine(SendMiniWave(4));
		yield return StartCoroutine(SendMiniWave(5, "fast"));
		_nextWave = new Wave(++_waveNumber);
		
		_nextNextWave = new Wave(_waveNumber + 1);
	}

	private IEnumerator SendMiniWave(int miniWave, string type = "normal")
	{
		for (var s = 0; s < 5; s++)
		{
			var delay = 0f;
			for (var e = 0; e < _nextWave.MiniWaves[miniWave] / 5; e++)
			{
				_spawner.SpawnEnemy(type, _waveNumber);
				yield return new WaitForSeconds(0.25f);
				delay += 0.25f;
			}
			if (_nextWave.MiniWaves[miniWave] % 5 > 0)
			{
				_spawner.SpawnEnemy(type, _waveNumber);
				_nextWave.MiniWaves[miniWave]--;
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
		return new float[] {_phaseDuration[0] + _phaseDuration[1], 2 * _phaseDuration[0] + _phaseDuration[1]};
	}
	
	public int GetWaveNumber()
	{
		return _waveNumber;
	}

	public int[] GetMiniWaves()
	{
		return _nextWave.MiniWaves;
	}
	
	public int GetWaveNumberNext()
	{
		return _waveNumber + 1;
	}
	
	public int[] GetMiniWavesNext()
	{
		return _nextNextWave.MiniWaves;
	}

	public int GetWavesToWin()
	{
		return _wavesToWin;
	}
}
