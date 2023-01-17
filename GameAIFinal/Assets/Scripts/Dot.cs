using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : GridAligned {
    private const int POINT_VALUE = 100;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.tag == "PacMan") {
            collision.GetComponent<PacManStats>().Score += POINT_VALUE;
            Destroy(gameObject);
        }
    }
}
