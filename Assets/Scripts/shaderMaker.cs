using UnityEngine;

public class shaderMaker : MonoBehaviour
{
    public Shader newShader; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera cam = GetComponent<Camera>();
        cam.SetReplacementShader (newShader, "RenderType");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
