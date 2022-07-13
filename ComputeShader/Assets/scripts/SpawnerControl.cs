using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerControl : MonoBehaviour
{ 
    void Start()
    {
        this.spheres = new Sphere[ObjCount * ObjCount];
        this.gobjSpheres = new GameObject[ObjCount * ObjCount];
        this.finished = new bool[ObjCount * ObjCount];
        for (int i = 0, k = 0; i < ObjCount; i++)
        {
            float offsetX = (-ObjCount / 2 + i);
            for (int j = 0; j < ObjCount; j++, k++)
            {
                float offsetZ = (-ObjCount / 2 + j);
                gobjSpheres[k] = Instantiate(obj);
                gobjSpheres[k].transform.position = new Vector3(offsetX * distance + .5f, transform.position.y, offsetZ * distance + .5f);
                spheres[k].position = gobjSpheres[i].transform.position;
                spheres[k].radius = .5f;
                spheres[k].direction = Vector3.down;
                spheres[k].speed = Random.Range(MinSpeed, MaxSpeed);
                spheres[k].weight = Random.Range(MinWeight, MaxWeight);
                finished[k] = false;

            }
        }
        int totalSize = sizeof(float) * 3 + sizeof(float) * 3 + sizeof(float) + sizeof(float) * 3 + sizeof(float) + sizeof(float);
        computeBuffer = new ComputeBuffer(spheres.Length, totalSize);
        computeBuffer.SetData(spheres);


        computeShader.SetBuffer(0, "objs", computeBuffer);
        computeShader.SetFloat("floor", floor.GetComponent<Transform>().position.y);
        computeShader.SetFloat("height", transform.position.y);
    }
}
