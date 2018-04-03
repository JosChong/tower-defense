using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class WaveInformationController : MonoBehaviour
{
	private Text _text;
	private Image _icon;
	private WaveController _wave;
	private float _phaseStart;
	private float[] _phaseDurations;
	private int _normalEnemies;
	private int _fastEnemies;
	private int _strongEnemies;
	private int _waveNumber;

	private Text _textNext;
	private Image _iconNext;
	private int _phaseState = -1;
	private int _normalEnemiesNext;
	private int _fastEnemiesNext;
	private int _strongEnemiesNext;

	internal void Initialize()
	{
		_text = GameObject.Find("Wave Text").GetComponent<Text>();
		_icon = GameObject.Find("Icon").GetComponent<Image>();
		_wave = FindObjectOfType<WaveController>();
		_phaseDurations = _wave.GetPhaseDurations();
		
		_textNext = GameObject.Find("Wave Text 2").GetComponent<Text>();
		_iconNext = GameObject.Find("Icon 2").GetComponent<Image>();
		
		GameObject.Find("Container").GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, 120);
	}

	internal void Update()
	{
		if (_phaseState != _wave.GetPhase())
		{
			_phaseStart = _wave.GetPhaseStart();
			_phaseState = _wave.GetPhase();
			_waveNumber = _wave.GetWaveNumber();
			if (_phaseState == 0 && _waveNumber <= _wave.GetWavesToWin())
			{
				SetWaveInformation();
			}
			else
			{
				ClearInformation();
			}
			if (_waveNumber < _wave.GetWavesToWin())
			{
				SetWaveInformationNext();
			}
			else
			{
				ClearInformationNext();
			}
		}
		if (_waveNumber <= _wave.GetWavesToWin()) UpdateWaveInformation();
		if (_waveNumber >= _wave.GetWavesToWin()) return;
		UpdateWaveInformationNext();
	}

	private void UpdateWaveInformation()
	{
		if (_phaseState == 1) return;
		var value = 1 - (Time.time - _phaseStart + _phaseDurations[0]) / (_phaseDurations[1]);
		_text.transform.position = new Vector3(value * Screen.width + 37.5f, 20, 0);
		_icon.transform.position = new Vector3(value * Screen.width + 37.5f, 0, 0);
	}

	private void SetWaveInformation()
	{
		var miniWaves = new Queue<int>(_wave.GetMiniWaves());
		var miniWaveTypes = new Queue<string>(_wave.GetMiniWaveTypes());
		_normalEnemies = 0;
		_fastEnemies = 0;
		_strongEnemies = 0;

		while (miniWaves.Count != 0)
		{
			var miniWave = miniWaves.Dequeue();
			var miniWaveType = miniWaveTypes.Dequeue();

			switch (miniWaveType)
			{
					case "normal":
						_normalEnemies += miniWave;
						break;
					case "fast":
						_fastEnemies += miniWave;
						break;
					case "strong":
						_strongEnemies += miniWave;
						break;
			}
		}

		_text.text = "Wave " + _waveNumber + "\nNormal x" + _normalEnemies + "\nFast x" + _fastEnemies + "\nStrong x" +
		             _strongEnemies;
	}

	private void ClearInformation()
	{
		_text.text = "";
		_icon.transform.position = new Vector3(0, -100, 0);
	}
	
	
	private void UpdateWaveInformationNext()
	{
		var progress = _phaseState == 1 ? _phaseDurations[2] : 0f;
		var value = 1 - (Time.time - _phaseStart + progress) / (_phaseDurations[1]);
		_textNext.transform.position = new Vector3(value * Screen.width + 37.5f, 20, 0);
		_iconNext.transform.position = new Vector3(value * Screen.width + 37.5f, 0, 0);
	}
	
	private void SetWaveInformationNext()
	{
		var miniWaves = new Queue<int>(_wave.GetMiniWavesNext());
		var miniWaveTypes = new Queue<string>(_wave.GetMiniWaveTypesNext());
		_normalEnemiesNext = 0;
		_fastEnemiesNext = 0;
		_strongEnemiesNext = 0;

		while (miniWaves.Count != 0)
		{
			var miniWave = miniWaves.Dequeue();
			var miniWaveType = miniWaveTypes.Dequeue();

			switch (miniWaveType)
			{
				case "normal":
					_normalEnemiesNext += miniWave;
					break;
				case "fast":
					_fastEnemiesNext += miniWave;
					break;
				case "strong":
					_strongEnemiesNext += miniWave;
					break;
			}
		}

		_textNext.text = "Wave " + (_waveNumber + 1) + "\nNormal x" + _normalEnemiesNext + "\nFast x" + _fastEnemiesNext + "\nStrong x" +
		             _strongEnemiesNext;
	}
	
	private void ClearInformationNext()
	{
		_textNext.text = "";
		_iconNext.transform.position = new Vector3(0, -100, 0);
	}
}
