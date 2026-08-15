using UnityEngine;

public class TestComponent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(TestDLLImport.add(5, 10));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
