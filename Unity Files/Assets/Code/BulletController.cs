using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BulletController : MonoBehaviour {

    public int Damage;
    private float _deathtime;
    private Rigidbody _rb;
    private float _speed;
    public EnemyController Target;

	private double life;

    void Start()
    {
        Damage = 5;
        _deathtime = 5f;
        _rb = GetComponent<Rigidbody>();
        _speed = 15f;

		life = 5;
    }

    void Update()
    {
        _deathtime -= Time.deltaTime;
        if (_deathtime <= 0) { Die(); }

        if (Target == null)
        {
            Die();
            return;
        }

		var direction = Target.transform.position - transform.position;
	    _rb.velocity = direction.normalized * _speed;

		life -= Time.deltaTime;

		if(life < 0) Die();

    }

    void OnCollisionEnter(Collision colli)
    {
        Die();
    }

    /// <summary>
    /// We all die eventually
    /// </summary>
    public void Die()
    {
        Destroy(gameObject);
    }
}
