using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveFrame
{
    public bool isSelected = false;
    public int type = 0;
    public int eggA = 0, eggB = 0;
    public int larvaeA = 0, larvaeB = 0;
    public int pupaA = 0, pupaB = 0;
    public int foodA = 0, foodB = 0;
    public int workersA = 0, workersB = 0;
    public int workersLife = 0;
    public int[,] cellA = new int[40,40];
    public int[,] cellB = new int[40,40];
   
    public HiveFrame()
    {
        //Initial Values of Frames
        /*
            * 0        Dummy
            * 1-3      egg
            * 4-9     larvae
            * 10-21    pupa
            * 22-81    Adult/Worker - Walang Tirahan?
            * 82       Dead 
            * 100      Food
            * 101-114	Foundation
            * 115		Sticky
            * 1000      NaN
        */
        /*
            * 1-Food   			Pollen/Honey  Black
            * 2-Open Brood		Egg/Larvae    Red
            * 3-Sealed Brood	Pupa          Green
            * 4-Sticky			Empty Comb    Blue
            * 5-Foundation		Unbuilt comb  Gold
            * 6-Blocker			Wood
        */

        isSelected = false;
        type = 0;
        eggA = 0;
        eggB = 0;
        larvaeA = 0;
        larvaeB = 0;
        pupaA = 0;
        pupaB = 0;
        foodA = 0;
        foodB = 0;
        workersA = 0;
        workersB = 0;
        workersLife = 0;

        for (int i = 0; i < 40; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                cellA[i,j] = 1000;
                cellB[i,j] = 1000;
            }
        }
    }
}
