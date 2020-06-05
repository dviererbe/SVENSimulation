﻿using Assets.Scripts.Roomcreation;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Assets.Scripts
{
    class Roomreader
    {
        private static int WALL = 1;
        private static int WINDOW = 2;
        private static int DOOR = 3;
        private static int CHAIR = 0;
        private static int TABLE = 1;


        private static string PATHWALL = "/Roomplan/Walls/Wall";
        private static string PATHWINDOW = "/Roomplan/Walls/Window";
        private static string PATHDOOR = "/Roomplan/Walls/Door";
        private static string PATHCHAIR = "/Roomplan/Furniture/Chair";
        private static string PATHTABLE = "/Roomplan/Furniture/Table";
        private static string PATHROOMHEIGTH = "/Roomplan/Room/Height";
        private static string PATHROOMWIDTH = "/Roomplan/Room/Width";
        private static string PATHSTARTHEIGTH = "./StartPosition/Height";
        private static string PATHSTARTWIDTH = "./StartPosition/Width";
        private static string PATHENDHEIGTH = "./EndPosition/Height";
        private static string PATHENDWIDTH = "./EndPosition/Width";
        private static string PATHHEIGHT = "./Position/Height";
        private static string PATHWIDTH = "./Position/Width";
        private static string PATHROTATION= "./Rotation";
        private static string PATHTYPE= "./Type";

        private string _xmlPath;

        public Roomreader(string xmlPath)
        {
            _xmlPath = xmlPath;
        }

        public Roomobjects[] ReadRoom(out int[,] walls, out int height, out int width)
        {
            Debug.Log("Enter");

            //StreamReader str = new StreamReader(_xmlPath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_xmlPath);

            XmlNode root = xmlDoc.DocumentElement;

            Debug.Log("Load File");

            #region Roomsize
            {

                //read Roomsize
                XmlNode heightNode = root.SelectSingleNode(PATHROOMHEIGTH);
                height = int.Parse(heightNode.InnerText);

                XmlNode widthNode = root.SelectSingleNode(PATHROOMWIDTH);
                width = int.Parse(widthNode.InnerText);
            }
            #endregion

            #region Walls
            walls = new int[height, width];

            Debug.Log("Heigth: " + height);
            Debug.Log("Width: " + width);

            XmlNodeList wallsNodeList = xmlDoc.SelectNodes(PATHWALL);
            walls = readWallElements(walls, wallsNodeList, WALL);
            #endregion

            List<Roomobjects> roomobjects = new List<Roomobjects>();

            #region Windows and Doors

            XmlNodeList windows = root.SelectNodes(PATHWINDOW);
            walls = readWallElements(walls, windows, WINDOW);

            XmlNodeList doors = root.SelectNodes(PATHDOOR);
            walls = readWallElements(walls, doors, DOOR);

            #endregion

            #region furniture

            XmlNodeList chairs = root.SelectNodes(PATHCHAIR);
            roomobjects = readRoomobjects(roomobjects, chairs, CHAIR);

            XmlNodeList tables = root.SelectNodes(PATHTABLE);
            roomobjects = readRoomobjects(roomobjects, tables, TABLE);

            #endregion
            return roomobjects.Count == 0 ? new Roomobjects[0] : roomobjects.ToArray();

        }

        private int[,] readWallElements(int[,] walls, XmlNodeList elementList, int type)
        {
            if (elementList != null)
            {
                foreach (XmlNode wallElement in elementList)
                {
                    XmlNode posValue = wallElement.SelectSingleNode(PATHSTARTHEIGTH);
                    int startheight = int.Parse(posValue.InnerText);

                    posValue = wallElement.SelectSingleNode(PATHSTARTWIDTH);
                    int startwidth = int.Parse(posValue.InnerText);

                    posValue = wallElement.SelectSingleNode(PATHENDHEIGTH);
                    int endheight = int.Parse(posValue.InnerText);

                    posValue = wallElement.SelectSingleNode(PATHENDWIDTH);
                    int endwidth = int.Parse(posValue.InnerText);


                    Debug.Log("SHeigth: " + startheight);
                    Debug.Log("SWidth: " + startwidth);
                    Debug.Log("EHeigth: " + endheight);
                    Debug.Log("EWidth: " + endwidth);

                    //darstellung von geraden wänden nur möglich
                    if (endheight == startheight)
                    {
                        //Tauschen der Psozionsdaten falls falsch eingetragen
                        if (startwidth > endwidth)
                        {
                            int zw = startwidth;
                            startwidth = endwidth;
                            endwidth = zw;
                        }

                        for (int i = startwidth; i <= endwidth; i++)
                        {
                            walls[startheight, i] = type;
                        }
                    }
                    else if (startwidth == endwidth)
                    {
                        //Tauschen der Psozionsdaten falls falsch eingetragen
                        if (startheight > endheight)
                        {
                            int zw = startwidth;
                            startwidth = endwidth;
                            endwidth = zw;
                        }

                        for (int i = startheight; i <= endheight; i++)
                        {
                            walls[i, startwidth] = type;
                        }
                    }
                    else
                    {
                        //TODO schräge wände
                    }
                }
            }
            return walls;
        }

        private List<Roomobjects> readRoomobjects(List<Roomobjects> roomobjects, XmlNodeList elementList, int type)
        {
            if(elementList != null)
            {
                foreach(XmlNode element in elementList)
                {
                    Roomobjects newobject = new Roomobjects();
                    newobject.Element = type;

                    XmlNode pos = element.SelectSingleNode(PATHHEIGHT);
                    newobject.PosY = int.Parse(pos.InnerText);

                    pos = element.SelectSingleNode(PATHWIDTH);
                    newobject.PosX = int.Parse(pos.InnerText);

                    pos = element.SelectSingleNode(PATHROTATION);
                    newobject.Rotation = float.Parse(pos.InnerText);

                    pos = element.SelectSingleNode(PATHTYPE);
                    newobject.Type = pos.InnerText;

                    roomobjects.Add(newobject);
                }
            }

            return roomobjects;
        }
    }
}