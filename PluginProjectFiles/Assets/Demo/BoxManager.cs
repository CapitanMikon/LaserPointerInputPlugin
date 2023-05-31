using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxManager : MonoBehaviour
{

    [SerializeField] private GameObject[] boxesGameObjects;

    private List<Box> _boxes = new List<Box>();
    // Start is called before the first frame update
    void Start()
    {
        foreach (var b in boxesGameObjects)
        {
            _boxes.Add(b.GetComponent<Box>());
        }
    }

    public void OnResetTrigger()
    {
        foreach (var b in _boxes)
        {
            b.ResetBox();
        }
        Debug.LogError("Button Clicked!");
    }
}
