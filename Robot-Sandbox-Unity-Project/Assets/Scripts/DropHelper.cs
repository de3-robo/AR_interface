using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropHelper : MonoBehaviour
{

    public void DoDrop()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.freezeRotation = false;
        rb.useGravity = true;
    }

    public void DoNotDrop()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.rotation = Quaternion.Euler(-90f, 0f, 0f);
        rb.freezeRotation = true;

        this.transform.position = new Vector3(0f, 0.65f, -0.5f);
    }

}
