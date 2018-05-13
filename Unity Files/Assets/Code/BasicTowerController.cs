using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTowerController : MonoBehaviour {

    public GridController.Index Position;
    private Transform _cap;
    //private Transform _barrel;
    private float _lastTime;
    private float _waitTime;
    private Object _bullet;

    void Start()
    {
        _cap = gameObject.transform.GetChild(1);
        //_barrel = _cap.GetChild(0);
        _lastTime = 0f;
        _waitTime = 1.5f;
        _bullet = Resources.Load("Bullet");
    }

    void Update()
    {
        EnemyController closestMonster = FindClosestMonster();
        if (closestMonster == null) { return; }

        Vector3 direction = closestMonster.transform.position - _cap.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        _cap.rotation = rotation;

        if (Time.time < _lastTime + _waitTime) { return; }
        Fire(closestMonster, direction);
    }

    private EnemyController FindClosestMonster()
    {
        var monsters = FindObjectsOfType<EnemyController>();
        EnemyController closestMonster = null;
        Vector3 pos = _cap.position;
        foreach (var monster in monsters)
        {
            if (closestMonster == null)
            {
                if (Vector3.Distance(pos, monster.gameObject.transform.position) < 3)
                {
                    closestMonster = monster;
                }
            }
            else if (Vector3.Distance(pos, monster.transform.position) <=
                Vector3.Distance(pos, closestMonster.transform.position))
            {
                closestMonster = monster;
            }
        }
        return closestMonster;
    }

    private void Fire(EnemyController target, Vector3 direction)
    {
        //makes bullets a child of their respective barrels for object heirarchy cleanliness
        //uncomment lines in variable declarations and Start() as well
        //var bullet = (GameObject)Instantiate(_bullet, _cap.position, new Quaternion(), _barrel.transform);
        var bullet = (GameObject)Instantiate(_bullet, _cap.position, new Quaternion());
        bullet.GetComponent<BulletController>().Target = target;
        _lastTime = Time.time;
    }
    
    void OnMouseDown()
    {
        transform.parent.gameObject.GetComponent<CubeController>().ClickResponse();
    }
}
