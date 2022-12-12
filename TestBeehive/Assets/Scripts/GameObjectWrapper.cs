using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectWrapper : MonoBehaviour
{
    public GameObject frame;

    public GameObjectWrapper(GameObject _frame)
    {
        frame = _frame;
    }
}
