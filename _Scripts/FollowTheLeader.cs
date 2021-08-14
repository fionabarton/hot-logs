using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTheLeader : MonoBehaviour {
    public GameObject   leader; // the game object to follow - assign in inspector
    public int          steps; // number of steps to stay behind - assign in inspector

    private Queue<Vector3>  record = new Queue<Vector3>();
    //private Vector3         lastRecord;

    //public float speed = 5;
    //public float stoppingDistance = 1;

    // DontDestroyOnLoad
    private  bool exists;

    void Awake() {
        // DontDestroyOnLoad
        if (!exists) {
            exists = true;
            DontDestroyOnLoad(transform.gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    void FixedUpdate() {
        // record position of leader
        if (Player.S.mode != eRPGMode.idle) {
            record.Enqueue(leader.transform.localPosition);
        }

        // remove last position from the record and use it for our own
        if (record.Count > steps) {
            transform.localPosition = record.Dequeue();
        }

        //if (Vector2.Distance(transform.localPosition, leader.transform.localPosition) > stoppingDistance) {
        //    transform.localPosition = Vector2.MoveTowards(transform.localPosition, leader.transform.localPosition, speed * Time.fixedDeltaTime);
        //}
    }
}
