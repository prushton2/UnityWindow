//pipeline stuff
using System;
using System.IO;
using System.IO.Pipes;

//unity stuff
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//json stuff
using System.Text.Json;


public class pipeline : MonoBehaviour
{
    private StreamReader sr;
    private NamedPipeClientStream pipeClient;
    private bool isConnected;
    private string temp;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Running");

        pipeClient = new NamedPipeClientStream(".", "FaceRecogServer", PipeDirection.In);

            // Connect to the pipe or wait until the pipe is available.
        Debug.Log("Attempting to connect to pipe...");
        pipeClient.Connect();
        Debug.Log("Connected to pipe.");

        Debug.Log("There are currently "+pipeClient.NumberOfServerInstances+" pipe server instances open.");
        sr = new StreamReader(pipeClient);

        isConnected = true;
    }

    // Update is called once per frame
    void Update()
    {
        temp = sr.ReadLine();
        if( temp != null && isConnected ) {
            // Debug.Log("Received from server: "+temp);
            string[] array = temp.Split(",");
            HeadPos head = new HeadPos(
                float.Parse(array[0]),
                float.Parse(array[1]),
                float.Parse(array[2]),
                float.Parse(array[3]));

            setNewPosition(head);

        } else {
            isConnected = false;
        }

        if(!isConnected) {
            Debug.Log("Exited the pipeline");
        }
    }

    void setNewPosition(HeadPos headPos) {

        /*
        calibrate size to get the distance from the camera
        with distance, you can properly calculate the angle from the camera center
        use distance and angle to calculate x,y
        */

        transform.position = new Vector3(
            (headPos.x / 50.0f) - 5.0f,
            (headPos.y / 50.0f) - 6.2f,
            headPos.w
        );

    }
}

class HeadPos {
    public float x;
    public float y;
    public float w;
    public float h;

    public HeadPos(float x, float y, float w, float h) {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }
}