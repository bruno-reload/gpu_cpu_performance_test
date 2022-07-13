using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerControl : MonoBehaviour
{ 
    public ComputeShader computeShader;
    public int ObjCount = 20;
    public GameObject floor;
    public float MaxSpeed = 10;
    public float MinSpeed = 1;
    public float MaxWeight = 10;
    public float MinWeight = 1;
    public GameObject obj;
    private float distance = 1;
    private bool[] finished;
    private Sphere[] spheres;
    private GameObject[] gobjSpheres;
    private ComputeBuffer computeBuffer;
    private float time;
    private bool gpu = false;
    private bool cpu;
    private float delta;
    private float count;
    private bool avaliableSpeedup = true;
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

    void Update()
    {
        count = Time.realtimeSinceStartup;
        if (gpu)
        {
            time += Time.deltaTime;
            computeShader.SetFloat("tf", time);
            computeShader.Dispatch(0, spheres.Length / 10, 1, 1);

            computeBuffer.GetData(spheres);

            if (avaliableSpeedup)
            {
                count = Time.realtimeSinceStartup - count;
                Debug.Log("speedup gpb: " + count);
                avaliableSpeedup = false;
            }
        }
        if (cpu)
        {
            time += Time.deltaTime;
            delta = time - delta;
            for (int i = 0; i < spheres.Length; i++)
            {
                if (!finished[i])
                {
                    float f = spheres[i].weight * (spheres[i].speed / time);
                    float speed = (f / spheres[i].weight) * delta;
                    Vector3 p = this.gobjSpheres[i].transform.position;
                    if (p.y - spheres[i].speed * delta - spheres[i].radius > floor.transform.position.y)
                    {
                        this.spheres[i].position += new Vector3(0, speed * delta * spheres[i].direction.y, 0);
                        this.spheres[i].color = new Vector3(1, 1, 1);
                    }
                    else
                    {
                        this.spheres[i].position = new Vector3(p.x, floor.transform.position.y + spheres[i].radius, p.z);
                        spheres[i].color = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                        finished[i] = true;
                    }
                }
            }
            if (avaliableSpeedup)
            {
                count = Time.realtimeSinceStartup - count;
                Debug.Log("speedup cpu: " + count);
                avaliableSpeedup = false;
            }
        }
        for (int i = 0; i < spheres.Length; i++)
        {
            Vector3 pos = this.gobjSpheres[i].transform.position;
            this.gobjSpheres[i].transform.position = new Vector3(pos.x, spheres[i].position.y, pos.z);
            Color c = new Color(spheres[i].color.x, spheres[i].color.y, spheres[i].color.z, 1);

            this.gobjSpheres[i].GetComponent<Renderer>().material.color = c;
        }
    }
}
