using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trolley : MonoBehaviour
{
    public PlayerController playerController;

    private Coroutine droppingCoroutine;
    private void OnTriggerEnter(Collider other)
    {
        GameObject collidedObject = other.gameObject;

        switch (collidedObject.tag)
        {
            case "Collectable":
                PickUpBrick(collidedObject);
                break;

            case "Station":
                StartDropingBrick(collidedObject);
                break;

            default:
                break;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        GameObject collidedObject = other.gameObject;

        switch (collidedObject.tag)
        {
            case "Station":
                StopDroppingBrick();
                break;

            default:
                break;
        }
    }
    private void PickUpBrick(GameObject brick)
    {
        if (!playerController.canPickUpBrick)
        {
            return;
        }
        brick.GetComponent<Collider>().enabled = false;
        Destroy(brick);
        playerController.PickUpBrick();
    }
    private void StartDropingBrick(GameObject stationObj)
    {
        var station = stationObj.GetComponent<Station>();
        if (station.isFull)
        {
            return;
        }

        droppingCoroutine = StartCoroutine(DropBricks(stationObj, 0.5f));
    }

    private void StopDroppingBrick()
    {
        if (droppingCoroutine != null)
        {
            StopCoroutine(droppingCoroutine);
        }
    }

    private IEnumerator DropBricks(GameObject stationObj, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        var station = stationObj.GetComponent<Station>();
        if (station.isFull)
        {
            yield break;
        }

        GameObject brick = playerController.DropBrick();
        if (brick == null)
        {
            yield break;
        }

        station.RecieveAmount(1);
        brick.GetComponent<Collectable>().GoToPos(stationObj.transform.position);

        yield return new WaitForEndOfFrame();
        droppingCoroutine = StartCoroutine(DropBricks(stationObj, 0.05f));
    }
}
