using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Transform target;

    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position;
    }
}
