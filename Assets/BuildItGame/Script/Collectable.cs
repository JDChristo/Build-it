using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public CollectableType type;

    internal void GoToPos(Vector3 position)
    {
        transform.LeanMove(position, 0.3f).setEaseOutCubic().setOnComplete(() =>
        {
            Destroy(this.gameObject);
        });
    }
}
