using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class CubeController : MonoBehaviour {

    public GameObject BasicTower;
    public GameObject FreezeTower;
    public GameObject ShockTower;
    public GameObject BuildMenu;
    public GameObject SellMenu;
    public GridController.Index Position;
    public int freezeCount;

    private GridController _grid;
    private SpawnerController _spawner;
    private BaseController _base;
    private MoneyController _money;
    private int _basicTowerCost = 25;
    private int _freezeTowerCost = 150;
    private int _shockTowerCost = 250;
    private float _speedMultiplier = 0.666f;

    private GameObject _go;

    public enum State
    {
        Empty,
        Objective,
        BasicTower,
        ShockTower,
        FreezeTower,
    }

    private State _state;

    void Start ()
    {
        _grid = FindObjectOfType<GridController>();
        _spawner = FindObjectOfType<SpawnerController>();
        _base = FindObjectOfType<BaseController>();
        _money = FindObjectOfType<MoneyController>();
        if (Position.Equals(_spawner.Position) || Position.Equals(_base.Position))
            _state = State.Objective;
        else _state = State.Empty;
    }

    void Update()
    {
        var pos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        var enemies = Physics.OverlapBox(pos, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, 1 << 8);
        if (_go != null && _state == State.Empty)
        {
            var buttons = _go.gameObject.GetComponentsInChildren<Button>();

            if (!_grid.NewPathExists(_spawner.Position, Position) || _money.GetMoney() < _basicTowerCost || enemies.Length != 0)
                foreach (var button in buttons)
                {
                    button.interactable = false;
                }
            else
            {
                buttons[0].interactable = true;
                buttons[1].interactable = _money.GetMoney() >= _freezeTowerCost;
                buttons[2].interactable = _money.GetMoney() >= _shockTowerCost;
            }
        }
        if (freezeCount > 0)
        {
            foreach (var enemy in enemies)
                enemy.GetComponent<EnemyController>().ApplyFreeze(freezeCount);

        }

    }

    void OnMouseDown()
    {
        ClickResponse();
    }

    public void ClickResponse()
    {
        if (_state == State.Objective || _grid.GameOver) return;
        if (_go != null)
        {
            Destroy(_go);
            _grid.Go = null;
            _go = null;
            return;
        }
        if (_grid.Go != null)
        {
            Destroy(_grid.Go);
            _grid.Go = null;
        }
        if (_state == State.Empty)
        {
            _go = (GameObject) Instantiate(BuildMenu, GameObject.Find("Canvas").transform);
            _grid.Go = _go;
            var buttons = _go.gameObject.GetComponentsInChildren<Button>();
            buttons[0].onClick.AddListener(CreateBasicTower);
            buttons[1].onClick.AddListener(CreateFreezeTower);
            buttons[2].onClick.AddListener(CreateShockTower);
            var pos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
            if (!_grid.NewPathExists(_spawner.Position, Position) || _money.GetMoney() < _basicTowerCost
                || Physics.OverlapBox(pos, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, 1 << 8).Length != 0)
                foreach (var button in buttons)
                {
                    button.interactable = false;
                }
            else
            {
                buttons[0].interactable = true;
                buttons[1].interactable = _money.GetMoney() >= _freezeTowerCost;
                buttons[2].interactable = _money.GetMoney() >= _shockTowerCost;
            }

            return;
        }

        if (_state == State.BasicTower || _state == State.ShockTower || _state == State.FreezeTower)
        {
            _go = (GameObject) Instantiate(SellMenu, GameObject.Find("Canvas").transform);
            _grid.Go = _go;
            var button = _go.gameObject.GetComponentInChildren<Button>();
            button.onClick.AddListener(DestroyTower);
            button.interactable = true;

            return;
        }
    }

    public void CreateBasicTower()
    {
        if (_money.GetMoney() < _basicTowerCost) return;
        _money.ChangeMoney(-_basicTowerCost);

        var tower = Instantiate(BasicTower, gameObject.transform);
        _grid.setObjectAtLocation(Position, tower);
        foreach (var enemy in FindObjectsOfType<EnemyController>())
            enemy.NewPath(Position);
        _state = State.BasicTower;
        Destroy(_go);
        _go = null;
    }
    
    public void CreateShockTower()
    {
        if (_money.GetMoney() < _shockTowerCost) return;
        _money.ChangeMoney(-_shockTowerCost);

        var tower = Instantiate(ShockTower, gameObject.transform);
        _grid.setObjectAtLocation(Position, tower);
        foreach (var enemy in FindObjectsOfType<EnemyController>())
            enemy.NewPath(Position);
        _state = State.ShockTower;
        Destroy(_go);
        _go = null;
    }

    public void CreateFreezeTower()
    {
        if (_money.GetMoney() < _freezeTowerCost) return;
        _money.ChangeMoney(-_freezeTowerCost);

        var tower = Instantiate(FreezeTower, gameObject.transform);
        _grid.setObjectAtLocation(Position, tower);
        foreach (var enemy in FindObjectsOfType<EnemyController>())
            enemy.NewPath(Position);
        _state = State.FreezeTower;
        foreach (var neighbor in _grid.validNeighbors(Position))
        {
            _grid.getCubeAtLocation(neighbor).GetComponent<CubeController>().freezeCount++;
        }

        Destroy(_go);
        _go = null;
    }

    public void DestroyTower()
    {
        switch (_state)
        {
            case State.BasicTower:
                _money.ChangeMoney(_basicTowerCost * 9 / 10);
                break;
            case State.ShockTower:
                _money.ChangeMoney(_shockTowerCost * 9 / 10);
                break;
            case State.FreezeTower:
                _money.ChangeMoney(_freezeTowerCost * 9 / 10);
                foreach (var neighbor in _grid.validNeighbors(Position))
                {
                    _grid.getCubeAtLocation(neighbor).GetComponent<CubeController>().freezeCount--;
                }
                break;
        }
        _grid.DestroyObjectAtLocation(Position);
        _state = State.Empty;
        Destroy(_go);
        _go = null;
    }
}
