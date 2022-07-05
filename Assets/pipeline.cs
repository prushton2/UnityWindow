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
    
    public string temp;


    public float calibrationDistance = 465;   //millimeters
    public float calibrationWidth = 200;      //width of your head in pixels at the calibration distance
    public float calibrationHeight = 200;     //height of your head in pixels at the calibration distance

    public float headWidth = 144; //width of your head in mm
    private HeadPos head;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Running");

        //instantiate head with the globals it needs for math
        head = new HeadPos(
            calibrationDistance,
            calibrationWidth,
            calibrationHeight,
            headWidth
        );


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

            head.setPos(
                float.Parse(array[0]),
                float.Parse(array[1]),
                float.Parse(array[2]),
                float.Parse(array[3]));

            head.getXPosition();

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
            head.getXPosition(),
            (headPos.y / 50.0f) - 6.2f,
            head.getHeadDistance()
        );

    }
}

class HeadPos {
    public float x;
    public float y;
    public float w;
    public float h;

    public float calibrationDistance;
    public float calibrationSize;
    public float calibrationWidth;
    public float calibrationHeight;
    public float headWidth;

    public HeadPos(float calibrationDistance, float calibrationWidth, float calibrationHeight, float headWidth) {
        this.calibrationDistance = calibrationDistance;
        this.calibrationWidth = calibrationWidth;
        this.calibrationHeight = calibrationHeight;
        this.headWidth = headWidth;

        this.calibrationSize = calibrationHeight * calibrationWidth;
    }

    public void setPos(float x, float y, float w, float h) {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;    
    }

    public float getHeadSize() {
        return this.w * this.h;
    }

    public float getHeadDistance() {
        return (this.calibrationSize / getHeadSize()) * calibrationDistance;
    }

    public float getXPosition() {
        //we measure distance from the center in heads (kinda)
        float MillsPerPixel =  this.headWidth / this.w; //how many pixels is a mm?
        float distanceToCenter = (this.x - 250) * MillsPerPixel;
        return distanceToCenter;
    }
}